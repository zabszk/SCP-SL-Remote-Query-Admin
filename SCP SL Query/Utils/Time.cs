using System;

namespace SCP_SL_Query.Utils
{
    public class Time
    {
        public static long CurrentTimestamp()
        {
            return DateTime.UtcNow.Ticks;
        }

        public static bool ValidateTimestamp(long timestampentry, long timestampexit, long limit)
        {
            return (timestampexit - timestampentry < limit);
        }
    }
}
