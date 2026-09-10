using FastEndpoints;
using Roomy.API.Repositories;

namespace Roomy.API.Endpoints.Bookings.GetBookingByRef;

public class GetBookingByRefEndpoint(IBookingRepository bookings)
    : Endpoint<GetBookingByRefRequest, GetBookingByRefResponse>
{
    public override void Configure()
    {
        Get("/booking/{refNumber}");
        AllowAnonymous();
        Summary(summary =>
        {
            summary.Summary = "Retrieve a booking by its reference.";
            summary.Description = "Retrieve a booking by its reference.";
            summary.RequestParam(request => request.RefNumber, "The booking reference, for example BK-4F2A9C.");
            summary.Response<GetBookingByRefResponse>(200, "The booking.");
            summary.Response(404, "No booking exists with that reference.");
        });
    }

    public override async Task HandleAsync(GetBookingByRefRequest request, CancellationToken ct)
    {
        var booking = await bookings.GetByReferenceAsync(request.RefNumber, ct);

        if (booking is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(
            new GetBookingByRefResponse
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
            ct);
    }
}
