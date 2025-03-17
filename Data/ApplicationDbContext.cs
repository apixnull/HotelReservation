using HotelReservation.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }  // ✅ Register 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enforce unique constraint on the Email property in the Users table.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // ✅ Explicitly define decimal precision for Room Price
            modelBuilder.Entity<Room>()
                .Property(r => r.Price)
                .HasColumnType("decimal(10,2)");

            // ✅ Enforce unique constraint on RoomNumber in Rooms table
            modelBuilder.Entity<Room>()
                .HasIndex(r => r.RoomNumber)
                .IsUnique();
        }
    }
}   
