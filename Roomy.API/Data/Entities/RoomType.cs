namespace Roomy.API.Data.Entities;

public class RoomType
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public ICollection<Room> Rooms { get; set; } = new List<Room>();

    public Domain.RoomType ToDomain() =>
        new()
        {
            Name = Name,
            Capacity = Capacity
        };
}
