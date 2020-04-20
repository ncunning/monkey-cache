using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Realms;

namespace MonkeyCache.Realm
{
	public class Barrel : IBarrel
	{
		public static string ApplicationId { get; set; } = string.Empty;
		public static string EncryptionKey { get; set; } = string.Empty;
		public bool AutoExpire { get; set; }

		/// <summary>
		/// Gets the instance of the Barrel
		/// </summary>
		public static IBarrel Current => (instance ?? (instance = new Barrel()));

		public static IBarrel Create(string cacheDirectory)
			=> new Barrel(cacheDirectory);

		private static readonly Lazy<string> baseCacheDir = new Lazy<string>(() =>
		   {
			   return Path.Combine(BarrelUtils.GetBasePath(ApplicationId), "MonkeyCache");
		   });

		private readonly RealmConfiguration configuration;

		private JsonSerializerSettings jsonSettings;

		private static Barrel instance = null;

		public Barrel(string cacheDirectory = null)
		{
			var directory = string.IsNullOrEmpty(cacheDirectory) ? baseCacheDir.Value : cacheDirectory;
			var path = Path.Combine(directory, "Barrel.realm");
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			// NOTE : If you want to open this database in Realm Studio, HEX Encode this key (appIdHash) from : http://www.convertstring.com/EncodeDecode/HexEncode
			// and use that 128 charecter HEX string while opening the database.
			configuration = new RealmConfiguration(path)
			{
				//Not encrypting the databse while in DEBUG mode, so that it can be opened in Windows machines using Realm Studio.

				// Used to handle migrations, if you need to alter schema in situ
				SchemaVersion = 1,

				// todo: once data is saved in realm that release customers can
				// see this will need to be moved behind the debug flag and
				// migrations handled properly
				ShouldDeleteIfMigrationNeeded = true,
			};

			if (!string.IsNullOrWhiteSpace(EncryptionKey))
				// AES-256 bit encryption, just like that
				configuration.EncryptionKey = System.Text.Encoding.ASCII.GetBytes(EncryptionKey);

			jsonSettings = new JsonSerializerSettings
			{
				ObjectCreationHandling = ObjectCreationHandling.Replace,
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				TypeNameHandling = TypeNameHandling.All,
			};
		}

		private Realms.Realm GetRealm() => Realms.Realm.GetInstance(configuration);

		#region Add Methods

		/// <summary>
		/// Adds a string netry to the barrel
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">Unique identifier for the entry</param>
		/// <param name="data">Data string to store</param>
		/// <param name="expireIn">Time from UtcNow to expire entry in</param>
		/// <param name="eTag">Optional eTag information</param>
		public void Add(string key, string data, TimeSpan expireIn, string eTag = null)
		{
			if (data == null)
				return;

			var realm = GetRealm();

			realm.Write(() =>
			{
				var found = realm.Find<Banana>(key);

				if (found != null)
				{
					found.Id = key;
					found.ExpirationDate = BarrelUtils.GetExpirationOffset(expireIn);
					found.ETag = eTag;
					found.Contents = data;
				}
				else
				{
					var ent = new Banana
					{
						Id = key,
						ExpirationDate = BarrelUtils.GetExpirationOffset(expireIn),
						ETag = eTag,
						Contents = data
					};

					realm.Add(ent);
				}
			});
		}

		/// <summary>
		/// Adds an entry to the barrel
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">Unique identifier for the entry</param>
		/// <param name="data">Data object to store</param>
		/// <param name="expireIn">Time from UtcNow to expire entry in</param>
		/// <param name="eTag">Optional eTag information</param>
		public void Add<T>(string key, T data, TimeSpan expireIn, string eTag = null, JsonSerializerSettings jsonSerializationSettings = null)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("Key can not be null or empty.", nameof(key));

			if (data == null)
				throw new ArgumentNullException("Data can not be null.", nameof(data));

			var dataJson = string.Empty;

			if (BarrelUtils.IsString(data))
			{
				dataJson = data as string;
			}
			else
			{
				dataJson = JsonConvert.SerializeObject(data, jsonSerializationSettings ?? jsonSettings);
			}

			Add(key, dataJson, expireIn, eTag);
		}

		#endregion Add Methods

		#region Exist and Expiration Methods

		/// <summary>
		/// Checks to see if the key exists in the Barrel.
		/// </summary>
		/// <param name="key">Unique identifier for the entry to check</param>
		/// <returns>If the key exists</returns>
		public bool Exists(string key)
		{
			var realm = GetRealm();
			var ent = realm.Find<Banana>(key);

			return ent != null;
		}

