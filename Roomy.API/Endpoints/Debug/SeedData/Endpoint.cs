using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Roomy.API.Common;
using Roomy.API.Data;
using Roomy.API.Data.Entities;

namespace Roomy.API.Endpoints.Debug.SeedData;

public class SeedDataEndpoint(RoomyDbContext database) : EndpointWithoutRequest<SeedDataResponse>
{
    private static readonly (int Number, int RoomTypeId)[] RoomPlan =
    [
        (1, 1), (2, 1), (3, 2), (4, 2), (5, 3), (6, 3)
    ];

    public override void Configure()
    {
        Post("/debug");
        AllowAnonymous();
        Summary(summary =>
        {
            summary.Summary = "Seed the database with test data.";
            summary.Description = "Deletes all data and inserts test data into the SQL Lite database.";
            summary.Response<SeedDataResponse>(200, "Counts of the records inserted.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await database.Bookings.ExecuteDeleteAsync(ct);
        await database.Rooms.ExecuteDeleteAsync(ct);
        await database.Hotels.ExecuteDeleteAsync(ct);

        var hotels = new[]
        {
            new Hotel { Name = "The Kelvin Arms" },
            new Hotel { Name = "Riverside Lodge" }
        };

        foreach (var hotel in hotels)
        {
            foreach (var (number, roomTypeId) in RoomPlan)
            {
                hotel.Rooms.Add(new Room { Number = number, RoomTypeId = roomTypeId });
            }
        }

        database.Hotels.AddRange(hotels);
        await database.SaveChangesAsync(ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var kelvin = hotels[0];
        var riverside = hotels[1];

        var turnoverRoom = kelvin.Rooms.Single(room => room.Number == 3);

        var bookings = new[]
        {
            NewBooking(turnoverRoom, "Ada Lovelace", today.AddDays(10), today.AddDays(12), 2),
            NewBooking(turnoverRoom, "Grace Hopper", today.AddDays(12), today.AddDays(14), 2),
            NewBooking(kelvin.Rooms.Single(room => room.Number == 5), "Alan Turing", today.AddDays(3), today.AddDays(6), 3),
            NewBooking(riverside.Rooms.Single(room => room.Number == 1), "Mary Somerville", today.AddDays(1), today.AddDays(2), 1)
        };

        database.Bookings.AddRange(bookings);
        await database.SaveChangesAsync(ct);

        await Send.OkAsync(
            new SeedDataResponse
            {
                Hotels = hotels.Length,
                Rooms = hotels.Sum(hotel => hotel.Rooms.Count),
                Bookings = bookings.Length
            },
            ct);
    }

    private static Booking NewBooking(Room room, string leadGuestName, DateOnly startDate, DateOnly endDate, int numberOfGuests) =>
        new()
        {
            RefNumber = BookingReference.Generate(),
            HotelId = room.HotelId,
            RoomId = room.Id,
            StartDate = startDate,
            EndDate = endDate,
            NumberOfGuests = numberOfGuests,
            LeadGuestName = leadGuestName
        };
}
