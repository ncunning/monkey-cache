using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyCache.FileStore
{
	/// <summary>
	/// Data object for Barrel
	/// </summary>
	public class Banana

	{
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
		/// Expiration data of the object, stored in UTC
		/// </summary>
		public DateTime ExpirationDate { get; set; }
	}
}