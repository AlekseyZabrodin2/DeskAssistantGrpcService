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
        private readonly IBirthdaysService _birthdayService;
        private readonly NotificationTimerHelper _timerHelper;


        public NotificationManagerService(IDbContextFactory<NotificationDbContext> contextNotificationDb,
            IDbContextFactory<TasksDbContext> contextTasksDb,
            ITelegramNotificationService telegramService,
            IBirthdaysService birthdayService)
        {
            _contextNotificationDb = contextNotificationDb;
            _contextTasksDb = contextTasksDb;
            _telegramService = telegramService;
            _birthdayService = birthdayService;
            _timerHelper = new(_contextTasksDb, _telegramService, _birthdayService);
        }


        public async Task CreatNotificationsAsync(NotificationEntity notification)
        {
            await using var context = _contextNotificationDb.CreateDbContext();

            await context.Notifications.AddAsync(notification);
            await context.SaveChangesAsync();

            _timerHelper.GraficsNotificationTimers(notification);
        }

        public async Task DeleteNotificationAsync(string clientId, string notificationId)
        {
            using var context = _contextNotificationDb.CreateDbContext();
            var notificationsForDelete = await context.Notifications
                .Where(notif => notif.ClientId == clientId && notif.Id == notificationId)
                .FirstOrDefaultAsync();

            if (notificationsForDelete != null)
            {
                context.Notifications.Remove(notificationsForDelete);
                context.SaveChanges();
            }
            _timerHelper.RemoveTimer((Guid)notificationsForDelete.TimerId);
        }

        public async Task SetNotificationsStatusAsync(NotificationEntityStatus setNotification)
        {
            using var context = _contextNotificationDb.CreateDbContext();
            var notifications = await context.Notifications
                .Where(notif => notif.Id == setNotification.Id && notif.ClientId == setNotification.ClientId)
                .FirstOrDefaultAsync();

            if (notifications != null)
            {
                notifications.IsEnabled = setNotification.IsEnabled;

                context.SaveChanges();
            }                
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
