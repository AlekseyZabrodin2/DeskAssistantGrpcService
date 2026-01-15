using DeskAssistant.Core.Models;
using DeskAssistant.Core.Services;
using DeskAssistantGrpcService.DataBase;
using DeskAssistantGrpcService.Models;
using Microsoft.EntityFrameworkCore;
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
        private readonly IDbContextFactory<TasksDbContext> _contextFactory;


        public TelegramNotificationService(IOptions<ConnectionSettings> connectionSettings, IDbContextFactory<TasksDbContext> contextFactory)
        {
            _connectionSettings = connectionSettings.Value;
            _contextFactory = contextFactory;

            var botToken = _connectionSettings.BotClientToken;
            _chatId = long.Parse(_connectionSettings.BotChatId);
            _botClient = new TelegramBotClient(botToken);
        }




        public async Task NotifiTaskCompletedAsync(CalendarTaskModel notification)
        {
            throw new NotImplementedException();
        }

        public async Task NotifiTaskCreatedAsync(CalendarTaskModel notification)
        {
            try
            {
                var message = " *📝  DeskAssistant *\n" +
                             $" *______________________________*\n\n" +
                             $"🚀 *Новая задача создана!*\n" +
                             $" *______________________________*\n\n" +
                             $"📌  -   *{notification.Name}*\n\n" +
                             $"📄  -   *Описание:* {notification.Description}\n\n" +
                             $"🗓️  -   *Дата:* {notification.DueDate:dd.MM.yyyy}\n\n" +
                             $"📈  -   *Статус:* {notification.Status}" +
                             $" *______________________________*\n\n";

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

        public async Task NotificationAboutTasksAsync(CalendarTaskModel notification)
        {
            try
            {
                var message = " *📝  DeskAssistant *\n" +
                             $" *______________________________*\n\n" +
                             $"🚀 *Напоминание!*\n" +
                             $" *______________________________*\n\n" +
                             $"        * Задача на сегодня *\n\n" +
                             $"📌  -   *{notification.Name}*\n\n" +
                             $"📄  -   *Описание:* {notification.Description}\n\n" +
                             $"🗓️  -   *Дата:* {notification.DueDate:dd.MM.yyyy}\n\n" +
                             $"📈  -   *Статус:* {notification.Status}" +
                             $" *______________________________*\n\n";

                await _botClient.SendMessage(
                    _chatId,
                    message,
                    parseMode: ParseMode.Markdown);

                _logger.Info($"Напоминание отправлено: {notification.Name}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка отправки напоминания в Telegram");
            }
        }

        public async Task NotificationAboutBirthdaysAsync(BirthdaysEntity birthdays)
        {
            try
            {
                var message = $"🚀 *Напоминание!*\n" +
                            $" *______________________________*\n\n" +
                            $"🥳 * День рождения сегодня! *\n" +
                            $" * празднует - [ {birthdays.LastName} {birthdays.Name} ]*\n" +
                            $" *______________________________*\n\n" + 
                            " *📝  DeskAssistant *\n" +
                            $" *______________________________*\n\n";

                await _botClient.SendMessage(
                    _chatId,
                    message,
                    parseMode: ParseMode.Markdown);

                _logger.Info($"Напоминание о ДР отправлено: {birthdays.Id}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка отправки напоминания о ДР в Telegram");
            }
        }

        public async Task<List<CalendarTaskEntity>> GetAllTasksAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            var response = await context.Tasks.ToListAsync();
            return response;
        }

        public async Task<List<CalendarTaskEntity>> GetTasksForTodayAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            using var context = _contextFactory.CreateDbContext();

            var response = await context.Tasks.Where(task => task.DueDate == today).ToListAsync();
            return response;
        }

        public Task<CalendarTaskEntity> AddTaskAsync(CalendarTaskEntity taskEntity)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTaskAsync(CalendarTaskModel taskEntity, TaskStatusEnum status)
        {
            throw new NotImplementedException();
        }
    }
}
