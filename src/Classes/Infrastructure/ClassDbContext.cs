using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class ClassDbContext : DbContext
    {
        public ClassDbContext(DbContextOptions<ClassDbContext> options)
            : base(options)
        {
        }

        public DbSet<FitnessClass> FitnessClasses { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // FitnessClass configuration
            modelBuilder.Entity<FitnessClass>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.InstructorId)
                    .IsRequired();

                entity.Property(e => e.InstructorName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ScheduledAt)
                    .IsRequired();

                entity.Property(e => e.Capacity)
                    .IsRequired();

                entity.Property(e => e.BookedCount)
                    .IsRequired();

                // Relationships
                entity.HasMany(e => e.Bookings)
                    .WithOne(b => b.Class)
                    .HasForeignKey(b => b.ClassId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Booking configuration
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ClassId)
                    .IsRequired();

                entity.Property(e => e.MemberId)
                    .IsRequired();

                entity.Property(e => e.BookedAt)
                    .IsRequired();

                // Unique constraint: prevent duplicate booking for same class and member
                entity.HasIndex(e => new { e.ClassId, e.MemberId })
                    .IsUnique();
            });
        }
    }
}