		/// <summary>
		/// Checks to see if the entry for the key is expired.
		/// </summary>
		/// <param name="key">Key to check</param>
		/// <returns>If the expiration data has been met</returns>
		public bool IsExpired(string key)
		{
			var realm = GetRealm();
			var ent = realm.Find<Banana>(key);

			if (ent == null)
				return true;

			return DateTimeOffset.Now > ent.ExpirationDate;
		}

		#endregion Exist and Expiration Methods

		#region Get Methods

		/// <summary>
		/// Gets the data entry for the specified key.
		/// </summary>
		/// <param name="key">Unique identifier for the entry to get</param>
		/// <returns>The data object that was stored if found, else default(T)</returns>
		public T Get<T>(string key, JsonSerializerSettings jsonSerializationSettings = null)
		{
			var realm = GetRealm();
			var ent = realm.Find<Banana>(key);

			var result = default(T);

			if (ent == null)
				return default(T);

			if (ent == null || (AutoExpire && IsExpired(key)))
				return result;

			if (BarrelUtils.IsString(result))
			{
				object final = ent.Contents;
				return (T)final;
			}

			return JsonConvert.DeserializeObject<T>(ent.Contents, jsonSerializationSettings ?? jsonSettings);
		}

		/// <summary>
		/// Gets the string entry for the specified key.
		/// </summary>
		/// <param name="key">Unique identifier for the entry to get</param>
		/// <returns>The string that was stored if found, else null</returns>
		public string Get(string key)
		{
			var realm = GetRealm();
			var ent = realm.Find<Banana>(key);

			if (ent == null)
				return null;

			return ent.Contents;
		}

		/// <summary>
		/// Gets the ETag for the specified key.
		/// </summary>
		/// <param name="key">Unique identifier for entry to get</param>
		/// <returns>The ETag if the key is found, else null</returns>
		public string GetETag(string key)
		{
			var realm = GetRealm();
			var ent = realm.Find<Banana>(key);

			if (ent == null)
				return null;

			return ent.ETag;
		}

		public DateTimeOffset? GetExpiration(string key, bool isRealm)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("Key can not be null or empty.", nameof(key));

			var realm = GetRealm();
			var ent = realm.Find<Banana>(key);

			if (ent == null)
				return null;

			return ent.ExpirationDate;
		}

		#endregion Get Methods

		#region Empty Methods

		/// <summary>
		/// Empties all expired entries that are in the Barrel.
		/// Throws an exception if any deletions fail and rolls back changes.
		/// </summary>
		public void EmptyExpired()
		{
			var now = DateTimeOffset.Now;
			var realm = GetRealm();
			realm.Write(() =>
			{
				realm.RemoveRange(realm.All<Banana>().Where(b => b.ExpirationDate < now));
			});
		}

		/// <summary>
		/// Empties all expired entries that are in the Barrel.
		/// Throws an exception if any deletions fail and rolls back changes.
		/// </summary>
		public void EmptyAll()
		{
			var realm = GetRealm();
			realm.Write(() => realm.RemoveAll<Banana>());
		}

		/// <summary>
		/// Empties all specified entries regardless if they are expired.
		/// Throws an exception if any deletions fail and rolls back changes.
		/// </summary>
		/// <param name="key">keys to empty</param>
		public void Empty(params string[] key)
		{
			var realm = GetRealm();
			realm.Write(() =>
			{
				foreach (var item in key)
				{
					var foundItem = realm.Find<Banana>(item);
					if (foundItem != null)
						realm.Remove(foundItem);
				}
			});
		}

		public IEnumerable<string> GetKeys(CacheState state = CacheState.Active)
		{
			var realm = GetRealm();
			var allBananas = realm.All<Banana>();

			if (allBananas != null)
			{
				var bananas = new List<Banana>();

				if (state.HasFlag(CacheState.Active))
				{
					try
					{
						foreach (var banana in allBananas)
						{
							if (GetExpiration(banana.Id) >= DateTime.UtcNow)
								bananas.Add(banana);
						}
					}
					catch (Exception ex)
					{
						var message = ex.Message;
					}
				}

				if (state.HasFlag(CacheState.Expired))
				{
					foreach (var banana in allBananas)
					{
						if (GetExpiration(banana.Id) < DateTime.UtcNow)
							bananas.Add(banana);
					}
				}

				return bananas.Select(x => x.Id);
			}

			return new string[0];
		}

		public DateTime? GetExpiration(string key)
		{
			var dto = GetExpiration(key, true);
			return DateTimeExtensions.ConvertFromDateTimeOffset(dto);
		}

		#endregion Empty Methods
	}
}