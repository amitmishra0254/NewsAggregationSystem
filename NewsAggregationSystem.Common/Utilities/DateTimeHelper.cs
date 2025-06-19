namespace NewsAggregationSystem.Common.Utilities
{
    public sealed class DateTimeHelper
    {
        private DateTimeHelper() { }

        private static DateTimeHelper _dateTimeHelper;
        private static readonly object _lock = new object();

        public static DateTimeHelper GetInstance()
        {
            if (_dateTimeHelper == null)
            {
                lock (_lock)
                {
                    if (_dateTimeHelper == null)
                    {
                        _dateTimeHelper = new DateTimeHelper();
                    }
                }
            }
            return _dateTimeHelper;
        }

        public DateTime CurrentUtcDateTime
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        public DateTime GetMinUtcDate
        {
            get
            {
                return DateTime.MinValue;
            }
        }

        public DateTime GetCurrentSystemDateTime
        {
            get
            {
                return DateTime.Now;
            }
        }
    }
}