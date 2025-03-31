using HotelReservation.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }  // ✅ Register Reservations Table
        public DbSet<Payment> Payments { get; set; }  // ✅ Register Payments Table

        // ✅ Register new Housekeeping-related Tables
        public DbSet<CleaningLog> CleaningLogs { get; set; }
        public DbSet<HousekeepingRequest> HousekeepingRequests { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // ✅ Enforce unique constraint on User Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();


            // ✅ Enforce unique constraint on RoomNumber in Rooms
            modelBuilder.Entity<Room>()
                .HasIndex(r => r.RoomNumber)
                .IsUnique();

            // ✅ Explicitly define decimal precision for Room Price
            modelBuilder.Entity<Room>()
                .Property(r => r.Price)
                .HasColumnType("decimal(10,2)");

            // ✅ Define decimal precision for Payments
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(10,2)");

            // ✅ Fix Foreign Key: Reservation ↔ Room
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Room)
                .WithMany()
                .HasForeignKey(r => r.RoomId)
                .OnDelete(DeleteBehavior.Restrict);  // 🔥 Prevents NULL issue

            // ✅ Configure Foreign Key Relationship: Reservation ↔ User (Nullable for Guests)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // ✅ Configure Foreign Key Relationship: Reservation ↔ Payment
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Payment)
                .WithOne(p => p.Reservation)
                .HasForeignKey<Payment>(p => p.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);  // If reservation is deleted, payment is also deleted


            // ✅ Fix Foreign Key: CheckedInBy (Front Desk User)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.CheckedInByUser)
                .WithMany()
                .HasForeignKey(r => r.CheckedInBy)
                .OnDelete(DeleteBehavior.Restrict);  // 🔥 Prevents multiple cascade paths
        }
    }
}
