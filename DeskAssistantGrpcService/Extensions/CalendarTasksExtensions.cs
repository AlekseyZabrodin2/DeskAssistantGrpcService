using DeskAssistant.Core.Extensions;
using DeskAssistant.Core.Models;
using TelegramBotGrpcService;

namespace DeskAssistantGrpcService.Extensions
{
    public class CalendarTasksExtensions
    {
        private EnumExtensions _enumExtensions = new EnumExtensions();


        public CalendarTaskModel GrpcTaskItemToCalendarTaskModel(TaskItem taskItem)
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
                Status = taskItem.Status,
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

        public CalendarTaskEntity GrpcTaskItemToCalendarTaskEntity(TaskItem taskItem)
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

        public CalendarTaskModel TaskEntityToCalendarTaskModel(CalendarTaskEntity entity)
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
                Status = _enumExtensions.StatusToString(entity.Status),
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

        public TaskItem CalendarTaskModelToGrpcTask(CalendarTaskModel model)
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
                Status = model.Status,
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

        public TaskItem CalendarTaskEntityToGrpcTask(CalendarTaskEntity entity)
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

        public TelegramTaskItem TelegramTaskItemToGrpcTask(CalendarTaskEntity entity)
        {
            return new TelegramTaskItem
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
