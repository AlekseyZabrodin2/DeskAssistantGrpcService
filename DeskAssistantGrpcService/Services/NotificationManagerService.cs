using DeskAssistant.Core.Models;
using DeskAssistant.Core.Services;
using DeskAssistantGrpcService.DataBase;
using DeskAssistantGrpcService.Helpers;
using DeskAssistantGrpcService.Models;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Collections.Concurrent;
using ILogger = NLog.ILogger;

namespace DeskAssistantGrpcService.Services
{
    public class NotificationManagerService : INotificationService
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IDbContextFactory<NotificationDbContext> _contextFactory;
        private readonly ConcurrentDictionary<Guid, NotificationTimer> _notificationTimers = new();
        private readonly NotificationTimerHelper _timerHelper = new();


        public NotificationManagerService(IDbContextFactory<NotificationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }


        public async Task CreatNotificationsAsync(NotificationEntity notification)
        {
            await using var context = _contextFactory.CreateDbContext();

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
            using var context = _contextFactory.CreateDbContext();
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
            await using var context = _contextFactory.CreateDbContext();
            var notifications = await context.Notifications.ToListAsync();

            foreach (var notification in notifications)
            {
                _timerHelper.GraficsNotificationTimers(notification);
            }
        }        
    }
}
