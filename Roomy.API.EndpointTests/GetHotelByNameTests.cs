using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Roomy.API.Domain;
using Roomy.API.Endpoints.Hotels.GetHotelByName;
using Roomy.API.Repositories;

namespace Roomy.API.EndpointTests;

public class GetHotelByNameTests
{
    private static Hotel Kelvin => new() { Name = "The Kelvin Arms", RoomCount = 3 };

    private static Hotel Clyde => new() { Name = "The Clyde Rooms", RoomCount = 12 };

    private static GetHotelByNameEndpoint Endpoint(IHotelRepository hotels) =>
        Factory.Create<GetHotelByNameEndpoint>(context => context.AddTestServices(services => services.AddRouting()), hotels);

    [Fact]
    public async Task ASearchTermIsPassedToTheRepositoryUnaltered()
    {
        var hotels = Substitute.For<IHotelRepository>();

        hotels.FindByNameAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns([Kelvin]);

        var endpoint = Endpoint(hotels);
        await endpoint.HandleAsync(new GetHotelByNameRequest { Name = "kelvin" }, CancellationToken.None);

        await hotels.Received(1).FindByNameAsync("kelvin", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AnAbsentNameIsPassedThroughAsNullSoTheRepositoryListsEveryHotel()
    {
        var hotels = Substitute.For<IHotelRepository>();

        hotels.FindByNameAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns([Kelvin, Clyde]);

        var endpoint = Endpoint(hotels);
        await endpoint.HandleAsync(new GetHotelByNameRequest(), CancellationToken.None);

        await hotels.Received(1).FindByNameAsync(null, Arg.Any<CancellationToken>());
        Assert.Equal(200, endpoint.HttpContext.Response.StatusCode);
        Assert.Equal(2, endpoint.Response.Hotels.Count);
    }

    [Fact]
    public async Task EveryMatchIsProjectedWithItsNameAndRoomCount()
    {
        var hotels = Substitute.For<IHotelRepository>();

        hotels.FindByNameAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns([Kelvin, Clyde]);

        var endpoint = Endpoint(hotels);
        await endpoint.HandleAsync(new GetHotelByNameRequest(), CancellationToken.None);

        Assert.Collection(
            endpoint.Response.Hotels,
            first =>
            {
                Assert.Equal("The Kelvin Arms", first.Name);
                Assert.Equal(3, first.RoomCount);
            },
            second =>
            {
                Assert.Equal("The Clyde Rooms", second.Name);
                Assert.Equal(12, second.RoomCount);
            });
    }

    [Fact]
    public async Task TheRepositoryOrderingIsPreserved()
    {
        var hotels = Substitute.For<IHotelRepository>();

        hotels.FindByNameAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns([Clyde, Kelvin]);

        var endpoint = Endpoint(hotels);
        await endpoint.HandleAsync(new GetHotelByNameRequest(), CancellationToken.None);

        Assert.Equal(["The Clyde Rooms", "The Kelvin Arms"], endpoint.Response.Hotels.Select(hotel => hotel.Name));
    }

    [Fact]
    public async Task NoMatchesIsAnEmptyListRatherThanANotFound()
    {
        var hotels = Substitute.For<IHotelRepository>();

        hotels.FindByNameAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns([]);

        var endpoint = Endpoint(hotels);
        await endpoint.HandleAsync(new GetHotelByNameRequest { Name = "no such hotel" }, CancellationToken.None);

        Assert.Equal(200, endpoint.HttpContext.Response.StatusCode);
        Assert.Empty(endpoint.Response.Hotels);
    }
}
