using System.Diagnostics;

namespace DataRepository.Bus.Internals
{
    internal static class BusActivities
    {
        internal static readonly string? Version=typeof(BusActivities).Assembly.GetName().Version?.ToString();

        internal static readonly ActivitySource busSource = new ActivitySource("Bus", Version);
        internal static readonly ActivitySource consumerSource = new ActivitySource("Consumer", Version);
        internal static readonly ActivitySource requestReplySource = new ActivitySource("RequestReply", Version);
    }
}
