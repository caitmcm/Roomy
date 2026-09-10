namespace Roomy.API.Endpoints.Bookings.CreateBooking;

public class CreateBookingRequest
{
    public string HotelName { get; set; } = string.Empty;

    public int RoomNumber { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int NumberOfGuests { get; set; }

    public string LeadGuestName { get; set; } = string.Empty;
}
