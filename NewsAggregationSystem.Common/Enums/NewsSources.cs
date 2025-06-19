using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAggregationSystem.Common.Enums
{
    public enum NewsSourcesType
    {
        [Description("The News API")]
        TheNewsApi = 1,
        [Description("News API")]
        NewsApi = 2
    }
}
