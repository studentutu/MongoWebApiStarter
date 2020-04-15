﻿using NodaTime;
using NodaTime.Text;
using System;

namespace MongoWebApiStarter.Biz
{
    /// <summary>
    /// Utility for converting local DateTime to UTC and vise versa
    /// </summary>
    public static class Dates
    {
        public const string year_month_date = "yyyy-MM-dd";
        public const string hour_minute_am = "hh:mm tt";
        public const string default_timezone = "Asia/Colombo"; // all zones: https://nodatime.org/TimeZones

        /// <summary>
        /// Returns the local date segment "2020-12-31" from a UTC DateTime instance for a given time zone
        /// </summary>
        /// <param name="UTCDateTime">The input UTC DateTime</param>
        /// <param name="timeZone">The time zone to convert the DateTime in to</param>
        public static string ToDatePart(this DateTime UTCDateTime, string timeZone = default_timezone)
        {
            return ToLocal(UTCDateTime, timeZone)
                    .ToString(year_month_date);
        }

        /// <summary>
        /// Returns the local time segment "12:30 AM" from a UTC DateTime instance for a given time zone
        /// </summary>
        /// <param name="UTCDateTime">The input UTC DateTime</param>
        /// <param name="timeZone">The time zone to convert the DateTime in to</param>
        public static string ToTimePart(this DateTime UTCDateTime, string timeZone = default_timezone)
        {
            return ToLocal(UTCDateTime, timeZone)
                    .ToString(hour_minute_am);
        }

        /// <summary>
        /// Converts a UTC DateTime to the given local time zone
        /// </summary>
        /// <param name="UTCDateTime">The input UTC DateTime</param>
        /// <param name="timeZone">The time zone to convert the DateTime in to</param>
        public static DateTime ToLocal(this DateTime UTCDateTime, string timeZone = default_timezone)
        {
            if (UTCDateTime.Kind != DateTimeKind.Utc) throw new ArgumentException("The supplied date must be a UTC date/time");

            return Instant.FromDateTimeUtc(UTCDateTime)
                          .InZone(DateTimeZoneProviders.Tzdb[timeZone])
                          .ToDateTimeUnspecified();
        }

        /// <summary>
        /// Create a UTC DateTime from given date and time strings and the time zone
        /// </summary>
        /// <param name="date">Local date string "2020-12-31"</param>
        /// <param name="time">Local time string "12:12 AM"</param>
        /// <param name="timeZone">The time zone of the local date/time</param>
        public static DateTime ToUTC(string date, string time = "00:00 AM", string timeZone = default_timezone)
        {
            var result = LocalDateTimePattern
                .CreateWithInvariantCulture(
                    $"{year_month_date} {hour_minute_am}")
                .Parse(date + " " + time);

            if (!result.Success) throw new InvalidPatternException(result.Exception.Message);

            return result.Value
                .InZoneStrictly(DateTimeZoneProviders.Tzdb[timeZone])
                .ToDateTimeUtc();
        }
    }
}