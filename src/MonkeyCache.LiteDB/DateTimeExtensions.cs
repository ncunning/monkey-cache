using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyCache.LiteDB
{
	public static class DateTimeExtensions
	{
		public static long ToMillisUnixTimestamp(this DateTime dateTime)
		{
			return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
		}

		public static DateTime MillisUnixTimestampFromDateTime(long unixTimeStamp)
		{
			// Unix timestamp is seconds past epoch
			var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dateTime = dateTime.AddMilliseconds(unixTimeStamp);
			return dateTime.ToLocalTime();
		}
	}
}