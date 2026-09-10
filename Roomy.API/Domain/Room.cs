namespace Roomy.API.Domain;

public record Room
{
    public required int Number { get; init; }

    public required string HotelName { get; init; }

    public required RoomType RoomType { get; init; }
}
