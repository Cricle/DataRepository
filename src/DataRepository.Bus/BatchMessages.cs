using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DataRepository.Bus
{
    public readonly struct BatchMessages<T> : IDisposable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BatchMessages<T> Create(T[] buffer,int size)
        {
            return new BatchMessages<T>(false, buffer, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BatchMessages<T> Rent(int size)
        {
            var buffer = ArrayPool<T>.Shared.Rent(size);
            return new BatchMessages<T>(true, buffer, size);
        }

        private readonly bool shouldReturn;
        private readonly T[] buffer;

        internal BatchMessages(bool shouldReturn, T[] buffer, int size)
        {
            Debug.Assert(buffer != null);
            Debug.Assert(size > 0);

            this.shouldReturn = shouldReturn;
            this.buffer = buffer;
            Size = size;
        }

        public int Size { get; }

        public readonly T[] UnsafeGetBuffer() => buffer;

        public void Dispose()
        {
            if (shouldReturn)
            {
                ArrayPool<T>.Shared.Return(buffer);
            }
        }

        public Span<T> Span => buffer;

        public ReadOnlyMemory<T> Memory => buffer;
    }
}
