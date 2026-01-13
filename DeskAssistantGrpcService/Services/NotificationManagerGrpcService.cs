using DeskAssistant.Core.Services;
using DeskAssistantGrpcService.Extensions;
using Grpc.Core;
using NLog;
using NotificationGrpcService;
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

        public override async Task<NotificationResponse> NotificationsDelete(NotificationItemId request, ServerCallContext context)
        {
            try
            {
                _logger.Info($"Удаление уведомления для '{request.ClientId}' с номером - [{request.Id}]");

                await _notificationService.DeleteNotificationAsync(request.ClientId, request.Id);

                return new NotificationResponse
                {
                    Success = true,
                    Message = "Уведомление успешно удалено"
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "gRPC: Ошибка при удалении уведомления");

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

        public override async Task<NotificationResponse> NotificationsSetStatus(NotificationItemStatus notification, ServerCallContext context)
        {
            try
            {
                _logger.Info($"Уведомление для '{notification.ClientId}' включен - [{(bool.Parse(notification.IsEnabled) ? "ВКЛ" : "ВЫКЛ")}]");

                var notificationEntity = _notificationExtensions.ItemStatusToNotificationEntity(notification);
                await _notificationService.SetNotificationsStatusAsync(notificationEntity);

                return new NotificationResponse
                {
                    Success = true,
                    Message = $"Статус для уведомления успешно {(bool.Parse(notification.IsEnabled) ? "ВКЛ" : "ВЫКЛ")}]"
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при выключении/включении уведомления");

                return new NotificationResponse
                {
                    Success = false,
                    Message = $"Ошибка: {ex.Message}"
                };
            }
        }
    }
}
