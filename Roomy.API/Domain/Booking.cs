namespace Roomy.API.Domain;

public record Booking
{
    public required string RefNumber { get; init; }

    public required string HotelName { get; init; }

    public required int RoomNumber { get; init; }

    public required RoomType RoomType { get; init; }

    public required DateOnly StartDate { get; init; }

    public required DateOnly EndDate { get; init; }

    public required int NumberOfGuests { get; init; }

    public required string LeadGuestName { get; init; }

    public int Nights => EndDate.DayNumber - StartDate.DayNumber;
}
