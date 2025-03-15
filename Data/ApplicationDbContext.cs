using HotelReservation.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }  // ✅ Register Room
        public DbSet<RoomImage> RoomImages { get; set; }  // ✅ Register RoomImage

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enforce unique constraint on the Email property in the Users table.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Define one-to-many relationship between Room and RoomImage
            modelBuilder.Entity<RoomImage>()
                .HasOne(ri => ri.Room)
                .WithMany(r => r.Images) 
                .HasForeignKey(ri => ri.RoomId)
                .OnDelete(DeleteBehavior.Cascade); // If a room is deleted, its images are also deleted

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
