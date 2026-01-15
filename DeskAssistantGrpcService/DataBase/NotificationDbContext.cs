using DeskAssistant.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskAssistantGrpcService.DataBase
{
    public class NotificationDbContext : DbContext
    {
        public DbSet<NotificationEntity> Notifications { get; set; }


        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationEntity>(entity =>
            {
                entity.ToTable("Notifications");

                entity.HasKey(notif => notif.Id).HasName("PK_Notification");

                entity.Property(notif => notif.ClientId).HasMaxLength(128).IsRequired();
                entity.Property(notif => notif.IsEnabled).HasDefaultValue(false);
                entity.Property(notif => notif.NotificationTime);

                entity.Property(notif => notif.TimerId).HasMaxLength(36).IsRequired(false);

                entity.Property(notif => notif.MondayEnabled).HasDefaultValue(false);
                entity.Property(notif => notif.TuesdayEnabled).HasDefaultValue(false);
                entity.Property(notif => notif.WednesdayEnabled).HasDefaultValue(false);
                entity.Property(notif => notif.ThursdayEnabled).HasDefaultValue(false);
                entity.Property(notif => notif.FridayEnabled).HasDefaultValue(false);
                entity.Property(notif => notif.SaturdayEnabled).HasDefaultValue(false);
                entity.Property(notif => notif.SundayEnabled).HasDefaultValue(false);

                entity.Property(notif => notif.CreatedAt).HasDefaultValueSql("NOW()").IsRequired();

                entity.Property(notif => notif.IsSentToday).HasDefaultValue(false);

                entity.HasIndex(notif => notif.TimerId).IsUnique().HasFilter("\"TimerId\" IS NOT NULL");
            });
        }
    }
}
