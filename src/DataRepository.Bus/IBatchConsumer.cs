using System.Diagnostics.CodeAnalysis;

namespace DataRepository.Bus
{
    public interface IBatchConsumer : IConsumer
    {
        async Task HandleAsync(BatchMessages<object> messages, CancellationToken cancellationToken = default)
        {
            var buffer = messages.UnsafeGetBuffer();
            for (int i = 0; i < messages.Size; i++)
            {
                await HandleAsync(buffer[i], cancellationToken);

            }
        }
    }

    public interface IBatchConsumer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage> : IBatchConsumer, IConsumer<TMessage>
    {
        async Task HandleAsync(BatchMessages<TMessage> messages, CancellationToken cancellationToken = default)
        {
            var buffer = messages.UnsafeGetBuffer();
            for (int i = 0; i < messages.Size; i++)
            {
                await HandleAsync(buffer[i], cancellationToken);
            }
        }

        async Task IBatchConsumer.HandleAsync(BatchMessages<object> messages, CancellationToken cancellationToken)
        {
            using (var stongBox = StrongBox(messages))
            {
                await HandleAsync(stongBox, cancellationToken);
            }
        }

        private static BatchMessages<TMessage> StrongBox(in BatchMessages<object> messages)
        {
            var newMsg = BatchMessages<TMessage>.Rent(messages.Size);
            var oldSpan = messages.Span;
            var buffer = newMsg.UnsafeGetBuffer();
            for (int i = 0; i < messages.Size; i++)
            {
                buffer[i] = (TMessage)oldSpan[i];
            }
            return newMsg;
        }
    }
}
