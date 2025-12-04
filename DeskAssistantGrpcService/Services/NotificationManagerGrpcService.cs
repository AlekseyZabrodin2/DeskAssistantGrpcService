using DeskAssistant.Core.Models;
using DeskAssistant.Core.Services;
using DeskAssistantGrpcService.Extensions;
using Grpc.Core;
using NLog;
using NotificationGrpcService;
using System.Globalization;
using ILogger = NLog.ILogger;

namespace DeskAssistantGrpcService.Services
{
    public class NotificationManagerGrpcService : NotificationService.NotificationServiceBase
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly INotificationService _notificationService;
        private readonly NotificationItemExtensions _notificationExtensions = new();


        public NotificationManagerGrpcService(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }


        public override async Task<NotificationResponse> NotificationsCreate(NotificationItem notification, ServerCallContext context)
        {
            try
            {
                _logger.Info($"gRPC: Создание уведомления для '{notification.ClientId}'");

                var notificationModel = _notificationExtensions.GrpcNotificationItemToNotificationEntity(notification);

                await _notificationService.CreatNotificationsAsync(notificationModel);

                return new NotificationResponse
                {
                    Success = true,
                    Message = "Уведомление успешно создано"
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "gRPC: Ошибка создания уведомления");

                return new NotificationResponse
                {
                    Success = false,
                    Message = $"Ошибка: {ex.Message}"
                };
            }
        }

        public override async Task<NotificationItemResponse> NotificationsGetSettings(NotificationClientIdRequest request, ServerCallContext context)
        {
            try
            {
                var response = new NotificationItemResponse();

                var notificationList = await _notificationService.GetNotificationsSettingsAsync(request.ClientId);
                foreach ( var notification in notificationList)
                {
                    var notificationItem = _notificationExtensions.NotificationEntityToNotificationItem(notification);
                    response.Notification.Add(notificationItem);
                }
                response.Success = true;
                response.Message = "Уведомления успешно получены";

                return response;
            }
            catch (Exception ex)
            {
                return new NotificationItemResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
