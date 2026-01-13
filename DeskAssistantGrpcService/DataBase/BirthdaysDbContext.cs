using DeskAssistant.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskAssistantGrpcService.DataBase
{
    public class BirthdaysDbContext : DbContext
    {
        public DbSet<BirthdaysEntity> Birthdays { get; set; }

        public BirthdaysDbContext(DbContextOptions<BirthdaysDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BirthdaysEntity>(entity =>
            {
                entity.ToTable("Birthdays");

                entity.HasKey(birthday => birthday.Id).HasName("PK_Birthday");

                entity.Property(birthday => birthday.LastName).HasMaxLength(50).IsRequired();
                entity.Property(birthday => birthday.Name).HasMaxLength(20).IsRequired();
                entity.Property(birthday => birthday.MiddleName).HasMaxLength(20).IsRequired();

                entity.Property(birthday => birthday.Birthday).HasColumnType("date").IsRequired();
                entity.Property(birthday => birthday.Email).HasMaxLength(128).IsRequired();
            });
        }
    }
}