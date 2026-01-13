using DeskAssistant.Core.Models;
using DeskAssistant.Core.Services;
using DeskAssistantGrpcService.DataBase;
using Microsoft.EntityFrameworkCore;
using NLog;
using Telegram.Bot.Types;
using ILogger = NLog.ILogger;

namespace DeskAssistantGrpcService.Services
{
    public class BirthdayServiceImpl : IBirthdaysService
    {
        private readonly IDbContextFactory<BirthdaysDbContext> _contextFactory;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();


        public BirthdayServiceImpl(IDbContextFactory<BirthdaysDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }



        public async Task AddBirthdaysToDbAsync(BirthdaysEntity birthdaysEntity)
        {
            if (birthdaysEntity == null)
                return;

            await using var context = _contextFactory.CreateDbContext();

            bool exists = await context.Birthdays
                .AnyAsync(b => b.Email == birthdaysEntity.Email &&
                      b.Birthday == birthdaysEntity.Birthday);

            if (exists)
                return;

            try
            {
                context.Birthdays.Add(birthdaysEntity);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.Warn(ex, "Duplicate birthday detected");
            }
        }

        public async Task<List<BirthdaysEntity>> GetBirthdaysFromDbAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            var response = await context.Birthdays.ToListAsync();
            return response;
        }

        public async Task<List<BirthdaysEntity>> GetBirthdaysForTodayAsync()
        {
            var today = DateTime.Today;

            using var context = _contextFactory.CreateDbContext();

            var response = await context.Birthdays.Where(day => day.Birthday == today).ToListAsync();
            return response;
        }
    }
}
