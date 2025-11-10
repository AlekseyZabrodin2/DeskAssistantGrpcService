using DeskAssistant.Core.Models;
using DeskAssistant.Core.Services;
using DeskAssistantGrpcService.DataBase;
using Microsoft.EntityFrameworkCore;
using NLog;
using ILogger = NLog.ILogger;

namespace DeskAssistantGrpcService.Services
{
    public class TaskServiceImpl : ITaskService
    {
        private readonly IDbContextFactory<TasksDbContext> _contextFactory;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public TaskServiceImpl(IDbContextFactory<TasksDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }


        public async Task<List<CalendarTaskEntity>> GetAllTasksAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            var response = await context.Tasks.ToListAsync();
            return response;
        }

        public async Task AddTaskForSelectedDate(CalendarTaskEntity taskEntity)
        {
            if (string.IsNullOrEmpty(taskEntity.Name) ||
                string.IsNullOrEmpty(taskEntity.Description))
                return;

            using var context = _contextFactory.CreateDbContext();

            await context.Tasks.AddAsync(taskEntity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateTaskAsync(CalendarTaskModel model, TaskStatusEnum status)
        {
            using var context = _contextFactory.CreateDbContext();
            var entity = await context.Tasks.FindAsync(model.Id);
            if (entity != null)
            {
                entity.Name = model.Name;
                entity.Description = model.Description;
                entity.DueDate = model.DueDate;
                entity.IsCompleted = model.IsCompleted;
                entity.Priority = model.Priority;
                entity.Category = model.Category;
                entity.Status = status;
                entity.Tags = model.Tags;
                entity.DueTime = model.DueTime;
                entity.ReminderTime = model.ReminderTime;
                entity.CompletedDate = model.CompletedDate;
                entity.IsRecurring = model.IsRecurring;
                entity.RecurrencePattern = model.RecurrencePattern;
                entity.Duration = model.Duration;

                context.SaveChanges();
            }
        }

        public Task<CalendarTaskEntity> AddTaskAsync(CalendarTaskEntity task)
        {
            throw new NotImplementedException();
        }

        public Task<List<CalendarTaskEntity>> GetTasksForDateAsync(DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<CalendarTaskEntity> GetTaskByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteTaskAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
