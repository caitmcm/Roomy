using Microsoft.EntityFrameworkCore;
using Roomy.API.Data;
using Roomy.API.Domain;

namespace Roomy.API.Repositories;

public class RoomRepository(RoomyDbContext database) : IRoomRepository
{
    public async Task<IReadOnlyList<Room>> FindAvailableAsync(string hotelName, DateOnly from, DateOnly to, int guests, CancellationToken ct)
    {
        var occupiedRoomIds = database.Bookings
            .Overlapping(from, to)
            .Select(booking => booking.RoomId);

        var available = await WithRelations()
            .Where(room => room.Hotel.Name == hotelName
                && room.RoomType.Capacity >= guests
                && !occupiedRoomIds.Contains(room.Id))
            .OrderBy(room => room.Number)
            .ToListAsync(ct);

        return [.. available.Select(room => room.ToDomain())];
    }

    public async Task<Room?> GetAsync(string hotelName, int roomNumber, CancellationToken ct)
    {
        var room = await WithRelations()
            .SingleOrDefaultAsync(candidate => candidate.Hotel.Name == hotelName && candidate.Number == roomNumber, ct);

        return room?.ToDomain();
    }

    private IQueryable<Data.Entities.Room> WithRelations() =>
        database.Rooms
            .AsNoTracking()
            .Include(room => room.Hotel)
            .Include(room => room.RoomType);
}
