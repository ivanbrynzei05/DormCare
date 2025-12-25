using Microsoft.EntityFrameworkCore;
using DormCare.Models;

namespace DormCare.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Request> Requests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", FullName = "Administrator", Email = "admin@dorm.local", Password = "admin123", Phone = "+380000000000", Role = "Admin" },
                new User { Id = 2, Username = "ivanov", FullName = "Ivan Ivanov", Email = "ivanov@student.local", Password = "password1", Phone = "+380501111111", Role = "Student" },
                new User { Id = 3, Username = "petrenko", FullName = "Olena Petrenko", Email = "petrenko@student.local", Password = "password2", Phone = "+380502222222", Role = "Student" },
                new User { Id = 4, Username = "shevchenko", FullName = "Taras Shevchenko", Email = "shevchenko@student.local", Password = "password3", Phone = "+380503333333", Role = "Student" },
                new User { Id = 5, Username = "koval", FullName = "Anna Koval", Email = "koval@student.local", Password = "password4", Phone = "+380504444444", Role = "Student" },
                new User { Id = 6, Username = "melnyk", FullName = "Oksana Melnyk", Email = "melnyk@student.local", Password = "password5", Phone = "+380505555555", Role = "Student" }
            );

            modelBuilder.Entity<Room>().HasData(
                new Room { Id = 1, Number = "101A", Capacity = 2 },
                new Room { Id = 2, Number = "102B", Capacity = 3 },
                new Room { Id = 3, Number = "103C", Capacity = 2 }
            );

            modelBuilder.Entity<Student>().HasData(
                new Student { Id = 1, UserId = 2, RoomId = 1 },
                new Student { Id = 2, UserId = 3, RoomId = 1 },
                new Student { Id = 3, UserId = 4, RoomId = 2 },
                new Student { Id = 4, UserId = 5, RoomId = 2 },
                new Student { Id = 5, UserId = 6, RoomId = 3 }
            );

            modelBuilder.Entity<Request>().HasData(
                new Request { Id = 1, StudentId = 1, Text = "Leaky faucet in room", Type = "Maintenance", Status = "New", CreatedAt = new System.DateTime(2025, 12, 24, 10, 0, 0) },
                new Request { Id = 2, StudentId = 2, Text = "Request for extra blanket", Type = "Service", Status = "Done", CreatedAt = new System.DateTime(2025, 12, 23, 15, 30, 0) },
                new Request { Id = 3, StudentId = 3, Text = "Broken window latch", Type = "Maintenance", Status = "New", CreatedAt = new System.DateTime(2025, 12, 22, 9, 0, 0) }
            );
        }
    }
}
