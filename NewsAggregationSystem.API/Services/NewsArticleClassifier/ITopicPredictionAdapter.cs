namespace NewsAggregationSystem.API.Services.NewsArticleClassifier
{
    public interface ITopicPredictionAdapter
    {
        Task<string> PredictTopicAsync(string text);
    }
}
