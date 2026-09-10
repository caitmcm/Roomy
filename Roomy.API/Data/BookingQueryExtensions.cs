using Roomy.API.Data.Entities;

namespace Roomy.API.Data;

public static class BookingQueryExtensions
{
    public static IQueryable<Booking> Overlapping(this IQueryable<Booking> bookings, DateOnly from, DateOnly to) =>
        bookings.Where(booking => booking.StartDate < to && booking.EndDate > from);
}
