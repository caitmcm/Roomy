using Microsoft.EntityFrameworkCore;
using Roomy.API.Data;
using Roomy.API.Domain;

namespace Roomy.API.Repositories;

public class HotelRepository(RoomyDbContext database) : IHotelRepository
{
    public async Task<IReadOnlyList<Hotel>> FindByNameAsync(string? name, CancellationToken ct)
    {
        var queryBase = database.Hotels
            .AsNoTracking()
            .Include(hotel => hotel.Rooms);

        var query = string.IsNullOrWhiteSpace(name)
            ? queryBase.AsQueryable()
            : queryBase.Where(hotel => EF.Functions.Like(hotel.Name, $"%{name}%"));

        var matches = await query
            .OrderBy(hotel => hotel.Name)
            .ToListAsync(ct);

        return [.. matches.Select(hotel => hotel.ToDomain())];
    }

    public Task<bool> ExistsAsync(string hotelName, CancellationToken ct) =>
        database.Hotels.AnyAsync(hotel => hotel.Name == hotelName, ct);
}
