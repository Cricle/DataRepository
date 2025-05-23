﻿using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace DataRepository.Bus.Internals
{
    internal sealed class RequestReplyDispatcherMeter
    {
        internal readonly Meter meter;

        internal readonly Counter<long> requestReplyTotal;
        internal readonly Counter<long> requestReplySucceed;
        internal readonly Counter<long> requestReplyFail;

        internal readonly ObservableCounter<long> pendingMessageCount;
        internal Func<long> pendingMessageFunc = () => 0;

        public RequestReplyDispatcherMeter(string name, string? version)
        {
            meter = new Meter("RequestReplyDispatcher", version, [new KeyValuePair<string, object?>("type", name)]);
            requestReplyTotal = meter.CreateCounter<long>("requestReplyTotal");
            requestReplySucceed = meter.CreateCounter<long>("requestReplySucceed");
            requestReplyFail = meter.CreateCounter<long>("requestReplyFail");
            pendingMessageCount = meter.CreateObservableCounter<long>("pendingMessageCount", pendingMessageFunc);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddHandle(bool succeed, string requestType,string replyType)
        {
            var pair1 = new KeyValuePair<string, object?>("requestType", requestType);
            var pair2 = new KeyValuePair<string, object?>("replyType", replyType);
            requestReplyTotal.Add(1, pair1,pair2);
            if (succeed)
            {
                requestReplySucceed.Add(1, pair1, pair2);
            }
            else
            {
                requestReplyFail.Add(1, pair1, pair2);
            }
        }
    }
}
