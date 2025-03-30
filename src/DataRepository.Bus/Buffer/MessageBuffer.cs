using System.Runtime.CompilerServices;

namespace DataRepository.Bus.Buffer
{
    public sealed class MessageBuffer<TMessage>
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

        public bool Add(TMessage message)
        {
            messages[index++] = message;
            return index >= maxCount;
        }


        public void Clear()
        {
            index = 0;
            if (!IsValueType)
            {
                Array.Clear(messages, 0, messages.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BatchMessages<TMessage> ToBathMessages() => BatchMessages<TMessage>.Create(messages, index);
    }
}
