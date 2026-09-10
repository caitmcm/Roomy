namespace Roomy.API.Domain;

public record RoomType
{
    public required string Name { get; init; }

    public required int Capacity { get; init; }
}
