using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace DataRepository.Bus.Test
{
    public class BatchBusBaseTest
    {
        private readonly Mock<IServiceScopeFactory> serviceScopeFactoryMock = new();
        private readonly Mock<ILogger> loggerMock = new();
        private readonly Mock<IDispatcherBuilder> dispatcherBuilderMock = new();

        [Fact]
        public void Init_ThrowArgumentNullException_WhenInputNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BatchBusBaseMock(null!, loggerMock.Object, dispatcherBuilderMock.Object));
            Assert.Throws<ArgumentNullException>(() => new BatchBusBaseMock(serviceScopeFactoryMock.Object, null!, dispatcherBuilderMock.Object));
            Assert.Throws<ArgumentNullException>(() => new BatchBusBaseMock(serviceScopeFactoryMock.Object, loggerMock.Object, null!));
        }

        class BatchBusBaseMock : BatchBusBase
        {
            public BatchBusBaseMock(IServiceScopeFactory serviceScopeFactory, ILogger logger, IDispatcherBuilder dispatcherBuilder) : base(serviceScopeFactory, logger, dispatcherBuilder)
            {
            }

            protected internal override Task CorePublishAsync<[DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] TMessage>(TMessage message, IConsumerIdentity identity, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            protected internal override Task<TReply> CoreRequestAsync<[DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] TRequest, [DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] TReply>(TRequest message, IRequestReplyIdentity identity, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(default(TReply))!;
            }
        }
    }
}
