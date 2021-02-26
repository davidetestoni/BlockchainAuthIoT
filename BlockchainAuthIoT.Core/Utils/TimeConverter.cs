using System;
using System.Numerics;

namespace BlockchainAuthIoT.Core.Utils
{
    public static class TimeConverter
    {
        /// <summary>
        /// Converts a unix time to a UTC DateTime.
        /// </summary>
        /// <param name="isMilliseconds">True if the unix time is in milliseconds instead of seconds</param>
        public static DateTime ToDateTimeUtc(this BigInteger unixTime, bool isMilliseconds = false)
        {
            if (!isMilliseconds) unixTime *= 1000;
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                .AddMilliseconds((double)unixTime)
                .ToUniversalTime();
        }

        /// <summary>
        /// Converts a DateTime to a unix time.
        /// </summary>
        /// <param name="outputMilliseconds"></param>
        public static BigInteger ToUnixTime(this DateTime dateTime, bool outputMilliseconds = false)
        {
            TimeSpan dt = dateTime.Subtract(new DateTime(1970, 1, 1));

            return outputMilliseconds
                ? new BigInteger(dt.TotalMilliseconds)
                : new BigInteger(dt.TotalSeconds);
        }

        /// <summary>
        /// Gets a new DateTime equal to the one provided but with milliseconds set to 0.
        /// </summary>
        public static DateTime TrimMilliseconds(this DateTime dt)
            => new(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Kind);
    }
}
