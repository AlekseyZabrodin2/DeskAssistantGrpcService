using DeskAssistant.Core.Models;
using DeskAssistant.Core.Services;
using DeskAssistantGrpcService.DataBase;
using DeskAssistantGrpcService.Extensions;
using DeskAssistantGrpcService.Models;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Collections.Concurrent;
using ILogger = NLog.ILogger;

namespace DeskAssistantGrpcService.Helpers
{
    public class NotificationTimerHelper : IDisposable
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly ConcurrentDictionary<Guid, NotificationTimer> _notificationTimers = new();
        private readonly ConcurrentDictionary<string, Guid> _notificationIdToTimerMap = new();
        private CalendarTasksExtensions _calendarExtensions = new();
        private readonly IDbContextFactory<TasksDbContext> _contextTasksDb;
        private readonly ITelegramNotificationService _telegramService;
        private bool _disposed = false;


        public NotificationTimerHelper(IDbContextFactory<TasksDbContext> contextTasksDb,
            ITelegramNotificationService telegramService)
        {
            _contextTasksDb = contextTasksDb;
            _telegramService = telegramService;
        }


        public void GraficsNotificationTimers(NotificationEntity notification)
        {
            if (!notification.IsEnabled)
            {
                _logger.Info($"Уведомление {notification.Id} отключено, пропускаем");
                return;
            }

            var timerId = Guid.NewGuid();
            var (delay, nextAlarm) = ScheduleNotification(notification, timerId);

            var timer = new Timer(async _ =>
            {
                try
                {
                    _logger.Trace($"Уведомление - [{notification.Id}], статус отправления - [{notification.IsSentToday}]");

                    if (!notification.IsSentToday)
                    {
                        _logger.Info($"\n🔔 УВЕДОМЛЕНИЕ СРАБОТАЛО!\n" +
                                                $"   ├─ ID: {notification.Id}\n" +
                                                $"   ├─ Клиент: {notification.ClientId}\n" +
                                                $"   ├─ Время: {DateTime.Now:HH:mm:ss}\n" +
                                                $"   └─ Запланированное время: {notification.NotificationTime:hh\\:mm}");

                        await SendNotificationsForTodayAsync();

                        notification.IsSentToday = true;

                        _logger.Info($"✅ Уведомление - [{notification.Id}] отправлено: {notification.IsSentToday}");

                        RemoveTimer(timerId);

                        _logger.Info($"🔄 Перепланирование уведомления {notification.Id}...");
                        GraficsNotificationTimers(notification);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Ошибка при отправке уведомления {notification.Id}");
                    RemoveTimer(timerId);
                    GraficsNotificationTimers(notification);
                }
            }, null, delay, Timeout.InfiniteTimeSpan);

            CreateNotificationTimersDictionary(notification, timerId, timer, nextAlarm);
        }

        public async Task<List<CalendarTaskEntity>> GetTodayTasksAsync()
        {
            await using var context = _contextTasksDb.CreateDbContext();
            var tasks = await context.Tasks.ToListAsync();

            var todayTasks = tasks.Where(task => task.DueDate == DateOnly.FromDateTime(DateTime.Now)).ToList();

            return todayTasks;
        }

        public async Task SendNotificationsForTodayAsync()
        {
            try
            {
                var todayTasks = await GetTodayTasksAsync();

                foreach (var taskItem in todayTasks)
                {
                    var taskModel = _calendarExtensions.TaskEntityToCalendarTaskModel(taskItem);

                    _ = Task.Run(async () => _telegramService.NotifycationFromClientAsync(taskModel));

                    _logger.Info($"Уведомление для задачи - '{taskModel.Id}' успешно отправлено");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "gRPC: Ошибка отправки уведомления");
            }
        }

        private bool IsNotificationAlreadyScheduled(NotificationEntity notificationId)
        {
            return _notificationIdToTimerMap.ContainsKey(notificationId.Id);
        }

        private (TimeSpan delay, DateTime nextAlarm) ScheduleNotification(NotificationEntity notification, Guid timerId)
        {
            var nextAlarm = GetNextAlarmDateTime(notification);
            var now = DateTime.Now;
            var delay = nextAlarm - now;

            if (delay < TimeSpan.Zero)
            {
                // уведомление уже прошло — берём следующий ближайший день
                nextAlarm = GetNextAlarmDateTime(notification, after: true);
                delay = nextAlarm - DateTime.Now;
            }

            var timerInfo = $"\n📅 ПЛАНИРОВАНИЕ УВЕДОМЛЕНИЯ\n" +
                    $"   ├─ ID таймера: {timerId}\n" +
                    $"   ├─ ID уведомления: {notification.Id}\n" +
                    $"   ├─ Для клиента: {notification.ClientId}\n" +
                    $"   ├─ Запланировано на: {nextAlarm}\n" +
                    $"   ├─ Задержка: {delay.TotalMinutes:F1} минут\n" +
                    $"   └─ Время планирования: {now:HH:mm:ss}";

            _logger.Info(timerInfo);

            return (delay, nextAlarm);
        }

        private void CreateNotificationTimersDictionary(NotificationEntity notification, Guid timerId, Timer timer, DateTime nextAlarm)
        {
            if (IsNotificationAlreadyScheduled(notification))
            {
                _logger.Warn($"⚠️ Уведомление {notification.Id} уже запланировано. Пропускаем дублирование.");
                return;
            }

            var notificationTimer = new NotificationTimer
            {
                Id = timerId,
                NotificationId = notification.Id,
                Timer = timer,
                ScheduledTime = nextAlarm,
                Notification = notification
            };

            notification.IsSentToday = false;
            _logger.Trace($"Таймер - [{notification.Id}] : [{notification.IsSentToday}]");

            _notificationTimers[timerId] = notificationTimer;
            _notificationIdToTimerMap[notification.Id] = timerId;

            _logger.Debug($"Добавлен таймер {timerId} в коллекцию. Всего таймеров: {_notificationTimers.Count}");
        }


        private void RemoveTimer(Guid timerId)
        {
            // Удаляем таймер из коллекции после выполнения
            if (_notificationTimers.TryRemove(timerId, out var timerInfo))
            {
                timerInfo.Timer?.Dispose();

                _notificationIdToTimerMap.TryRemove(timerInfo.NotificationId, out _);

                _logger.Debug($"🗑️ Таймер {timerId} удален из коллекции");
                _logger.Debug($"📊 Статистика: Уведомлений - {_notificationIdToTimerMap.Count}, Таймеров - {_notificationTimers.Count}");
            }
        }

        public void StopAllNotificationTimers()
        {
            _logger.Info("🛑 Остановка всех таймеров уведомлений...");

            var timersToStop = _notificationTimers.Values.ToList();
            _notificationTimers.Clear();
            _notificationIdToTimerMap.Clear();

            foreach (var timerInfo in timersToStop)
            {
                try
                {
                    timerInfo.Timer?.Dispose();
                    _logger.Debug($"Остановлен таймер для уведомления {timerInfo.NotificationId}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Ошибка остановки таймера {timerInfo.Id}");
                }
            }

            _logger.Info($"✅ Остановлено {timersToStop.Count} таймеров");
        }        

        DateTime GetNextAlarmDateTime(NotificationEntity notification, bool after = false)
        {
            if (notification.NotificationTime == null)
                throw new ArgumentException("NotificationTime не установлен", nameof(notification));

            var now = DateTime.Now;
            var days = GetAlarmDays(notification);

            for (int i = after ? 1 : 0; i < 7; i++) // начинаем с 1, если after = true
            {
                var candidateDate = now.Date.AddDays(i);
                var candidate = candidateDate + notification.NotificationTime;

                if (days.Contains(candidate.DayOfWeek) && candidate > now)
                    return candidate;
            }

            return now.Date.AddDays(7) + notification.NotificationTime;
        }

        private List<DayOfWeek> GetAlarmDays(NotificationEntity notification)
        {
            var alarmDay = new List<DayOfWeek>();

            if (notification.MondayEnabled) alarmDay.Add(DayOfWeek.Monday);
            if (notification.TuesdayEnabled) alarmDay.Add(DayOfWeek.Tuesday);
            if (notification.WednesdayEnabled) alarmDay.Add(DayOfWeek.Wednesday);
            if (notification.ThursdayEnabled) alarmDay.Add(DayOfWeek.Thursday);
            if (notification.FridayEnabled) alarmDay.Add(DayOfWeek.Friday);
            if (notification.SaturdayEnabled) alarmDay.Add(DayOfWeek.Saturday);
            if (notification.SundayEnabled) alarmDay.Add(DayOfWeek.Sunday);

            return alarmDay;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                StopAllNotificationTimers();
                _disposed = true;
            }
        }
    }
}
