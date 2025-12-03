using DeskAssistant.Core.Models;

namespace DeskAssistantGrpcService.Models
{
    public class NotificationTimer
    {
        public Guid Id { get; set; }
        public string NotificationId { get; set; }
        public Timer Timer { get; set; }
        public DateTime ScheduledTime { get; set; }
        public NotificationEntity Notification { get; set; }
    }
}
