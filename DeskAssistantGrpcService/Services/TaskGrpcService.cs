using DeskAssistant.Core.Extensions;
using DeskAssistant.Core.Models;
using DeskAssistant.Core.Services;
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

                var taskEntity = GrpcTaskItemToCalendarTaskEntity(taskItem);
                var taskModel = TaskEntityToCalendarTaskModel(taskEntity);

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

                var taskModel = GrpcTaskItemToCalendarTaskModel(taskItem);

                await _taskService.UpdateTaskAsync(taskModel, taskModel.Status);

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
                    var taskItem = CalendarTaskEntityToGrpcTask(item);

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

        private CalendarTaskModel GrpcTaskItemToCalendarTaskModel(TaskItem taskItem)
        {
            return new CalendarTaskModel
            {
                Id = string.IsNullOrEmpty(taskItem.Id) ? 0 : int.Parse(taskItem.Id),
                Name = taskItem.Name,
                Description = taskItem.Description,
                DueDate = DateOnly.Parse(taskItem.DueDate),
                IsCompleted = bool.Parse(taskItem.IsCompleted),
                Priority = _enumExtensions.PrioritiesLevelFromString(taskItem.Priority),
                Category = taskItem.Category,
                Status = _enumExtensions.StatusFromString(taskItem.Status),
                Tags = taskItem.Tags,
                CreatedDate = taskItem.CreatedDate == "" ? null : DateTime.SpecifyKind(DateTime.Parse(taskItem.CreatedDate), DateTimeKind.Utc),
                DueTime = taskItem.DueTime == "" ? null : TimeSpan.Parse(taskItem.DueTime),
                ReminderTime = taskItem.ReminderTime == "" ? null : DateTime.SpecifyKind(DateTime.Parse(taskItem.ReminderTime), DateTimeKind.Utc),
                CompletedDate = taskItem.CompletedDate == "" ? null : DateTime.SpecifyKind(DateTime.Parse(taskItem.CompletedDate), DateTimeKind.Utc),
                IsRecurring = string.IsNullOrEmpty(taskItem.IsRecurring) ? false : bool.Parse(taskItem.IsRecurring),
                RecurrencePattern = taskItem.RecurrencePattern,
                Duration = taskItem.Duration == "" ? null : TimeSpan.Parse(taskItem.Duration)
            };
        }

        private CalendarTaskEntity GrpcTaskItemToCalendarTaskEntity(TaskItem taskItem)
        {
            return new CalendarTaskEntity
            {
                Name = taskItem.Name,
                Description = taskItem.Description,
                CreatedDate = DateTime.SpecifyKind(DateTime.Parse(taskItem.CreatedDate), DateTimeKind.Utc),
                DueDate = DateOnly.Parse(taskItem.DueDate),
                Priority = _enumExtensions.PrioritiesLevelFromString(taskItem.Priority),
                Category = taskItem.Category,
                IsCompleted = bool.Parse(taskItem.IsCompleted),
                Status = _enumExtensions.StatusFromString(taskItem.Status),
                Tags = taskItem.Tags,
                RecurrencePattern = "None"
            };
        }

        private CalendarTaskModel TaskEntityToCalendarTaskModel(CalendarTaskEntity entity)
        {
            return new CalendarTaskModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                DueDate = entity.DueDate,
                IsCompleted = entity.IsCompleted,
                Priority = entity.Priority,
                Category = entity.Category,
                Status = entity.Status,
                Tags = entity.Tags,
                CreatedDate = entity.CreatedDate,
                DueTime = entity.DueTime,
                ReminderTime = entity.ReminderTime,
                CompletedDate = entity.CompletedDate,
                IsRecurring = entity.IsRecurring,
                RecurrencePattern = entity.RecurrencePattern,
                Duration = entity.Duration
            };
        }

        private TaskItem CalendarTaskModelToGrpcTask(CalendarTaskModel model)
        {
            return new TaskItem
            {
                Id = model.Id.ToString(),
                Name = model.Name,
                Description = model.Description,
                DueDate = model.DueDate.ToString("yyyy-MM-dd"),
                IsCompleted = model.IsCompleted.ToString(),
                Priority = _enumExtensions.PrioritiesLevelToString(model.Priority),
                Category = model.Category,
                Status = _enumExtensions.StatusToString(model.Status),
                Tags = model.Tags,
                CreatedDate = model.CreatedDate.ToString(),
                DueTime = model.DueTime.ToString(),
                ReminderTime = model.ReminderTime.ToString(),
                CompletedDate = model.CompletedDate.ToString(),
                IsRecurring = model.IsRecurring.ToString(),
                RecurrencePattern = "None",
                Duration = model.Duration.ToString()

            };
        }

        private TaskItem CalendarTaskEntityToGrpcTask(CalendarTaskEntity entity)
        {
            return new TaskItem
            {
                Id = entity.Id.ToString(),
                Name = entity.Name ?? "",
                Description = entity.Description ?? "",
                DueDate = entity.DueDate.ToString("yyyy-MM-dd") ?? "",
                IsCompleted = entity.IsCompleted.ToString(),
                Priority = _enumExtensions.PrioritiesLevelToString(entity.Priority) ?? "",
                Category = entity.Category ?? "",
                Status = _enumExtensions.StatusToString(entity.Status) ?? "",
                Tags = entity.Tags ?? "",
                CreatedDate = entity.CreatedDate.ToString(),
                DueTime = entity.DueTime?.ToString() ?? "",
                ReminderTime = entity.ReminderTime?.ToString() ?? "",
                CompletedDate = entity.CompletedDate?.ToString() ?? "",
                IsRecurring = entity.IsRecurring.ToString(),
                RecurrencePattern = "None",
                Duration = entity.Duration?.ToString() ?? ""
            };
        }


    }
}
