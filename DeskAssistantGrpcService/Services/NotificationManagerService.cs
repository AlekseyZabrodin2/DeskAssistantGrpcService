using DeskAssistant.Core.Models;
using DeskAssistant.Core.Services;
using DeskAssistantGrpcService.DataBase;
using DeskAssistantGrpcService.Helpers;
using Microsoft.EntityFrameworkCore;
using NLog;
using ILogger = NLog.ILogger;

namespace DeskAssistantGrpcService.Services
{
    public class NotificationManagerService : INotificationService
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IDbContextFactory<NotificationDbContext> _contextNotificationDb;
        private readonly IDbContextFactory<TasksDbContext> _contextTasksDb;
        private readonly ITelegramNotificationService _telegramService;
        private readonly NotificationTimerHelper _timerHelper;


        public NotificationManagerService(IDbContextFactory<NotificationDbContext> contextNotificationDb,
            IDbContextFactory<TasksDbContext> contextTasksDb,
            ITelegramNotificationService telegramService)
        {
            _contextNotificationDb = contextNotificationDb;
            _contextTasksDb = contextTasksDb;
            _telegramService = telegramService;
            _timerHelper = new(_contextTasksDb, _telegramService);
        }


        public async Task CreatNotificationsAsync(NotificationEntity notification)
        {
            await using var context = _contextNotificationDb.CreateDbContext();

            await context.Notifications.AddAsync(notification);
            await context.SaveChangesAsync();

            _timerHelper.GraficsNotificationTimers(notification);
        }

        public Task DeleteNotificationAsync(NotificationEntity notification)
        {
            throw new NotImplementedException();
        }

        public Task ActivateNotificationsAsync(NotificationEntity notification)
        {
            throw new NotImplementedException();
        }

        public Task DisableNotificationsAsync(NotificationEntity notification)
        {
            throw new NotImplementedException();
        }

        public async Task<List<NotificationEntity>> GetNotificationsSettingsAsync(string clientId)
        {
            using var context = _contextNotificationDb.CreateDbContext();
            var notifications = await context.Notifications
                .Where(notif => notif.ClientId == clientId)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                _timerHelper.GraficsNotificationTimers(notification);
            }

            return notifications;
        }

        public Task GetNotificationStatus(NotificationEntity notification)
        {
            throw new NotImplementedException();
        }

        public async Task StartNotificationServiceAsync()
        {
            
        }

        public async Task StopNotificationServiceAsync()
        {
            
        }

        public async Task InitializeTimersAsync()
        {
            await using var context = _contextNotificationDb.CreateDbContext();
            var notifications = await context.Notifications.ToListAsync();

            foreach (var notification in notifications)
            {
                _timerHelper.GraficsNotificationTimers(notification);
            }
        }        
    }
}
