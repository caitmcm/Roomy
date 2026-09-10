using Roomy.API.Domain;

namespace Roomy.API.Repositories;

public interface IBookingRepository
{
    Task<bool> IsRoomAvailableAsync(string hotelName, int roomNumber, DateOnly from, DateOnly to, CancellationToken ct);

    Task<Booking> CreateAsync(string hotelName, int roomNumber, DateOnly from, DateOnly to, int guests, string leadGuestName, CancellationToken ct);

    Task<Booking?> GetByReferenceAsync(string refNumber, CancellationToken ct);
}
