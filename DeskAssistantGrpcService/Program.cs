using DeskAssistant.Core.Services;
using DeskAssistantGrpcService.DataBase;
using DeskAssistantGrpcService.Models;
using DeskAssistantGrpcService.Services;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using System.ServiceProcess;


var logger = LogManager.GetCurrentClassLogger();

try
{

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();
    builder.Host.UseWindowsService();

    var environmentName = builder.Environment.EnvironmentName;

    logger.Info($"Current directory: {Directory.GetCurrentDirectory()}");
    logger.Info($"Base directory: {AppContext.BaseDirectory}");

    builder.Configuration
        .SetBasePath(AppContext.BaseDirectory)
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

    PostgresStopping(app);

    // Configure the HTTP request pipeline.
    app.MapGrpcService<TaskGrpcService>();
    app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

    EnsurePostgresRunning();

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

void EnsurePostgresRunning()
{
    try
    {
        var service = new ServiceController("PostgreSQL");
        if (service.Status != ServiceControllerStatus.Running)
        {
            logger.Info("Starting PostgreSQL service...");

            // In Debug you must Start Visual Studio as administrator, or start PostgreSQL mannually 
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(15));
            logger.Info("PostgreSQL service started successfully.");
        }
        else
        {
            logger.Info("PostgreSQL service is already running.");
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Failed to start PostgreSQL service.");
    }
}

void PostgresStopping(WebApplication app)
{
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStopping.Register(() =>
    {
        logger.Info("Application stopping...");

        try
        {
            var service = new ServiceController("PostgreSQL");
            if (service.Status != ServiceControllerStatus.Stopped &&
                service.Status != ServiceControllerStatus.StopPending)
            {
                logger.Info("Stopping PostgreSQL service...");
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                logger.Info("PostgreSQL service stopped.");
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error stopping PostgreSQL service");
        }
    });
}