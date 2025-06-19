using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAggregationSystem.Common.Exceptions
{
    public class NewsSourceFetchFailedException : Exception
    {
        public int NewsSourceId { get; }

        public NewsSourceFetchFailedException(int newsSourceId, string message)
            : base(message)
        {
            NewsSourceId = newsSourceId;
        }

        public NewsSourceFetchFailedException(int newsSourceId, string statusCode, string message)
            : base(message)
        {
            NewsSourceId = newsSourceId;
        }
    }
}
