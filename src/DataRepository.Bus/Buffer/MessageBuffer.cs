using System.Runtime.CompilerServices;

namespace DataRepository.Bus.Buffer
{
    internal sealed class MessageBuffer<TMessage>
    {
        private static readonly bool IsValueType = typeof(TMessage).IsValueType;

        private int index;

        private readonly int maxCount;
        private readonly TMessage[] messages;

        public int MaxCount => maxCount;

        public int Index => index;

        public TMessage[] UnsafeGetMessages() => messages;

        public MessageBuffer(uint maxCount)
        {
            this.maxCount = (int)maxCount;
            messages = new TMessage[maxCount];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(TMessage message)
        {
            messages[index++] = message;
            return index >= maxCount;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            index = 0;
            if (!IsValueType)
            {
                messages.AsSpan().Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BatchMessages<TMessage> ToBathMessages() => BatchMessages<TMessage>.Create(messages, index);
    }
}
