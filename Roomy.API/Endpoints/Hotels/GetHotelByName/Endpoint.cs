using FastEndpoints;
using Roomy.API.Repositories;

namespace Roomy.API.Endpoints.Hotels.GetHotelByName;

public class GetHotelByNameEndpoint(IHotelRepository hotels) : Endpoint<GetHotelByNameRequest, GetHotelByNameResponse>
{
    public override void Configure()
    {
        Get("/hotel");
        AllowAnonymous();
        Summary(summary =>
        {
            summary.Summary = "Find hotels by name, or list them all.";
            summary.Description =
                "Find a hotel by full or partial name, or find all hotels by leaving the name blank.";
            summary.RequestParam(request => request.Name, "Full or partial hotel name to search for. Leave blank to list every hotel.");
            summary.Response<GetHotelByNameResponse>(200, "All or matching hotels");
        });
    }

    public override async Task HandleAsync(GetHotelByNameRequest request, CancellationToken ct)
    {
        var matches = await hotels.FindByNameAsync(request.Name, ct);

        await Send.OkAsync(
            new GetHotelByNameResponse
            {
                Hotels = [.. matches
                    .Select(hotel => new HotelMatch
                    {
                        Name = hotel.Name,
                        RoomCount = hotel.RoomCount
                    })]
            },
            ct);
    }
}
