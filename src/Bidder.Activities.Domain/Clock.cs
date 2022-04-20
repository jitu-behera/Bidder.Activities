using System;

namespace Bidder.Activities.Domain
{
    public class Clock
    {
        public virtual DateTime GetCurrentUtcDateTime()
        {
            return DateTime.UtcNow;
        }
    }
}