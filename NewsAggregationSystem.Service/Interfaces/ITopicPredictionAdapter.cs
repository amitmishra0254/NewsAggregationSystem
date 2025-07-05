namespace NewsAggregationSystem.Service.Interfaces
{
    public interface ITopicPredictionAdapter
    {
        Task<string> PredictTopicAsync(string text);
        Task<int> ResolveCategory(string resolvedCategory);
    }
}
