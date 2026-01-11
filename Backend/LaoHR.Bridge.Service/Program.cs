using LaoHR.Bridge.Service;
using LaoHR.Shared.Data;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "LaoHR ZKTeco Bridge";
});

// Register Shared DbContext
builder.Services.AddDbContext<LaoHRDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
