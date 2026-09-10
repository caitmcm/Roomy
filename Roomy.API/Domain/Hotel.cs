namespace Roomy.API.Domain;

public record Hotel
{
    public required string Name { get; init; }

    public required int RoomCount { get; init; }
}
