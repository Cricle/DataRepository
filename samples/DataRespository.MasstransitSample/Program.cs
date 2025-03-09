using DataRespository.MasstransitSample;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddStackDataService();
builder.Services.AddDbContextFactory<TestDbContext>(p => p.UseSqlite("Data source=a.db"))
    .AddRespository<TestDbContext>();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly);
    x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
});
var app = builder.Build();
using (var scope=app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<TestDbContext>().Database.EnsureCreated();
}
app.MapDefaultControllerRoute();
app.Run();
