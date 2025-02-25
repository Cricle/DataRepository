﻿using DataRepository.Casing.Models;
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
        private readonly Mock<IValuePublisher<Student>> valuePublisherMock = new();

        [Fact]
        public void GivenNull_MustThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RedisHashCasingNewest<Student>(null!, newestValueConverterMock.Object, loggerMock.Object, valuePublisherMock.Object));
            Assert.Throws<ArgumentNullException>(() => new RedisHashCasingNewest<Student>(connectionMultiplexerMock.Object, null!, loggerMock.Object, valuePublisherMock.Object));
            Assert.Throws<ArgumentNullException>(() => new RedisHashCasingNewest<Student>(connectionMultiplexerMock.Object, newestValueConverterMock.Object, null!, valuePublisherMock.Object));
            Assert.Throws<ArgumentNullException>(() => new RedisHashCasingNewest<Student>(connectionMultiplexerMock.Object, newestValueConverterMock.Object, loggerMock.Object, null!));
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
        public async Task SetAsync_MustBeSetInRedis(string key, string convertValue, TimedResult<Student> newestResult)
        {
            RedisValue value = convertValue;
            newestValueConverterMock.Setup(x => x.Convert(newestResult.Value)).Returns(value);

            var sut = GetSut();
            await sut.SetAsync(key, newestResult);

            HashEntry[] entities = [
                new HashEntry("t",newestResult.UnixTimeMilliseconds),
                new HashEntry("v",convertValue)
            ];
            connectionMultiplexerMock.VerifyOnceNoOthersCall(x => x.GetDatabase(-1, null).HashSetAsync(key, entities, CommandFlags.None));
        }

        [Theory, AutoData]
        public async Task Exists_UseRedisExists(string key)
        {

            var sut = GetSut();
            await sut.ExistsAsync(key);

            connectionMultiplexerMock.VerifyOnceNoOthersCall(x => x.GetDatabase(-1, null).KeyExistsAsync(key, CommandFlags.None));
        }

        private RedisHashCasingNewest<Student> GetSut()
        {
            connectionMultiplexerMock.Setup(x => x.GetDatabase(-1, null)).Returns(databaseMock.Object);
            connectionMultiplexerMock.Setup(x => x.GetServers()).Returns([serverMock.Object]);
            return new RedisHashCasingNewest<Student>(connectionMultiplexerMock.Object, newestValueConverterMock.Object, loggerMock.Object, valuePublisherMock.Object); ;
        }
    }
}
