using FastEndpoints;
using Roomy.API.Endpoints.Bookings.GetBookingByRef;
using Roomy.API.Repositories;

namespace Roomy.API.Endpoints.Bookings.CreateBooking;

public class CreateBookingEndpoint(IRoomRepository rooms, IBookingRepository bookings)
    : Endpoint<CreateBookingRequest, CreateBookingResponse>
{
    public override void Configure()
    {
        Post("/booking");
        AllowAnonymous();
        Summary(summary =>
        {
            summary.Summary = "Book a room for a date range.";
            summary.Description =
                "Books requested room for the whole range and issues a booking reference.";
            summary.Response<CreateBookingResponse>(201, "The booking.");
            summary.Response(404, "No hotel of that name, or no room of that number in it.");
            summary.Response(409, "The room cannot hold that many guests, or is already booked for those dates.");
        });
    }

    public override async Task HandleAsync(CreateBookingRequest request, CancellationToken ct)
    {
        var room = await rooms.GetAsync(request.HotelName, request.RoomNumber, ct);

        if (room is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (request.NumberOfGuests > room.RoomType.Capacity)
        {
            AddError(
                request => request.NumberOfGuests,
                $"Room {room.Number} is a {room.RoomType.Name} and holds {room.RoomType.Capacity} guest{(room.RoomType.Capacity == 1 ? string.Empty : "s")}.");

            await Send.ErrorsAsync(409, ct);
            return;
        }

        if (!await bookings.IsRoomAvailableAsync(request.HotelName, request.RoomNumber, request.StartDate, request.EndDate, ct))
        {
            AddError(
                request => request.StartDate,
                $"Room {room.Number} is already booked for part of that range.");

            await Send.ErrorsAsync(409, ct);
            return;
        }

        var booking = await bookings.CreateAsync(
            request.HotelName,
            request.RoomNumber,
            request.StartDate,
            request.EndDate,
            request.NumberOfGuests,
            request.LeadGuestName,
            ct);

        await Send.CreatedAtAsync<GetBookingByRefEndpoint>(
            new { refNumber = booking.RefNumber },
            new CreateBookingResponse
            {
                RefNumber = booking.RefNumber,
                HotelName = booking.HotelName,
                RoomNumber = booking.RoomNumber,
                RoomTypeName = booking.RoomType.Name,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                Nights = booking.Nights,
                NumberOfGuests = booking.NumberOfGuests,
                LeadGuestName = booking.LeadGuestName
            },
            cancellation: ct);
    }
}
