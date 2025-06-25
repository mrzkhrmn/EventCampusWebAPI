using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EventCampusAPI.Entities;

namespace EventCampusAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<University> Universities { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Event - Category ilişkisi
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Event - User (Creator) ilişkisi
            modelBuilder.Entity<Event>()
                .HasOne(e => e.CreatedByUser)
                .WithMany(u => u.CreatedEvents)
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Event - University ilişkisi
            modelBuilder.Entity<Event>()
                .HasOne(e => e.University)
                .WithMany(u => u.Events)
                .HasForeignKey(e => e.UniversityId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // EventParticipant - Event ilişkisi
            modelBuilder.Entity<EventParticipant>()
                .HasOne(ep => ep.Event)
                .WithMany(e => e.Participants)
                .HasForeignKey(ep => ep.EventId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // EventParticipant - User ilişkisi
            modelBuilder.Entity<EventParticipant>()
                .HasOne(ep => ep.User)
                .WithMany(u => u.EventParticipations)
                .HasForeignKey(ep => ep.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // User - University ilişkisi
            modelBuilder.Entity<User>()
                .HasOne(u => u.University)
                .WithMany(un => un.Users)
                .HasForeignKey(u => u.UniversityId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // User - Faculty ilişkisi
            modelBuilder.Entity<User>()
                .HasOne(u => u.Faculty)
                .WithMany(f => f.Users)
                .HasForeignKey(u => u.FacultyId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // User - Department ilişkisi
            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Faculty - University ilişkisi
            modelBuilder.Entity<Faculty>()
                .HasOne(f => f.University)
                .WithMany(u => u.Faculties)
                .HasForeignKey(f => f.UniversityId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Department - Faculty ilişkisi
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Faculty)
                .WithMany(f => f.Departments)
                .HasForeignKey(d => d.FacultyId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Unique constraint for EventParticipant (Bir kullanıcı aynı eventi'e birden fazla kez katılamaz)
            modelBuilder.Entity<EventParticipant>()
                .HasIndex(ep => new { ep.EventId, ep.UserId })
                .IsUnique();
                
            // Event index'leri
            modelBuilder.Entity<Event>()
                .HasIndex(e => e.StartDate);
                
            modelBuilder.Entity<Event>()
                .HasIndex(e => e.UniversityId);
                
            modelBuilder.Entity<Event>()
                .HasIndex(e => e.CategoryId);
        }
    }
}