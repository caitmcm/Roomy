using Roomy.API.Domain;

namespace Roomy.API.Repositories;

public interface IHotelRepository
{
    Task<IReadOnlyList<Hotel>> FindByNameAsync(string? name, CancellationToken ct);

    Task<bool> ExistsAsync(string hotelName, CancellationToken ct);
}
