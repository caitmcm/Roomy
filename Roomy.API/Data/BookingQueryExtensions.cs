using Roomy.API.Data.Entities;

namespace Roomy.API.Data;

public static class BookingQueryExtensions
{
    /// <summary>
    /// The bookings that clash with the requested dates. Back-to-back stays are fine because guests are out before midday and the next lot arrive in the afternoon.
    /// See README for check-in/check-out assumptions.
    /// </summary>
    public static IQueryable<Booking> Overlapping(this IQueryable<Booking> bookings, DateOnly from, DateOnly to) =>
        bookings.Where(booking => booking.StartDate < to && booking.EndDate > from);
}
