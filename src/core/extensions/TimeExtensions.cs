namespace CCXT.Simple.Core.Extensions
{
    /// <summary>
    ///
    /// </summary>
    public static class TimeExtensions
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private static DateTime DateTimeUnixEpochStart { get; } = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// January 1, 1970 00:00:00 Coordinated Universal Time (UTC)
        /// </summary>
        public static DateTime UnixEpoch
        {
            get
            {
                return DateTimeUnixEpochStart;
            }
        }

        /// <summary>
        /// Gets the current UTC time as Unix epoch milliseconds.
        /// </summary>
        public static long UnixTimeMillisecondsNow => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        /// <summary>
        /// Gets the current UTC time as Unix epoch seconds.
        /// </summary>
        public static long UnixTimeSecondsNow => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        /// <summary>
        /// Gets the current UTC time as Unix epoch milliseconds (legacy property for backward compatibility).
        /// </summary>
        public static long UnixTime => UnixTimeMillisecondsNow;

        /// <summary>
        /// The elapsed time from the current UTC (Coordinated Universal Time) of this computer, converted to seconds as an integer
        /// </summary>
        public static Int64 Now
        {
            get
            {
                return ConvertToUnixTime(UtcNow);
            }
        }

        /// <summary>
        /// The elapsed time from the current UTC (Coordinated Universal Time) of this computer, converted to milliseconds as an integer
        /// </summary>
        public static Int64 NowMilli
        {
            get
            {
                return ConvertToUnixTimeMilli(UtcNow);
            }
        }

        /// <summary>
        /// Gets a System.DateTime object that is set to the current date and time on this computer, expressed as the Coordinated Universal Time (UTC).
        /// </summary>
        public static DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Gets a System.DateTime object that is set to the current date and time on this computer, expressed as the local time.
        /// </summary>
        public static DateTime LocalNow
        {
            get
            {
                return DateTime.Now;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public static DateTime MaxValue
        {
            get
            {
                return DateTime.MaxValue.ToUniversalTime();
            }
        }

        /// <summary>
        ///
        /// </summary>
        public static DateTime MinValue
        {
            get
            {
                return DateTime.MinValue.ToUniversalTime();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // UnixTime - unit32, 64
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// The elapsed time from January 1, 1970 00:00:00 Coordinated Universal Time (UTC), converted to seconds as an integer
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static Int64 ConvertToUnixTime(DateTime datetime)
        {
            return (Int64)((datetime.ToUniversalTime() - UnixEpoch).TotalSeconds);
        }

        /// <summary>
        /// The elapsed time from January 1, 1970 00:00:00 Coordinated Universal Time (UTC), converted to milliseconds as an integer
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static Int64 ConvertToUnixTimeMilli(DateTime datetime)
        {
            return (Int64)((datetime.ToUniversalTime() - UnixEpoch).TotalMilliseconds);
        }

        /// <summary>
        /// Converts a string to datetime and then converts UTC to seconds.
        /// </summary>
        /// <param name="timeString"></param>
        /// <returns></returns>
        public static Int64 ConvertToUnixTime(string timeString)
        {
            return ConvertToUnixTime(ConvertToUtcTime(timeString));
        }

        /// <summary>
        /// Converts a string to datetime and then converts UTC to milliseconds.
        /// </summary>
        /// <param name="timeString"></param>
        /// <returns></returns>
        public static Int64 ConvertToUnixTimeMilli(string timeString)
        {
            return ConvertToUnixTimeMilli(ConvertToUtcTime(timeString));
        }

        /// <summary>
        /// Calculates the value by adding the offset time to the current UTC time.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Int64 AddUnixTime(Int64 offset)
        {
            return Now + offset;
        }

        /// <summary>
        /// Calculates the value by adding the timespan seconds to the current UTC time.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Int64 AddUnixTime(TimeSpan offset)
        {
            return AddUnixTime((Int64)offset.TotalSeconds);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="unixTime"></param>
        /// <returns></returns>
        public static Int64 ConvertToSeconds(Int64 unixTime)
        {
            return unixTime > 9999999999 ? unixTime / 1000 : unixTime;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="unixTime"></param>
        /// <returns></returns>
        public static Int64 ConvertToMilliSeconds(Int64 unixTime)
        {
            return unixTime <= 9999999999 ? unixTime * 1000 : unixTime;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // UTC Time
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns a new (UTC) System.DateTime that adds the specified number of seconds.
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static DateTime ConvertToUtcTime(Int64 seconds)
        {
            return UnixEpoch.AddSeconds(seconds).ToUniversalTime();
        }

        /// <summary>
        /// Returns a new (UTC) System.DateTime that adds the specified number of milliseconds.
        /// </summary>
        /// <param name="milliSeconds"></param>
        /// <returns></returns>
        public static DateTime ConvertToUtcTimeMilli(Int64 milliSeconds)
        {
            return UnixEpoch.AddMilliseconds(milliSeconds).ToUniversalTime();
        }

        /// <summary>
        /// Returns a new (UTC) System.DateTime that adds the timespan to the specified seconds.
        /// </summary>
        /// <param name="unixtime"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static DateTime ConvertToUtcTime(Int64 unixtime, TimeSpan offset)
        {
            return ConvertToUtcTime(unixtime) + offset;
        }

        /// <summary>
        /// Returns a new (UTC) System.DateTime that adds the timespan to the specified milliseconds.
        /// </summary>
        /// <param name="unixtime"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static DateTime ConvertToUtcTimeMilli(Int64 unixtime, TimeSpan offset)
        {
            return ConvertToUtcTimeMilli(unixtime) + offset;
        }

        /// <summary>
        /// Returns a string as UTC. (The string must always include the timezone format.)
        /// </summary>
        /// <param name="timeWithZone"></param>
        /// <example>"2015-04-20T15:49:46.427+09:00"</example>
        /// <returns></returns>
        public static DateTime ConvertToUtcTime(string timeWithZone)
        {
            return Convert.ToDateTime(timeWithZone).ToUniversalTime();
        }

        /// <summary>
        /// Returns the UTC time corresponding to the UTC value as a string format.
        /// </summary>
        /// <param name="unixtime"></param>
        /// <returns></returns>
        public static string ToUtcTimeString(Int64 unixtime)
        {
            return ConvertToUtcTime(unixtime).ToDateTimeZoneString();
        }

        /// <summary>
        /// Returns the local time corresponding to the UTC value as a string format.
        /// </summary>
        /// <param name="unixTimeMilli"></param>
        /// <returns></returns>
        public static string ToUtcTimeMilliString(Int64 unixTimeMilli)
        {
            return ConvertToLocalTimeMilli(unixTimeMilli).ToDateTimeZoneString();
        }

        /// <summary>
        /// Returns the value corresponding to datetime as a string format.
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static string ToUtcTimeString(DateTime datetime)
        {
            return datetime.ToUniversalTime().ToDateTimeZoneString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Local Time
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the local time corresponding to the UTC value as a datetime format.
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static DateTime ConvertToLocalTime(Int64 seconds)
        {
            return ConvertToUtcTime(seconds).ToLocalTime();
        }

        /// <summary>
        /// Returns the local time corresponding to the UTC value as a datetime format.
        /// </summary>
        /// <param name="milliSeconds"></param>
        /// <returns></returns>
        public static DateTime ConvertToLocalTimeMilli(Int64 milliSeconds)
        {
            return ConvertToUtcTimeMilli(milliSeconds).ToLocalTime();
        }

        /// <summary>
        /// Returns a string as local time. (The string must always include the timezone format.)
        /// </summary>
        /// <param name="timeWithZone"></param>
        /// <example>"2015-04-20T15:49:46.427+09:00"</example>
        /// <returns></returns>
        public static DateTime ConvertToLocalTime(string timeWithZone)
        {
            return ConvertToUtcTime(timeWithZone).ToLocalTime();
        }

        /// <summary>
        /// Returns the local time corresponding to the UTC value as a string format.
        /// </summary>
        /// <param name="unixtime"></param>
        /// <returns></returns>
        public static string ToLocalTimeString(Int64 unixtime)
        {
            return ConvertToLocalTime(unixtime).ToDateTimeString();
        }

        /// <summary>
        /// Returns the local time corresponding to the UTC value as a string format.
        /// </summary>
        /// <param name="unixTimeMilli"></param>
        /// <returns></returns>
        public static string ToLocalTimeMilliString(Int64 unixTimeMilli)
        {
            return ConvertToLocalTimeMilli(unixTimeMilli).ToDateTimeString();
        }

        /// <summary>
        /// Returns the value corresponding to datetime as a string format.
        /// </summary>
        /// <param name="localtime"></param>
        /// <returns></returns>
        public static string ToLocalTimeString(DateTime localtime)
        {
            return localtime.ToLocalTime().ToDateTimeString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // parse
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Checks whether a string can be converted to datetime.
        /// </summary>
        /// <param name="timeString">The string to convert that contains the date and time.</param>
        /// <returns>true if the s parameter is converted; otherwise, false.</returns>
        public static bool IsDateTimeFormat(string timeString)
        {
            return DateTime.TryParse(timeString, out _);
        }

        /// <summary>
        /// Converts the specified string representation of a date and time to its System.DateTime equivalent.
        /// </summary>
        /// <param name="s">The string to convert that contains the date and time.</param>
        /// <returns>An object that is equivalent to the date and time contained in s.</returns>
        public static DateTime Parse(string s)
        {
            return DateTime.Parse(s);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Get the first day of the month for any full date submitted
        /// </summary>
        /// <param name="day_of_target"></param>
        /// <returns></returns>
        public static DateTime GetFirstDayOfMonth(DateTime day_of_target)
        {
            // set return value to the first day of the month
            // for any date passed in to the method

            // create a datetime variable set to the passed in date
            DateTime _firstDay = day_of_target;

            // remove all of the days in the month
            // except the first day and set the
            // variable to hold that date
            _firstDay = _firstDay.AddDays(-(_firstDay.Day - 1));

            // return the first day of the month
            return _firstDay;
        }

        /// <summary>
        /// Get the first day of the month for a month passed by it's integer value
        /// </summary>
        /// <param name="month_of_target"></param>
        /// <returns></returns>
        public static DateTime GetFirstDayOfMonth(int month_of_target)
        {
            // set return value to the last day of the month
            // for any date passed in to the method

            // create a datetime variable set to the passed in date
            var _firstDay = new DateTime(UtcNow.Year, month_of_target, 1).ToUniversalTime();

            // remove all of the days in the month
            // except the first day and set the
            // variable to hold that date
            _firstDay = _firstDay.AddDays(-(_firstDay.Day - 1));

            // return the first day of the month
            return _firstDay;
        }

        /// <summary>
        /// Get the last day of the month for any full date
        /// </summary>
        /// <param name="day_of_target"></param>
        /// <returns></returns>
        public static DateTime GetLastDayOfMonth(DateTime day_of_target)
        {
            // set return value to the last day of the month
            // for any date passed in to the method

            // create a datetime variable set to the passed in date
            DateTime _lastDay = day_of_target;

            // overshoot the date by a month
            _lastDay = _lastDay.AddMonths(1);

            // remove all of the days in the next month
            // to get bumped down to the last day of the
            // previous month
            _lastDay = _lastDay.AddDays(-(_lastDay.Day));

            // return the last day of the month
            return _lastDay;
        }

        /// <summary>
        /// Get the last day of a month expressed by it's integer value
        /// </summary>
        /// <param name="month_of_target"></param>
        /// <returns></returns>
        public static DateTime GetLastDayOfMonth(int month_of_target)
        {
            // set return value to the last day of the month
            // for any date passed in to the method

            // create a datetime variable set to the passed in date
            var _lastDay = new DateTime(UtcNow.Year, month_of_target, 1).ToUniversalTime();

            // overshoot the date by a month
            _lastDay = _lastDay.AddMonths(1);

            // remove all of the days in the next month
            // to get bumped down to the last day of the
            // previous month
            _lastDay = _lastDay.AddDays(-(_lastDay.Day));

            // return the last day of the month
            return _lastDay;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // pure coding
        //-----------------------------------------------------------------------------------------------------------------------------
        private static int[] DaysToMonth365
        {
            get;
        } = new int[]
        {
        0,
        31,
        59,
        90,
        120,
        151,
        181,
        212,
        243,
        273,
        304,
        334,
        365
        };

        private static int[] DaysToMonth366
        {
            get;
        } = new int[]
        {
        0,
        31,
        60,
        91,
        121,
        152,
        182,
        213,
        244,
        274,
        305,
        335,
        366
        };

        /// <summary>
        ///
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public static bool IsLeapYear(int year)
        {
            if (year < 1 || year > 9999)
                throw new ArgumentOutOfRangeException("year", "ArgumentOutOfRange_Year");

            return year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public static int DaysInYear(int year)
        {
            if (year < 1 || year > 9999)
                throw new ArgumentOutOfRangeException("year", "ArgumentOutOfRange_Year");

            var _days = year * 365 + (int)year / 4 - (int)year / 100 + (int)year / 400;
            return _days;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static int DaysInMonth(int year, int month)
        {
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException("month", "ArgumentOutOfRange_Month");

            int[] array = IsLeapYear(year) ? DaysToMonth366 : DaysToMonth365;
            return array[month - 1];
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="days"></param>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static UInt64 GetUnixTimeSecond(int days, int hours, int minutes, int seconds)
        {
            return (UInt64)days * 3600L * 24L + (UInt64)hours * 3600L + (UInt64)minutes * 60L + (UInt64)seconds;
        }

        /*-----------------------------------------------------------------------------------------------------------------------------

            var _now_time = DateTime.UtcNow;

            var _unix_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var _unix_time = (UInt64)((_now_time - _unix_epoch).TotalSeconds);

            var _days = DaysInYear(_now_time.Year - 1) - DaysInYear(1969)
                        + DaysInMonth(_now_time.Year, _now_time.Month)
                        + _now_time.Day - 1;

            var _hours = _now_time.Hour;
            var _minutes = _now_time.Minute;
            var _seconds = _now_time.Second;

            var _calc_time = GetUnixTimeSecond(_days, _hours, _minutes, _seconds);
            if (_unix_time != _calc_time)
                Console.Out.WriteLine(String.Format("different: {0}, {1}", _unix_time, _calc_time));

        -----------------------------------------------------------------------------------------------------------------------------*/
    }
}