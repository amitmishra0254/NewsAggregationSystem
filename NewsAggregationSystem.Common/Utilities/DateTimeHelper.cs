using NewsAggregationSystem.Common.DTOs;
using System.Globalization;

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

        public DateRangeDTO DateValidator()
        {
            DateTime fromDate;
            DateTime toDate;
            Console.Write("Enter From Date (yyyy-MM-dd): ");
            var fromInput = Console.ReadLine();

            Console.Write("Enter To Date (yyyy-MM-dd): ");
            var toInput = Console.ReadLine();

            bool isFromValid = DateTime.TryParseExact(fromInput, "yyyy-MM-dd", null, DateTimeStyles.None, out fromDate);
            bool isToValid = DateTime.TryParseExact(toInput, "yyyy-MM-dd", null, DateTimeStyles.None, out toDate);

            if (!isFromValid || !isToValid)
            {
                Console.WriteLine("Invalid date format. Please use yyyy-MM-dd.\n");
                return null;
            }

            if (fromDate >= toDate)
            {
                Console.WriteLine("From Date must be earlier than To Date.\n");
                return null;
            }
            return new DateRangeDTO { FromDate = fromDate, ToDate = toDate };
        }
    }
}