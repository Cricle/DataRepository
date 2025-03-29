using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

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
        private static readonly bool IsValueType = typeof(TMessage).IsValueType;

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
            if (IsValueType)
            {
                CopySlow(messages,newMsg);
            }
            else
            {
                Array.Copy(messages.UnsafeGetBuffer(), newMsg.UnsafeGetBuffer(), messages.Size);
            }
            return newMsg;
        }

        private static void CopySlow(in BatchMessages<object> old, in BatchMessages<TMessage> @new)
        {
            var oldBuffer = old.UnsafeGetBuffer();
            var newBuffer = @new.UnsafeGetBuffer();
            for (int i = 0; i < old.Size; i++)
            {
                newBuffer[i] = (TMessage)oldBuffer[i];
            }
        }
    }
}
