using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DataRepository.Casing.Redis.Test
{
    public class RedisHashNewestTest
    {
        private readonly Mock<IConnectionMultiplexer> connectionMultiplexerMock = new();
        private readonly Mock<IDatabase> databaseMock = new();
        private readonly Mock<IServer> serverMock = new();
        private readonly Mock<INewestValueConverter<Student>> newestValueConverterMock = new();
        private readonly Mock<ILogger<RedisHashCasingNewest<Student>>> loggerMock = new();
        private readonly RedisHashCasingNewestConfig config = new RedisHashCasingNewestConfig("test");

        [Fact]
        public void GivenNull_MustThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RedisHashCasingNewest<Student>(null!, connectionMultiplexerMock.Object, newestValueConverterMock.Object, loggerMock.Object));
            Assert.Throws<ArgumentNullException>(() => new RedisHashCasingNewest<Student>(config, null!, newestValueConverterMock.Object, loggerMock.Object));
            Assert.Throws<ArgumentNullException>(() => new RedisHashCasingNewest<Student>(config, connectionMultiplexerMock.Object, null!, loggerMock.Object));
            Assert.Throws<ArgumentNullException>(() => new RedisHashCasingNewest<Student>(config, connectionMultiplexerMock.Object, newestValueConverterMock.Object, null!));
        }

        [Fact]
        public async Task Init_MustCallScriptLoad()
        {

            var sut = GetSut();
            await sut.InitAsync();

            sut.scriptToken.Should().NotBeNull();
            connectionMultiplexerMock.VerifyOnceNoOthersCall(x => x.GetServers());
            serverMock.VerifyOnceNoOthersCall(x => x.ScriptLoad(sut.GetScript(), CommandFlags.None));
        }

        [Fact]
        public async Task Init_Mulity_OnlyOneCall()
        {

            var sut = GetSut();
            await sut.InitAsync();
            var token = sut.scriptToken;
            await sut.InitAsync();

            sut.scriptToken.Should().BeEquivalentTo(token);
            connectionMultiplexerMock.VerifyOnceNoOthersCall(x => x.GetServers());
            serverMock.VerifyOnceNoOthersCall(x => x.ScriptLoad(sut.GetScript(), CommandFlags.None));
        }

        [Theory, AutoData]
        public async Task SetAsync_MustBeSetInRedis(string key, string convertValue, Student newestResult)
        {
            RedisValue value = convertValue;
            newestValueConverterMock.Setup(x => x.Convert(newestResult)).Returns(value);

            var sut = GetSut();
            await sut.SetAsync(key, newestResult);

            HashEntry[] entities = [
                new HashEntry("t",new DateTimeOffset(newestResult.Time).ToUnixTimeMilliseconds()),
                new HashEntry("v",convertValue)
            ];
            connectionMultiplexerMock.VerifyOnceNoOthersCall(x => x.GetDatabase(-1, null).HashSetAsync($"test{key}", entities, CommandFlags.None));
        }

        [Theory, AutoData]
        public async Task Exists_UseRedisExists(string key)
        {

            var sut = GetSut();
            await sut.ExistsAsync(key);

            connectionMultiplexerMock.VerifyOnceNoOthersCall(x => x.GetDatabase(-1, null).KeyExistsAsync($"test{key}", CommandFlags.None));
        }

        private RedisHashCasingNewest<Student> GetSut()
        {
            connectionMultiplexerMock.Setup(x => x.GetDatabase(-1, null)).Returns(databaseMock.Object);
            connectionMultiplexerMock.Setup(x => x.GetServers()).Returns([serverMock.Object]);
            return new RedisHashCasingNewest<Student>(config, connectionMultiplexerMock.Object, newestValueConverterMock.Object, loggerMock.Object); ;
        }
    }
}
