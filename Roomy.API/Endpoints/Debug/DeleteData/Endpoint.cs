using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Roomy.API.Data;

namespace Roomy.API.Endpoints.Debug.DeleteData;

public class DeleteDataEndpoint(RoomyDbContext database) : EndpointWithoutRequest<DeleteDataResponse>
{
    public override void Configure()
    {
        Delete("/debug");
        AllowAnonymous();
        Summary(summary =>
        {
            summary.Summary = "Remove all data.";
            summary.Description = "Removes all data.";
            summary.Response<DeleteDataResponse>(200, "Counts of the records removed.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var bookings = await database.Bookings.ExecuteDeleteAsync(ct);
        var rooms = await database.Rooms.ExecuteDeleteAsync(ct);
        var hotels = await database.Hotels.ExecuteDeleteAsync(ct);

        await Send.OkAsync(
            new DeleteDataResponse
            {
                Hotels = hotels,
                Rooms = rooms,
                Bookings = bookings
            },
            ct);
    }
}
