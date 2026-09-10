using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Roomy.API.Domain;
using Roomy.API.Endpoints.Bookings.GetBookingByRef;
using Roomy.API.Repositories;

namespace Roomy.API.EndpointTests;

public class GetBookingByRefTests
{
    private const string RefNumber = "BK-4F2A9C";

    private static readonly RoomType Deluxe = new() { Name = "Deluxe", Capacity = 4 };

    private static Booking Existing => new()
    {
        RefNumber = RefNumber,
        HotelName = "The Kelvin Arms",
        RoomNumber = 3,
        RoomType = Deluxe,
        StartDate = TestDates.OnDay(10),
        EndDate = TestDates.OnDay(14),
        NumberOfGuests = 3,
        LeadGuestName = "Ada Lovelace"
    };

    private static GetBookingByRefEndpoint Endpoint(IBookingRepository bookings) =>
        Factory.Create<GetBookingByRefEndpoint>(context => context.AddTestServices(services => services.AddRouting()), bookings);

    [Fact]
    public async Task AnUnknownReferenceIsNotFound()
    {
        var bookings = Substitute.For<IBookingRepository>();

        bookings.GetByReferenceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Booking?)null);

        var endpoint = Endpoint(bookings);
        await endpoint.HandleAsync(new GetBookingByRefRequest { RefNumber = "BK-NOPE00" }, CancellationToken.None);

        Assert.Equal(404, endpoint.HttpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TheRouteReferenceReachesTheRepositoryUnaltered()
    {
        var bookings = Substitute.For<IBookingRepository>();

        bookings.GetByReferenceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Existing);

        var endpoint = Endpoint(bookings);
        await endpoint.HandleAsync(new GetBookingByRefRequest { RefNumber = RefNumber }, CancellationToken.None);

        await bookings.Received(1).GetByReferenceAsync(RefNumber, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AKnownReferenceIsReturnedInFull()
    {
        var bookings = Substitute.For<IBookingRepository>();

        bookings.GetByReferenceAsync(RefNumber, Arg.Any<CancellationToken>()).Returns(Existing);

        var endpoint = Endpoint(bookings);
        await endpoint.HandleAsync(new GetBookingByRefRequest { RefNumber = RefNumber }, CancellationToken.None);

        Assert.Equal(200, endpoint.HttpContext.Response.StatusCode);
        Assert.Equal(RefNumber, endpoint.Response.RefNumber);
        Assert.Equal("The Kelvin Arms", endpoint.Response.HotelName);
        Assert.Equal(3, endpoint.Response.RoomNumber);
        Assert.Equal("Deluxe", endpoint.Response.RoomTypeName);
        Assert.Equal(TestDates.OnDay(10), endpoint.Response.StartDate);
        Assert.Equal(TestDates.OnDay(14), endpoint.Response.EndDate);
        Assert.Equal(3, endpoint.Response.NumberOfGuests);
        Assert.Equal("Ada Lovelace", endpoint.Response.LeadGuestName);
    }

    [Fact]
    public async Task TheNightCountIsDerivedFromTheStayRatherThanStored()
    {
        var bookings = Substitute.For<IBookingRepository>();

        bookings.GetByReferenceAsync(RefNumber, Arg.Any<CancellationToken>())
            .Returns(Existing with { StartDate = TestDates.OnDay(1), EndDate = TestDates.OnDay(8) });

        var endpoint = Endpoint(bookings);
        await endpoint.HandleAsync(new GetBookingByRefRequest { RefNumber = RefNumber }, CancellationToken.None);

        Assert.Equal(7, endpoint.Response.Nights);
    }
}
