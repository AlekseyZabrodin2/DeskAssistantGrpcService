using DeskAssistant.Core.Services;
using DeskAssistantGrpcService.Extensions;
using Grpc.Core;
using NLog;
using TelegramBotGrpcService;
using ILogger = NLog.ILogger;


namespace DeskAssistantGrpcService.Services
{
    public class TelegramGrpcService : TelegramService.TelegramServiceBase
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly ITelegramNotificationService _telegramService;
        private CalendarTasksExtensions _calendarExtensions = new();


        public TelegramGrpcService(ITelegramNotificationService telegramService)
        {
            _telegramService = telegramService;
        }


        public override async Task<TelegramGetTodayTasksResponse> TelegramGetTasksForToday(TelegramEmptyRequest request, ServerCallContext context)
        {
            try
            {
                var response = new TelegramGetTodayTasksResponse();

                var allTasks = await _telegramService.GetTasksForTodayAsync();
                foreach (var item in allTasks)
                {
                    var taskItem = _calendarExtensions.TelegramTaskItemToGrpcTask(item);
                    var message = GenerateTelegramMessages(taskItem);

                    response.Messages.Add(message);
                    response.Tasks.Add(taskItem);
                }

                response.Success = true;
                response.Message = "Все задачи успешно получены";

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "gRPC: Ошибка получения задач");

                return new TelegramGetTodayTasksResponse
                {
                    Success = false,
                    Message = $"Ошибка: {ex.Message}"
                };
            }
        }

        private string GenerateTelegramMessages(TelegramTaskItem telegramTask)
        {
            var message = " *📝  Jarvis докладывает *\n" +
                         $" *______________________________*\n\n" +
                         $"🚀 *Задача на сегодня!*\n" +
                         $" *______________________________*\n\n" +
                         $"📌  -   *{telegramTask.Name}*\n\n" +
                         $"📄  -   *Описание:* {telegramTask.Description}\n\n" +
                         $"🗓️  -   *Дата:* {telegramTask.DueDate:dd.MM.yyyy}\n\n" +
                         $"📈  -   *Статус:* {telegramTask.Status}" +
                         $" *______________________________*\n\n";

            return message;
        }

        public override async Task<TelegramGetAllTasksWeekResponse> TelegramGetAllTasksForWeek(TelegramEmptyRequest request, ServerCallContext context)
        {
            var response = new TelegramGetAllTasksWeekResponse();


            return response;
        }
    }
}
