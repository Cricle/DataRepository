using DataRepository.Casing.Redis;
using DataRepository.SampleWeb;
using DataRepository.SampleWeb.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<IConnectionMultiplexer>(p => ConnectionMultiplexer.Connect("127.0.0.1:6379"));

        builder.Services.AddRedisCasting(typeof(ValuePublisher<>))
            .AddRedisNewest(new RedisHashCasingNewestConfig("test:newest:"))
            .AddRedisTopN(new RedisSortSetTopNConfig("test:topn:") { StoreSize = 100 });

        builder.Services.AddCasting<GpsPosition>(b => b.AddTopN().AddNewest());

        builder.Services.AddDbContextFactory<NumberDbContext>(p => p.UseSqlite("Data source=a.db"));
        builder.Services.AddRespository<NumberDbContext>();
        builder.Services.AddScoped<NumberService>();
        builder.Services.AddSingleton<NumberCalc>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbc = scope.ServiceProvider.GetRequiredService<NumberDbContext>();
            dbc.Database.EnsureCreated();
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();

        app.Run();
    }
}