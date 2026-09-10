namespace Roomy.API.Data.Entities;

public class Hotel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Room> Rooms { get; set; } = new List<Room>();

    public Domain.Hotel ToDomain() =>
        new()
        {
            Name = Name,
            RoomCount = Rooms.Count
        };
}
