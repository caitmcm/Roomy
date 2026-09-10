namespace Roomy.API.Data.Entities;

public class Booking
{
    public int Id { get; set; }

    public string RefNumber { get; set; } = string.Empty;

    public int HotelId { get; set; }

    public int RoomId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int NumberOfGuests { get; set; }

    public string LeadGuestName { get; set; } = string.Empty;

    public Hotel Hotel { get; set; } = null!;

    public Room Room { get; set; } = null!;

    public Domain.Booking ToDomain() =>
        new()
        {
            RefNumber = RefNumber,
            HotelName = Hotel.Name,
            RoomNumber = Room.Number,
            RoomType = Room.RoomType.ToDomain(),
            StartDate = StartDate,
            EndDate = EndDate,
            NumberOfGuests = NumberOfGuests,
            LeadGuestName = LeadGuestName
        };
}
