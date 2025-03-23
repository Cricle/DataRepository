
using System.Threading.Channels;

namespace DataRepository.Bus.InMemory
{
    public abstract class ChannelIdentity
    {
        protected ChannelIdentity(bool unBoundChannel, UnboundedChannelOptions? unBoundedChannelOptions, BoundedChannelOptions? boundedChannelOptions)
        {
            UnBoundChannel = unBoundChannel;
            UnBoundedChannelOptions = unBoundedChannelOptions;
            BoundedChannelOptions = boundedChannelOptions;
        }

        public bool UnBoundChannel { get; }

        public UnboundedChannelOptions? UnBoundedChannelOptions { get; }

        public BoundedChannelOptions? BoundedChannelOptions { get; }

        public Channel<T> CreateChannel<T>()
        {
            if (UnBoundChannel)
            {
                return Channel.CreateUnbounded<T>(UnBoundedChannelOptions ?? new UnboundedChannelOptions());
            }
            return Channel.CreateBounded<T>(BoundedChannelOptions ?? throw new InvalidOperationException("Bound channel must has BoundedChannelOptions"));
        }
    }
}
