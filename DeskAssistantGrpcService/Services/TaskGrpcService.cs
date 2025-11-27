using DeskAssistant.Core.Extensions;
using DeskAssistant.Core.Models;
using DeskAssistant.Core.Services;
using DeskAssistantGrpcService.Extensions;
using Grpc.Core;
using NLog;
using ILogger = NLog.ILogger;

namespace DeskAssistantGrpcService.Services
{
    public class TaskGrpcService : TaskService.TaskServiceBase
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly ITaskService _taskService;
        private readonly ITelegramNotificationService _telegramService;
        private EnumExtensions _enumExtensions = new EnumExtensions();
        private CalendarTasksExtensions _calendarExtensions = new();


        public TaskGrpcService(ITelegramNotificationService telegramService, ITaskService taskService)
        {
            _taskService = taskService;
            _telegramService = telegramService;
        }


        public override async Task<TaskResponse> CreateTask(TaskItem taskItem, ServerCallContext context)
        {
            try
            {
                _logger.Info($"gRPC: Создание задачи '{taskItem.Name}'");

                var taskEntity = _calendarExtensions.GrpcTaskItemToCalendarTaskEntity(taskItem);
                var taskModel = _calendarExtensions.TaskEntityToCalendarTaskModel(taskEntity);

                var createdTask = _taskService.AddTaskForSelectedDate(taskEntity);

                _ = Task.Run(async () => _telegramService.NotifyTaskCreatedAsync(taskModel));

                return new TaskResponse
                {
                    Success = true,
                    Message = "Задача успешно создана"
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "gRPC: Ошибка создания задачи");

                return new TaskResponse
                {
                    Success = false,
                    Message = $"Ошибка: {ex.Message}"
                };
            }
        }

        public override async Task<TaskResponse> UpdateTask(TaskItem taskItem, ServerCallContext context)
        {
            try
            {
                var response = new TaskResponse();

                var taskModel = _calendarExtensions.GrpcTaskItemToCalendarTaskModel(taskItem);
                var taskStatus = _enumExtensions.StatusFromString(taskModel.Status);
                await _taskService.UpdateTaskAsync(taskModel, taskStatus);

                response.Success = true;
                response.Message = "Задача успешно обновлена";

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "gRPC: Ошибка обновления задачи");

                return new TaskResponse
                {
                    Success = false,
                    Message = $"Ошибка: {ex.Message}"
                };
            }
        }

        public override async Task<GetAllTasksResponse> GetAllTasks(EmptyRequest empty, ServerCallContext context)
        {
            try
            {
                var response = new GetAllTasksResponse();
                var allTasks = await _taskService.GetAllTasksAsync();
                foreach (var item in allTasks)
                {
                    var taskItem = _calendarExtensions.CalendarTaskEntityToGrpcTask(item);

                    response.Tasks.Add(taskItem);
                }

                response.Success = true;
                response.Message = "Все задачи успешно получены";

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "gRPC: Ошибка получения задач");

                return new GetAllTasksResponse
                {
                    Success = false,
                    Message = $"Ошибка: {ex.Message}"
                };
            }
        }

        public override async Task<TaskResponse> CompleteTask(TaskCompleteRequest request, ServerCallContext context)
        {
            return new TaskResponse
            {
                Success = true,
                Message = "Задача успешно создана"
            };
        }

        public override async Task<TaskResponse> NotifyTask(TaskNotifyRequest request, ServerCallContext context)
        {
            try
            {
                await _telegramService.NotifyTaskCompletedAsync(new CalendarTaskModel
                {
                    Id = request.TaskId,
                    Name = request.TaskName,
                    DueDate = DateOnly.Parse(request.TaskDueDate),
                    Priority = _enumExtensions.PrioritiesLevelFromString(request.TaskPriority)
                });

                return new TaskResponse
                {
                    Success = true,
                    Message = "Уведомление отправлено"
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка отправки уведомления");
                return new TaskResponse
                {
                    Success = false,
                    Message = $"Ошибка: {ex.Message}"
                };
            }
        }

        public override async Task<TaskServerEchoResponse> ServerEcho(EmptyRequest empty, ServerCallContext context)
        {
            try
            {
                return new TaskServerEchoResponse
                {
                    Success = true,
                    Message = "Server ping"
                };
            }
            catch (Exception ex)
            {
                return new TaskServerEchoResponse
                {
                    Success = false,
                    Message = $"Ошибка: {ex.Message}"
                };
            }
        }

        public override async Task<TaskDataBaseEchoResponse> DataBaseEcho(EmptyRequest empty, ServerCallContext context)
        {
            try
            {
                await _taskService.DataBaseEchoAsync();

                return new TaskDataBaseEchoResponse
                {
                    Success = true,
                    Message = "DataBase ping"
                };
            }
            catch (Exception ex)
            {
                return new TaskDataBaseEchoResponse
                {
                    Success = false,
                    Message = $"Ошибка: {ex.Message}"
                };
            }
        }
    }
}
