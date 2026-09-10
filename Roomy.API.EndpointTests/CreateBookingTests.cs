using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Roomy.API.Domain;
using Roomy.API.Endpoints.Bookings.CreateBooking;
using Roomy.API.Repositories;

namespace Roomy.API.EndpointTests;

public class CreateBookingTests
{
    private const string HotelName = "The Kelvin Arms";

    private const int RoomNumber = 2;

    private static readonly RoomType Double = new() { Name = "Double", Capacity = 2 };

    private static Room DoubleRoom => new() { Number = RoomNumber, HotelName = HotelName, RoomType = Double };

    private static CreateBookingRequest Request(int numberOfGuests = 2) =>
        new()
        {
            HotelName = HotelName,
            RoomNumber = RoomNumber,
            StartDate = TestDates.OnDay(10),
            EndDate = TestDates.OnDay(12),
            NumberOfGuests = numberOfGuests,
            LeadGuestName = "Ada Lovelace"
        };

    private static CreateBookingEndpoint Endpoint(IRoomRepository rooms, IBookingRepository bookings) =>
        Factory.Create<CreateBookingEndpoint>(context => context.AddTestServices(services => services.AddRouting()), rooms, bookings);

    [Fact]
    public async Task AnUnknownRoomIsNotFound()
    {
        var rooms = Substitute.For<IRoomRepository>();
        var bookings = Substitute.For<IBookingRepository>();

        rooms.GetAsync(HotelName, RoomNumber, Arg.Any<CancellationToken>()).Returns((Room?)null);

        var endpoint = Endpoint(rooms, bookings);
        await endpoint.HandleAsync(Request(), CancellationToken.None);

        Assert.Equal(404, endpoint.HttpContext.Response.StatusCode);
        await bookings.DidNotReceive().CreateAsync(
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task APartyLargerThanTheRoomIsRejectedWithoutConsultingAvailability()
    {
        var rooms = Substitute.For<IRoomRepository>();
        var bookings = Substitute.For<IBookingRepository>();

        rooms.GetAsync(HotelName, RoomNumber, Arg.Any<CancellationToken>()).Returns(DoubleRoom);

        var endpoint = Endpoint(rooms, bookings);
        await endpoint.HandleAsync(Request(numberOfGuests: 3), CancellationToken.None);

        Assert.Equal(409, endpoint.HttpContext.Response.StatusCode);
        await bookings.DidNotReceive().IsRoomAvailableAsync(
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ARoomAlreadyTakenForTheRangeIsRejected()
    {
        var rooms = Substitute.For<IRoomRepository>();
        var bookings = Substitute.For<IBookingRepository>();

        rooms.GetAsync(HotelName, RoomNumber, Arg.Any<CancellationToken>()).Returns(DoubleRoom);
        bookings.IsRoomAvailableAsync(HotelName, RoomNumber, TestDates.OnDay(10), TestDates.OnDay(12), Arg.Any<CancellationToken>()).Returns(false);

        var endpoint = Endpoint(rooms, bookings);
        await endpoint.HandleAsync(Request(), CancellationToken.None);

        Assert.Equal(409, endpoint.HttpContext.Response.StatusCode);
        await bookings.DidNotReceive().CreateAsync(
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TheRouteValuesReachTheRepositoriesUnaltered()
    {
        var rooms = Substitute.For<IRoomRepository>();
        var bookings = Substitute.For<IBookingRepository>();

        rooms.GetAsync(HotelName, RoomNumber, Arg.Any<CancellationToken>()).Returns(DoubleRoom);
        bookings.IsRoomAvailableAsync(
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>()).Returns(true);
        bookings.CreateAsync(
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Created());

        var endpoint = Endpoint(rooms, bookings);
        await endpoint.HandleAsync(Request(), CancellationToken.None);

        await rooms.Received(1).GetAsync(HotelName, RoomNumber, Arg.Any<CancellationToken>());
        await bookings.Received(1).IsRoomAvailableAsync(HotelName, RoomNumber, TestDates.OnDay(10), TestDates.OnDay(12), Arg.Any<CancellationToken>());
        await bookings.Received(1).CreateAsync(HotelName, RoomNumber, TestDates.OnDay(10), TestDates.OnDay(12), 2, "Ada Lovelace", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ASuccessfulBookingIsReportedAsCreatedWithItsReference()
    {
        var rooms = Substitute.For<IRoomRepository>();
        var bookings = Substitute.For<IBookingRepository>();

        rooms.GetAsync(HotelName, RoomNumber, Arg.Any<CancellationToken>()).Returns(DoubleRoom);
        bookings.IsRoomAvailableAsync(
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>()).Returns(true);
        bookings.CreateAsync(
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Created());

        var endpoint = Endpoint(rooms, bookings);
        await endpoint.HandleAsync(Request(), CancellationToken.None);

        Assert.Equal(201, endpoint.HttpContext.Response.StatusCode);
        Assert.Equal("BK-TEST01", endpoint.Response.RefNumber);
        Assert.Equal(HotelName, endpoint.Response.HotelName);
        Assert.Equal(RoomNumber, endpoint.Response.RoomNumber);
        Assert.Equal("Double", endpoint.Response.RoomTypeName);
        Assert.Equal(2, endpoint.Response.Nights);
    }

    private static Booking Created() =>
        new()
        {
            RefNumber = "BK-TEST01",
            HotelName = HotelName,
            RoomNumber = RoomNumber,
            RoomType = Double,
            StartDate = TestDates.OnDay(10),
            EndDate = TestDates.OnDay(12),
            NumberOfGuests = 2,
            LeadGuestName = "Ada Lovelace"
        };
}
