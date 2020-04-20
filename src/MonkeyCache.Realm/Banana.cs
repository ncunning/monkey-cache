using Realms;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyCache.Realm
{
	/// <summary>
	/// Data object for Barrel
	/// </summary>
	public class Banana : RealmObject
	{
		/// <summary>
		/// Unique Identifier
		/// </summary>

		[PrimaryKey]
		public string Id { get; set; }

		/// <summary>
		/// Additional ETag to set for Http Caching
		/// </summary>
		public string ETag { get; set; }

		/// <summary>
		/// Main Contents.
		/// </summary>
		public string Contents { get; set; }

		/// <summary>
		/// Expiration data of the object
		/// </summary>
		public DateTimeOffset ExpirationDate { get; set; }
	}
}