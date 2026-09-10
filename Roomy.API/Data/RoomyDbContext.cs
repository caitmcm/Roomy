using Microsoft.EntityFrameworkCore;
using Roomy.API.Data.Entities;

namespace Roomy.API.Data;

public class RoomyDbContext(DbContextOptions<RoomyDbContext> options) : DbContext(options)
{
    public DbSet<Hotel> Hotels => Set<Hotel>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<RoomType> RoomTypes => Set<RoomType>();

    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hotel>(hotel =>
        {
            hotel.Property(h => h.Name).IsRequired().HasMaxLength(200).UseCollation("NOCASE");
            hotel.HasIndex(h => h.Name).IsUnique();
        });

        modelBuilder.Entity<RoomType>(roomType =>
        {
            roomType.Property(t => t.Name).IsRequired().HasMaxLength(50);

            roomType.HasData(
                new RoomType { Id = 1, Name = "Single", Capacity = 1 },
                new RoomType { Id = 2, Name = "Double", Capacity = 2 },
                new RoomType { Id = 3, Name = "Deluxe", Capacity = 3 });
        });

        modelBuilder.Entity<Room>(room =>
        {
            room.HasOne(r => r.Hotel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            room.HasOne(r => r.RoomType)
                .WithMany(t => t.Rooms)
                .HasForeignKey(r => r.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            room.HasIndex(r => new { r.HotelId, r.Number }).IsUnique();
        });

        modelBuilder.Entity<Booking>(booking =>
        {
            booking.Property(b => b.RefNumber).IsRequired().HasMaxLength(20);
            booking.HasIndex(b => b.RefNumber).IsUnique();

            booking.Property(b => b.LeadGuestName).IsRequired().HasMaxLength(200);

            booking.HasOne(b => b.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            booking.HasOne(b => b.Hotel)
                .WithMany()
                .HasForeignKey(b => b.HotelId)
                .OnDelete(DeleteBehavior.Restrict);

            booking.HasIndex(b => new { b.RoomId, b.StartDate, b.EndDate });
        });
    }
}
