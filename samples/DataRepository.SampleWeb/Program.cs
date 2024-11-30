using DataRepository;
using DataRepository.SampleWeb;
using Microsoft.EntityFrameworkCore;

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
        builder.Services.AddDbContext<NumberDbContext>(p => p.UseSqlite("Data source=a.db"));
        builder.Services.AddRespository<NumberDbContext>();
        builder.Services.AddScoped<NumberService>();

        var app = builder.Build();

        using (var scope=app.Services.CreateScope())
        {
            var dbc=scope.ServiceProvider.GetRequiredService<NumberDbContext>();
            dbc.Database.EnsureCreated();
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}