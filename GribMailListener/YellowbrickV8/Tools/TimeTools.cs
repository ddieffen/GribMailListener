using System;


namespace YellowbrickV8
{
    public static class TimeTools
    {
        /// <summary>
        /// Returns Gregorian calendar date from UNIX time past Epoch
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }

        /// <summary>
        /// Returns UNIX time past Epoch from Gregorian calendar date
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static double DateTimeToUnixTime(DateTime time)
        {
            return (time - new DateTime(1970, 1, 1)).TotalSeconds;
        }

    }
}
