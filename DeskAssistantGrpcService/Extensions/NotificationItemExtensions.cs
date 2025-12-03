using DeskAssistant.Core.Models;
using NotificationGrpcService;
using System.Globalization;

namespace DeskAssistantGrpcService.Extensions
{
    public class NotificationItemExtensions
    {

        public NotificationEntity GrpcNotificationItemToNotificationEntity(NotificationItem notificationItem)
        {
            return new NotificationEntity
            {
                Id = notificationItem.Id,
                ClientId = notificationItem.ClientId,
                IsEnabled = bool.Parse(notificationItem.IsEnabled),
                NotificationTime = TimeSpan.Parse(notificationItem.NotificationTime),
                MondayEnabled = bool.Parse(notificationItem.MondayEnabled),
                TuesdayEnabled = bool.Parse(notificationItem.TuesdayEnabled),
                WednesdayEnabled = bool.Parse(notificationItem.WednesdayEnabled),
                ThursdayEnabled = bool.Parse(notificationItem.ThursdayEnabled),
                FridayEnabled = bool.Parse(notificationItem.FridayEnabled),
                SaturdayEnabled = bool.Parse(notificationItem.SaturdayEnabled),
                SundayEnabled = bool.Parse(notificationItem.SundayEnabled),
                CreatedAt = DateTime.Parse(notificationItem.CreatedAt, null, DateTimeStyles.RoundtripKind)
            };
        }

        public NotificationItem NotificationEntityToNotificationItem(NotificationEntity notification)
        {
            return new NotificationItem
            {
                Id = notification.Id.ToString(),
                ClientId = notification.ClientId,
                IsEnabled = notification.IsEnabled.ToString(),
                NotificationTime = notification.NotificationTime.ToString(),
                MondayEnabled = notification.MondayEnabled.ToString(),
                TuesdayEnabled = notification.TuesdayEnabled.ToString(),
                WednesdayEnabled = notification.WednesdayEnabled.ToString(),
                ThursdayEnabled = notification.ThursdayEnabled.ToString(),
                FridayEnabled = notification.FridayEnabled.ToString(),
                SaturdayEnabled = notification.SaturdayEnabled.ToString(),
                SundayEnabled = notification.SundayEnabled.ToString(),
                CreatedAt = notification.CreatedAt.ToString("O")

            };
        }
    }
}
