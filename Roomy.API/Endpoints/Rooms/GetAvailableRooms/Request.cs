namespace Roomy.API.Endpoints.Rooms.GetAvailableRooms;

public class GetAvailableRoomsRequest
{
    public string HotelName { get; set; } = string.Empty;

    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public int Guests { get; set; }
}
