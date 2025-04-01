﻿using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace DataRepository.Bus.Internals
{
    internal sealed class ConsumerDispatcherMeter
    {
        internal readonly Meter meter;

        internal readonly Counter<long> handleTotal;
        internal readonly Counter<long> handleSucceed;
        internal readonly Counter<long> handleFail;

        internal readonly ObservableCounter<long> pendingMessageCount;
        internal Func<long> pendingMessageFunc = () => 0;

        public ConsumerDispatcherMeter(string name, string? version)
        {
            meter = new Meter("ConsumerDispatcher", version, [new KeyValuePair<string, object?>("type", name)]);
            handleTotal = meter.CreateCounter<long>("handleTotal");
            handleSucceed = meter.CreateCounter<long>("handleSucceed");
            handleFail = meter.CreateCounter<long>("handleFail");
            pendingMessageCount = meter.CreateObservableCounter<long>("pendingMessageCount", pendingMessageFunc);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddHandle(long count,bool succeed,string messageType)
        {
            var pair=new KeyValuePair<string,object?>("messageType", messageType);
            handleTotal.Add(count, pair);
            if (succeed)
            {
                handleTotal.Add(count, pair);
            }
            else
            {
                handleFail.Add(count, pair);
            }
        }
    }
}
