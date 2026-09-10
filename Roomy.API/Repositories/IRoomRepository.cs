using Roomy.API.Domain;

namespace Roomy.API.Repositories;

public interface IRoomRepository
{
    Task<IReadOnlyList<Room>> FindAvailableAsync(string hotelName, DateOnly from, DateOnly to, int guests, CancellationToken ct);

    Task<Room?> GetAsync(string hotelName, int roomNumber, CancellationToken ct);
}
