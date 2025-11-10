using DeskAssistant.Core.Models;
using DeskAssistant.Core.Services;
using DeskAssistantGrpcService.Models;
using Microsoft.Extensions.Options;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ILogger = NLog.ILogger;

namespace DeskAssistantGrpcService.Services
{
    public class TelegramNotificationService : ITelegramNotificationService
    {
        TelegramBotClient _botClient;
        private readonly ChatId _chatId;
        private readonly ConnectionSettings _connectionSettings;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();


        public TelegramNotificationService(IOptions<ConnectionSettings> connectionSettings)
        {
            _connectionSettings = connectionSettings.Value;

            var botToken = _connectionSettings.BotClientToken;
            _chatId = long.Parse(_connectionSettings.BotChatId);
            _botClient = new TelegramBotClient(botToken);
        }




        public async Task NotifyTaskCompletedAsync(CalendarTaskModel notification)
        {
            throw new NotImplementedException();
        }

        public async Task NotifyTaskCreatedAsync(CalendarTaskModel notification)
        {
            try
            {
                var message = $"🚀 *Новая задача создана!*\n" +
                             $" *______________________________*\n\n" +
                             $"📌  -   *{notification.Name}*\n\n" +
                             $"📄  -   *Описание:* {notification.Description}\n\n" +
                             $"🗓️  -   *Дата:* {notification.DueDate:dd.MM.yyyy}\n\n" +
                             $"📈  -   *Статус:* {notification.Status}";

                await _botClient.SendMessage(
                    _chatId,
                    message,
                    parseMode: ParseMode.Markdown);

                _logger.Info($"Уведомление отправлено: {notification.Name}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка отправки уведомления в Telegram");
            }
        }
    }
}
