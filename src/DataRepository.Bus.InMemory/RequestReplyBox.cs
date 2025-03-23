namespace DataRepository.Bus.InMemory
{
    internal readonly struct RequestReplyBox
    {
        public RequestReplyBox(object requqest)
        {
            Requqest = requqest;
            ReplySource = new TaskCompletionSource<object>();
        }

        public object Requqest { get; }

        public TaskCompletionSource<object> ReplySource { get; }
    }
}
