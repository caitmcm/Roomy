namespace Roomy.API.Endpoints.Hotels.GetHotelByName;

public class GetHotelByNameResponse
{
    public IReadOnlyList<HotelMatch> Hotels { get; set; } = [];
}

public class HotelMatch
{
    public string Name { get; set; } = string.Empty;

    public int RoomCount { get; set; }
}
