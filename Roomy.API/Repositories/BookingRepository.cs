using Microsoft.EntityFrameworkCore;
using Roomy.API.Common;
using Roomy.API.Data;
using Roomy.API.Domain;
using BookingEntity = Roomy.API.Data.Entities.Booking;
using RoomEntity = Roomy.API.Data.Entities.Room;

namespace Roomy.API.Repositories;

public class BookingRepository(RoomyDbContext database) : IBookingRepository
{
    public async Task<bool> IsRoomAvailableAsync(string hotelName, int roomNumber, DateOnly from, DateOnly to, CancellationToken ct) =>
        !await database.Bookings
            .Where(booking => booking.Room.Hotel.Name == hotelName && booking.Room.Number == roomNumber)
            .WithinDateRange(from, to)
            .AnyAsync(ct);

    public async Task<Booking> CreateAsync(string hotelName, int roomNumber, DateOnly from, DateOnly to, int guests, string leadGuestName, CancellationToken ct)
    {
        var room = await ResolveRoomAsync(hotelName, roomNumber, ct);

        var created = new BookingEntity
        {
            RefNumber = BookingReference.Generate(),
            HotelId = room.HotelId,
            RoomId = room.Id,
            StartDate = from,
            EndDate = to,
            NumberOfGuests = guests,
            LeadGuestName = leadGuestName
        };

        database.Bookings.Add(created);
        await database.SaveChangesAsync(ct);

        var persisted = await WithRelations().SingleAsync(booking => booking.Id == created.Id, ct);

        return persisted.ToDomain();
    }

    public async Task<Booking?> GetByReferenceAsync(string refNumber, CancellationToken ct)
    {
        var booking = await WithRelations().SingleOrDefaultAsync(candidate => candidate.RefNumber == refNumber, ct);

        return booking?.ToDomain();
    }

    private async Task<RoomEntity> ResolveRoomAsync(string hotelName, int roomNumber, CancellationToken ct) =>
        await database.Rooms
            .AsNoTracking()
            .SingleOrDefaultAsync(room => room.Hotel.Name == hotelName && room.Number == roomNumber, ct)
        ?? throw new InvalidOperationException($"No room {roomNumber} exists in a hotel named '{hotelName}'.");

    private IQueryable<BookingEntity> WithRelations() =>
        database.Bookings
            .AsNoTracking()
            .Include(booking => booking.Hotel)
            .Include(booking => booking.Room)
            .ThenInclude(room => room.RoomType);
}
