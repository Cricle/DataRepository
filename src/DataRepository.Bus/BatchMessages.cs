using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DataRepository.Bus
{
    public readonly struct BatchMessages<T> : IDisposable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BatchMessages<T> Create(T[] buffer, int size)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            if (size <= 0 || buffer.Length >= size)
            {
                throw new IndexOutOfRangeException($"The buffer size is {buffer.Length} but the size is {size}, size must min than buffer size or > 0");
            }
            return new BatchMessages<T>(false, buffer, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BatchMessages<T> Rent(int size)
        {
            if (size <= 0)
            {
                throw new IndexOutOfRangeException($"The size must > 0");
            }
            var buffer = ArrayPool<T>.Shared.Rent(size);
            return new BatchMessages<T>(true, buffer, size);
        }

        internal readonly bool shouldReturn;
        internal readonly T[] buffer;

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

        public Span<T> Span => buffer.AsSpan(0, Size);

        public ReadOnlyMemory<T> Memory => buffer.AsMemory(0, Size);
    }
}
