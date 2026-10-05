using EasyUpdate.Data;
using EasyUpdate.UpdateService;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Lets the app run as a Windows Service (and still as a console app when you press F5)
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "EasyUpdate Scheduler";
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnection")));
builder.Services.AddScoped<IUpdateExecutor, UpdateExecutor>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<UpdatePollingWorker>();

var host = builder.Build();
host.Run();