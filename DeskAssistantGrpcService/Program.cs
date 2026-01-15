using DeskAssistant.Core.Services;
using DeskAssistantGrpcService.DataBase;
using DeskAssistantGrpcService.Helpers;
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

    builder.Services.AddDbContextFactory<BirthdaysDbContext>(options =>
    {
        var connectingString = builder.Configuration.GetConnectionString("DefaultBirthdayTableConnection");
        options.UseNpgsql(connectingString);
    });

    builder.Services.AddDbContextFactory<TasksDbContext>(options =>
    {
        var connectingString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseNpgsql(connectingString);
    });

    builder.Services.AddDbContextFactory<NotificationDbContext>(options =>
    {
        var connectingString = builder.Configuration.GetConnectionString("DefaultNotificationTableConnection");
        options.UseNpgsql(connectingString);
    });

    // Add services to the container.
    builder.Services.AddGrpc();
    builder.Services.AddSingleton<IBirthdaysService, BirthdayServiceImpl>();
    builder.Services.AddSingleton<NotificationTimerHelper>();
    builder.Services.AddSingleton<ITelegramNotificationService, TelegramNotificationService>();
    builder.Services.AddScoped<ITaskService, TaskServiceImpl>();
    builder.Services.AddSingleton<NotificationManagerService>();
    builder.Services.AddScoped<INotificationService>(sp => sp.GetRequiredService<NotificationManagerService>());

    builder.Services.Configure<ConnectionSettings>(builder.Configuration.GetSection("ConnectionStrings"));

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.MapGrpcService<BirthdayGrpcService>();
    app.MapGrpcService<TaskGrpcService>();
    app.MapGrpcService<TelegramGrpcService>();
    app.MapGrpcService<NotificationManagerGrpcService>();
    app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
    
    EnsurePostgresRunning();

    await InitializeDatabaseAndTimersAsync(app);

    SetupApplicationStopping(app);

    logger.Info("Service is started");
    await app.RunAsync();     
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


async Task InitializeDatabaseAndTimersAsync(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var birthdayFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BirthdaysDbContext>>();
        using var dbBirthday = birthdayFactory.CreateDbContext();
        dbBirthday.Database.EnsureCreated();
        logger.Info("Birthdays Db Ensure Created");

        var taskFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TasksDbContext>>();
        using var db = taskFactory.CreateDbContext();
        db.Database.EnsureCreated();
        logger.Info("Tasks Db Ensure Created");

        var notifFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<NotificationDbContext>>();
        using var dbNotif = notifFactory.CreateDbContext();
        dbNotif.Database.EnsureCreated();
        logger.Info("Notification Db Ensure Created");

        var notificationService = scope.ServiceProvider.GetRequiredService<NotificationManagerService>();
        await notificationService.InitializeTimersAsync();
    }
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

void SetupApplicationStopping(WebApplication app)
{
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

    PostgresStopping(app);

    lifetime.ApplicationStopping.Register(() =>
    {
        logger.Info("Application stopping...");

        try
        {
            // Останавливаем таймеры уведомлений
            using var scope = app.Services.CreateScope();
            var timerHelper = scope.ServiceProvider.GetRequiredService<NotificationTimerHelper>();
            timerHelper.StopAllNotificationTimers();
            logger.Info("? Все таймеры уведомлений остановлены.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error stopping application services");
        }
    });
}