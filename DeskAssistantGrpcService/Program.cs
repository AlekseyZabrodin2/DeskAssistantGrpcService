using DeskAssistant.Core.Services;
using DeskAssistantGrpcService.DataBase;
using DeskAssistantGrpcService.Models;
using DeskAssistantGrpcService.Services;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;


var logger = LogManager.GetCurrentClassLogger();

try
{

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();
    builder.Host.UseWindowsService();

    var environmentName = builder.Environment.EnvironmentName;

    builder.Configuration
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{environmentName}.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    logger.Info($"App start with [{environmentName}] environment");

    builder.Services.AddDbContextFactory<TasksDbContext>(options =>
    {
        var connectingString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseNpgsql(connectingString);
    });

    // Add services to the container.
    builder.Services.AddGrpc();
    builder.Services.AddSingleton<ITelegramNotificationService, TelegramNotificationService>();
    builder.Services.AddScoped<ITaskService, TaskServiceImpl>();

    builder.Services.Configure<ConnectionSettings>(builder.Configuration.GetSection("ConnectionStrings"));

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.MapGrpcService<TaskGrpcService>();
    app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

    logger.Info("Service is started");
    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Stopped program because of exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}

