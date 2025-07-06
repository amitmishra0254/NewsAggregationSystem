using NewsAggregationSystem.Common.Enums;

namespace NewsAggregationSystem.API.Adapters.Interfaces
{
    public interface INewsApiAdapterFactory
    {
        INewsApiAdapter CreateAdapter(NewsSourcesType newsSourceType);
        INewsApiAdapter CreateAdapter(string adapterName);
    }
} 