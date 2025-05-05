using System.Diagnostics.CodeAnalysis;

namespace DataRepository.Bus.Test
{
    public class BusBaseTest
    {
        [Fact]
        public async Task StartAndPublish()
        {
            var bus = new BusBaseMock();
            bus.IsStarted.Should().BeFalse();

            await bus.StartAsync();
            bus.IsStarted.Should().BeTrue();
            await bus.StopAsync();
            bus.IsStarted.Should().BeFalse();

            for (int i = 0; i < 10; i++)
            {
                await bus.PublishAsync(i);
            }
            bus.PublishCount.Should().Be(10);

            await bus.StartAsync();
            bus.Dispose();
            bus.IsStarted.Should().BeFalse();

            await bus.StartAsync();
            await bus.DisposeAsync();
            bus.IsStarted.Should().BeFalse();
        }


        [ExcludeFromCodeCoverage]
        private class BusBaseMock: BusBase
        {
            public int PublishCount = 0;

            public override Task PublishAsync<[DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] TMessage>(TMessage message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default)
            {
                PublishCount++;
                return Task.CompletedTask;
            }

            public override Task<TReply> RequestAsync<[DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] TRequest, [DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] TReply>(TRequest message, IDictionary<string, object?>? header = null, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(default(TReply))!;
            }

            protected internal override Task OnStartAsync(CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            protected internal override Task OnStopAsync(CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }
    }
}
