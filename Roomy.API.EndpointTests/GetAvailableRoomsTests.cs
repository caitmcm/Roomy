using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Roomy.API.Domain;
using Roomy.API.Endpoints.Rooms.GetAvailableRooms;
using Roomy.API.Repositories;

namespace Roomy.API.EndpointTests;

public class GetAvailableRoomsTests
{
    private const string HotelName = "The Kelvin Arms";

    private static readonly RoomType Double = new() { Name = "Double", Capacity = 2 };

    private static readonly RoomType Deluxe = new() { Name = "Deluxe", Capacity = 4 };

    private static Room DoubleRoom => new() { Number = 2, HotelName = HotelName, RoomType = Double };

    private static Room DeluxeRoom => new() { Number = 3, HotelName = HotelName, RoomType = Deluxe };

    private static GetAvailableRoomsRequest Request(int guests = 2) =>
        new()
        {
            HotelName = HotelName,
            From = TestDates.OnDay(10),
            To = TestDates.OnDay(12),
            Guests = guests
        };

    private static GetAvailableRoomsEndpoint Endpoint(IHotelRepository hotels, IRoomRepository rooms) =>
        Factory.Create<GetAvailableRoomsEndpoint>(context => context.AddTestServices(services => services.AddRouting()), hotels, rooms);

    [Fact]
    public async Task AnUnknownHotelIsNotFoundWithoutSearchingForRooms()
    {
        var hotels = Substitute.For<IHotelRepository>();
        var rooms = Substitute.For<IRoomRepository>();

        hotels.ExistsAsync(HotelName, Arg.Any<CancellationToken>()).Returns(false);

        var endpoint = Endpoint(hotels, rooms);
        await endpoint.HandleAsync(Request(), CancellationToken.None);

        Assert.Equal(404, endpoint.HttpContext.Response.StatusCode);
        await rooms.DidNotReceive().FindAvailableAsync(
            Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TheRouteAndQueryValuesReachTheRepositoriesUnaltered()
    {
        var hotels = Substitute.For<IHotelRepository>();
        var rooms = Substitute.For<IRoomRepository>();

        hotels.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        rooms.FindAvailableAsync(
            Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([DoubleRoom]);

        var endpoint = Endpoint(hotels, rooms);
        await endpoint.HandleAsync(Request(guests: 2), CancellationToken.None);

        await hotels.Received(1).ExistsAsync(HotelName, Arg.Any<CancellationToken>());
        await rooms.Received(1).FindAvailableAsync(HotelName, TestDates.OnDay(10), TestDates.OnDay(12), 2, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EveryAvailableRoomIsProjectedWithItsTypeAndCapacity()
    {
        var hotels = Substitute.For<IHotelRepository>();
        var rooms = Substitute.For<IRoomRepository>();

        hotels.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        rooms.FindAvailableAsync(
            Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([DoubleRoom, DeluxeRoom]);

        var endpoint = Endpoint(hotels, rooms);
        await endpoint.HandleAsync(Request(), CancellationToken.None);

        Assert.Equal(200, endpoint.HttpContext.Response.StatusCode);
        Assert.Collection(
            endpoint.Response.Rooms,
            first =>
            {
                Assert.Equal(2, first.RoomNumber);
                Assert.Equal("Double", first.RoomTypeName);
                Assert.Equal(2, first.Capacity);
            },
            second =>
            {
                Assert.Equal(3, second.RoomNumber);
                Assert.Equal("Deluxe", second.RoomTypeName);
                Assert.Equal(4, second.Capacity);
            });
    }

    [Fact]
    public async Task TheRepositoryOrderingIsPreserved()
    {
        var hotels = Substitute.For<IHotelRepository>();
        var rooms = Substitute.For<IRoomRepository>();

        hotels.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        rooms.FindAvailableAsync(
            Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([DeluxeRoom, DoubleRoom]);

        var endpoint = Endpoint(hotels, rooms);
        await endpoint.HandleAsync(Request(), CancellationToken.None);

        Assert.Equal([3, 2], endpoint.Response.Rooms.Select(room => room.RoomNumber));
    }

    [Fact]
    public async Task TheSearchTermsAreEchoedBackAlongsideTheDerivedNightCount()
    {
        var hotels = Substitute.For<IHotelRepository>();
        var rooms = Substitute.For<IRoomRepository>();

        hotels.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        rooms.FindAvailableAsync(
            Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([DeluxeRoom]);

        var endpoint = Endpoint(hotels, rooms);
        await endpoint.HandleAsync(
            new GetAvailableRoomsRequest { HotelName = HotelName, From = TestDates.OnDay(1), To = TestDates.OnDay(8), Guests = 3 },
            CancellationToken.None);

        Assert.Equal(HotelName, endpoint.Response.HotelName);
        Assert.Equal(TestDates.OnDay(1), endpoint.Response.From);
        Assert.Equal(TestDates.OnDay(8), endpoint.Response.To);
        Assert.Equal(7, endpoint.Response.Nights);
        Assert.Equal(3, endpoint.Response.Guests);
    }

    [Fact]
    public async Task AKnownHotelWithNothingFreeIsAnEmptyListRatherThanANotFound()
    {
        var hotels = Substitute.For<IHotelRepository>();
        var rooms = Substitute.For<IRoomRepository>();

        hotels.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        rooms.FindAvailableAsync(
            Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);

        var endpoint = Endpoint(hotels, rooms);
        await endpoint.HandleAsync(Request(), CancellationToken.None);

        Assert.Equal(200, endpoint.HttpContext.Response.StatusCode);
        Assert.Empty(endpoint.Response.Rooms);
    }
}
