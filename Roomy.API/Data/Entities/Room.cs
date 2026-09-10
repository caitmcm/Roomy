namespace Roomy.API.Data.Entities;

public class Room
{
    public int Id { get; set; }

    public int Number { get; set; }

    public int HotelId { get; set; }

    public int RoomTypeId { get; set; }

    public Hotel Hotel { get; set; } = null!;

    public RoomType RoomType { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public Domain.Room ToDomain() =>
        new()
        {
            Number = Number,
            HotelName = Hotel.Name,
            RoomType = RoomType.ToDomain()
        };
}
