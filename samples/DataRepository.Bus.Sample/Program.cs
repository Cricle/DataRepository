using DataRepository.Bus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var ss = new ServiceCollection()
            .AddLogging(x => x.AddConsole())
            //.AddNatsBus(p => p.AddConsumer<Student>("student","1",scale:100).AddRequestReply<Student,int>($"rr.student","a",scale:1024))
            .AddInMemoryBus(p => p.AddConsumerUnBound<Student>(batchSize:1000,fetchTime:TimeSpan.FromMilliseconds(500)).AddRequestReplyUnBound<Student, int>())
            //.AddSingleton<IMessageSerialization>(new MemoryPackMessageSerialization(null))
            .AddMessageConsumer<Student, StudentConsumer>()
            .AddRequestReply<Student, int, StudentRequestReply>();
        var s=ss
            .BuildServiceProvider();
        var bus = s.GetRequiredService<IBus>();
        await bus.StartAsync();
        await bus.RequestAsync<Student, int>(new Student { Id = 1 });

        //var mem = GC.GetTotalMemory(true);
        //var sw = Stopwatch.GetTimestamp();
        //var tasks = new Task[10_000];
        //for (int i = 0; i < tasks.Length; i++)
        //{
        //    tasks[i] = Task.Factory.StartNew(async () =>
        //    {
        //        for (int j = 0; j < 10; j++)
        //        {
        //            var res = await bus.RequestAsync<Student, int>(new Student { Id = j });
        //        }
        //    }).Unwrap();
        //}
        //await Task.WhenAll(tasks);
        //Console.WriteLine(Stopwatch.GetElapsedTime(sw).TotalMilliseconds);
        //Console.WriteLine($"{(GC.GetTotalMemory(false) - mem) / 1024 / 1024.0}M");

        //Console.WriteLine(res);
        //var id = Random.Shared.Next(1, 9);
        //while (true)
        //{
        //    var rep = await bus.RequestAsync<Student, int>(new Student { Id = id, Name = "aseda" });
        //    Console.WriteLine($"{id}.{rep}");
        //    await Task.Delay(Random.Shared.Next(200));
        //}
        Console.ReadLine();
    }
}
public class StudentConsumer : IBatchConsumer<Student>
{
    public Task HandleAsync(Student message, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(message);
        return Task.CompletedTask;
    }
}

public class StudentRequestReply : IRequestReply<Student, int>
{
    public async Task<int> RequestAsync(Student request, CancellationToken token = default)
    {
        await Task.Delay(100);
        return request.Id;
    }
}
//[MemoryPackable]
public partial record class Student
{
    public int Id { get; set; }

    public string Name { get; set; }
}