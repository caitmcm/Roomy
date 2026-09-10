namespace Roomy.API.Endpoints.Rooms.GetAvailableRooms;

public class GetAvailableRoomsResponse
{
    public string HotelName { get; set; } = string.Empty;

    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public int Nights { get; set; }

    public int Guests { get; set; }

    public IReadOnlyList<AvailableRoom> Rooms { get; set; } = [];
}

public class AvailableRoom
{
    public int RoomNumber { get; set; }

    public string RoomTypeName { get; set; } = string.Empty;

    public int Capacity { get; set; }
}
