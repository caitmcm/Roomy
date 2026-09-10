using FastEndpoints;
using Roomy.API.Repositories;

namespace Roomy.API.Endpoints.Rooms.GetAvailableRooms;

public class GetAvailableRoomsEndpoint(IHotelRepository hotels, IRoomRepository rooms)
    : Endpoint<GetAvailableRoomsRequest, GetAvailableRoomsResponse>
{
    public override void Configure()
    {
        Get("/hotel/{hotelName}/room");
        AllowAnonymous();
        Summary(summary =>
        {
            summary.Summary = "Find rooms in a hotel available for a date range and number of guests.";
            summary.Description =
                "Returns the rooms in the hotel that can hold the requested number of guests and are available for the duration of their stay.\r\nSee README for check-in and check-out assumptions.";
            summary.RequestParam(request => request.HotelName, "The hotel's full name.");
            summary.RequestParam(request => request.From, "Check-in date. This must not in the past.");
            summary.RequestParam(request => request.To, "Check-out date. This must be later than the check-in date.");
            summary.RequestParam(request => request.Guests, "Number of guests to accommodate.");
            summary.Response<GetAvailableRoomsResponse>(200, "Available rooms in that date range.");
            summary.Response(404, "No hotel exists with that name.");
        });
    }

    public override async Task HandleAsync(GetAvailableRoomsRequest request, CancellationToken ct)
    {
        if (!await hotels.ExistsAsync(request.HotelName, ct))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var available = await rooms.FindAvailableAsync(request.HotelName, request.From, request.To, request.Guests, ct);

        await Send.OkAsync(
            new GetAvailableRoomsResponse
            {
                HotelName = request.HotelName,
                From = request.From,
                To = request.To,
                Nights = request.To.DayNumber - request.From.DayNumber,
                Guests = request.Guests,
                Rooms = [.. available
                    .Select(room => new AvailableRoom
                    {
                        RoomNumber = room.Number,
                        RoomTypeName = room.RoomType.Name,
                        Capacity = room.RoomType.Capacity
                    })]
            },
            ct);
    }
}
