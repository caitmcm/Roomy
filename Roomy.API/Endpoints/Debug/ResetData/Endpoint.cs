using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Roomy.API.Data;

namespace Roomy.API.Endpoints.Debug.ResetData;

public class ResetDataEndpoint(RoomyDbContext database) : EndpointWithoutRequest<ResetDataResponse>
{
    public override void Configure()
    {
        Delete("/debug");
        AllowAnonymous();
        Summary(summary =>
        {
            summary.Summary = "Remove all data, ready for seeding.";
            summary.Description =
                "Deletes every booking, room and hotel. The three room types are reference data rather than "
                + "test data and are left in place, since the API cannot function without them.";
            summary.Response<ResetDataResponse>(200, "Counts of the records removed.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var bookings = await database.Bookings.ExecuteDeleteAsync(ct);
        var rooms = await database.Rooms.ExecuteDeleteAsync(ct);
        var hotels = await database.Hotels.ExecuteDeleteAsync(ct);

        await Send.OkAsync(
            new ResetDataResponse
            {
                Hotels = hotels,
                Rooms = rooms,
                Bookings = bookings
            },
            ct);
    }
}
