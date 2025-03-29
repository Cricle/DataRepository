using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;

namespace DataRepository.Bus.InMemory
{
    internal sealed class RequestReplyBox : IValueTaskSource<object>
    {
        public RequestReplyBox(object requqest)
        {
            Requqest = requqest;
            taskSource = new ManualResetValueTaskSourceCore<object>();
        }

        private ManualResetValueTaskSourceCore<object> taskSource;

        public short Version => taskSource.Version;

        public object Requqest { get; }

        public ValueTask<object> Task
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueTask<object>(this, Version);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetResult(object result) => taskSource.SetResult(result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Reset() => taskSource.Reset();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetResult(short token) => taskSource.GetResult(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTaskSourceStatus GetStatus(short token) => taskSource.GetStatus(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetException(Exception exception) => taskSource.SetException(exception);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) => taskSource.OnCompleted(continuation, state, token, flags);
    }
}
