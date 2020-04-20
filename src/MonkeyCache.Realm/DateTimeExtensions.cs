using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyCache.Realm
{
	public static class DateTimeExtensions
	{
		public static DateTime? ConvertFromDateTimeOffset(this DateTimeOffset? dateTimeInput)
		{
			var dateTime = (DateTimeOffset)dateTimeInput;
			if (dateTime.Offset.Equals(TimeSpan.Zero))
				return dateTime.UtcDateTime;
			else if (dateTime.Offset.Equals(TimeZoneInfo.Local.GetUtcOffset(dateTime.DateTime)))
				return DateTime.SpecifyKind(dateTime.DateTime, DateTimeKind.Local);
			else
				return dateTime.DateTime;
		}
	}
}