using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DataRepository.Casing.Redis.Test
{
    public class RedisHashNewestTest
    {
        private readonly Mock<IConnectionMultiplexer> connectionMultiplexerMock = new();
        private readonly Mock<INewestValueConverter<Student>> newestValueConverterMock = new();
        private readonly Mock<ILogger<RedisHashNewest<Student>>> loggerMock = new();
        private readonly Mock<IValuePublisher<Student>> valuePublisherMock = new();

        [Fact]
        public void GivenNull_MustThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RedisHashNewest<Student>(null!, newestValueConverterMock.Object, loggerMock.Object, valuePublisherMock.Object));
            Assert.Throws<ArgumentNullException>(() => new RedisHashNewest<Student>(connectionMultiplexerMock.Object, null!, loggerMock.Object, valuePublisherMock.Object));
            Assert.Throws<ArgumentNullException>(() => new RedisHashNewest<Student>(connectionMultiplexerMock.Object, newestValueConverterMock.Object, null!, valuePublisherMock.Object));
            Assert.Throws<ArgumentNullException>(() => new RedisHashNewest<Student>(connectionMultiplexerMock.Object, newestValueConverterMock.Object, loggerMock.Object, null!));
        }
    }
}
