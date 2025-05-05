using BenchmarkDotNet.Attributes;
using DataRepository.Bus;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;

namespace DataRepository.Benchmark
{
    [MemoryDiagnoser]
    public class RequestReplyBenchmark
    {
        private IBus bus;
        private IAsyncRequestHandler<Student,int> publisher;

        [Params(1,20)]
        public int RequestCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var s = new ServiceCollection().AddInMemoryBus(x =>
            {
                x.AddRequestReplyUnBound<Student, int>();
            }).AddLogging().AddRequestReply<Student, int, StudentConsumer>();

            var cfg = s.AddMessagePipe();
            cfg.AddAsyncRequestHandler<StudentConsumer>();
            var ser = s
            .BuildServiceProvider();
            bus = ser.GetRequiredService<IBus>();
            publisher = ser.GetRequiredService<IAsyncRequestHandler<Student, int>>();
            bus.StartAsync().Wait();
        }

        [Benchmark]
        public async Task Request()
        {
            for (int i = 0; i < RequestCount; i++)
            {
                await bus.RequestAsync<Student, int>(new Student { A = 1 });
            }
        }

        [Benchmark(Baseline =true)]
        public async Task PipRequest()
        {
            for (int i = 0; i < RequestCount; i++)
            {
                await publisher.InvokeAsync(new Student { A = 1 });
            }
        }

        public class Student
        {
            public int A { get; set; }
        }

        class StudentConsumer : IRequestReply<Student, int>, IAsyncRequestHandler<Student, int>
        {
            public async ValueTask<int> InvokeAsync(Student request, CancellationToken cancellationToken = default)
            {
                await Task.Delay(1);
                return request.A;
            }

            public async Task<int> RequestAsync(Student request, CancellationToken token = default)
            {
                await Task.Delay(1);
                return request.A;
            }
        }
    }
}
