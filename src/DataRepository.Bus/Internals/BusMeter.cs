using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace DataRepository.Bus.Internals
{
    internal sealed class BusMeter
    {
        internal readonly Meter meter;
        internal readonly Counter<long> publishTotal;
        internal readonly Counter<long> publishSucceed;
        internal readonly Counter<long> publishFail;

        internal readonly Counter<long> requestTotal;
        internal readonly Counter<long> requestSucceed;
        internal readonly Counter<long> requestFail;

        public BusMeter(string name, string? version)
        {
            meter = new Meter("Bus", version, [new KeyValuePair<string, object?>("type", name)]);
            publishTotal = meter.CreateCounter<long>("publishTotal");
            publishSucceed = meter.CreateCounter<long>("publishSucceed");
            publishFail = meter.CreateCounter<long>("publishFail");

            requestTotal = meter.CreateCounter<long>("requestTotal");
            requestSucceed = meter.CreateCounter<long>("requestSucceed");
            requestFail = meter.CreateCounter<long>("requestFail");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrPublish(bool succeed, string messageType)
        {
            var pair = new KeyValuePair<string, object?>("messageType", messageType);
            publishTotal.Add(1, pair);
            if (succeed)
            {
                publishSucceed.Add(1, pair);
            }
            else
            {
                publishFail.Add(1, pair);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrRequest(bool succeed, string requestType, string replyType)
        {
            var pair1 = new KeyValuePair<string, object?>("requestType", requestType);
            var pair2 = new KeyValuePair<string, object?>("replyType", replyType);
            requestTotal.Add(1, pair1, pair2);
            if (succeed)
            {
                requestSucceed.Add(1, pair1, pair2);
            }
            else
            {
                requestFail.Add(1, pair1, pair2);
            }
        }
    }
}
