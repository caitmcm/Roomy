namespace Roomy.API.Endpoints.Bookings.GetBookingByRef;

public class GetBookingByRefResponse
{
    public string RefNumber { get; set; } = string.Empty;

    public string HotelName { get; set; } = string.Empty;

    public int RoomNumber { get; set; }

    public string RoomTypeName { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int Nights { get; set; }

    public int NumberOfGuests { get; set; }

    public string LeadGuestName { get; set; } = string.Empty;
}
