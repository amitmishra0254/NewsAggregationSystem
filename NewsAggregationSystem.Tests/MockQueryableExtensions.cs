using Microsoft.EntityFrameworkCore;
using Moq;

namespace NewsAggregationSystem.Tests
{

    public static class MockQueryableExtensions
    {
        public static Mock<DbSet<T>> AsMockDbSet<T>(this IQueryable<T> source) where T : class
        {
            return source.BuildMockDbSet();
        }

        public static Mock<DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> source) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IAsyncEnumerable<T>>()
                   .Setup(m => m.GetAsyncEnumerator(default))
                   .Returns(new TestAsyncEnumerator<T>(source.GetEnumerator()));

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(source.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(source.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(source.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => source.GetEnumerator());

            return mockSet;
        }
    }
}
