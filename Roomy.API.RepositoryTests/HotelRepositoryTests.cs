using Microsoft.EntityFrameworkCore;
using Roomy.API.Data.Entities;
using Roomy.API.Repositories;

namespace Roomy.API.RepositoryTests;

public class HotelRepositoryTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AnEmptySearchListsEveryHotel(string? name)
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        TestData.SeedHotel(context, "The Kelvin Arms");
        TestData.SeedHotel(context, "Riverside Lodge");

        var repository = new HotelRepository(context);
        var matches = await repository.FindByNameAsync(name, CancellationToken.None);

        Assert.Equal(["Riverside Lodge", "The Kelvin Arms"], matches.Select(hotel => hotel.Name).ToArray());
    }

    [Theory]
    [InlineData("kelvin", "The Kelvin Arms")]
    [InlineData("KELVIN", "The Kelvin Arms")]
    [InlineData("Riverside Lodge", "Riverside Lodge")]
    [InlineData("lodge", "Riverside Lodge")]
    public async Task APartialSearchMatchesIgnoringCase(string term, string expectedName)
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        TestData.SeedHotel(context, "The Kelvin Arms");
        TestData.SeedHotel(context, "Riverside Lodge");

        var repository = new HotelRepository(context);
        var matches = await repository.FindByNameAsync(term, CancellationToken.None);

        Assert.Equal(expectedName, Assert.Single(matches).Name);
    }

    [Fact]
    public async Task ASearchMatchingNothingReturnsAnEmptyList()
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        TestData.SeedHotel(context, "The Kelvin Arms");

        var repository = new HotelRepository(context);

        Assert.Empty(await repository.FindByNameAsync("Grand Budapest", CancellationToken.None));
    }

    [Fact]
    public async Task AMatchedHotelCarriesItsRoomCount()
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        TestData.SeedHotel(context, "The Kelvin Arms");

        var repository = new HotelRepository(context);
        var match = Assert.Single(await repository.FindByNameAsync("Kelvin", CancellationToken.None));

        Assert.Equal(3, match.RoomCount);
    }

    [Theory]
    [InlineData("The Kelvin Arms", true)]
    [InlineData("the kelvin arms", true)]
    [InlineData("Kelvin", false)]
    [InlineData("No Such Hotel", false)]
    public async Task ExistsRequiresTheWholeNameAndIgnoresCase(string hotelName, bool expected)
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        TestData.SeedHotel(context, "The Kelvin Arms");

        var repository = new HotelRepository(context);

        Assert.Equal(expected, await repository.ExistsAsync(hotelName, CancellationToken.None));
    }

    [Fact]
    public async Task TwoHotelsCannotShareANameEvenInDifferentCase()
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        TestData.SeedHotel(context, "The Kelvin Arms");

        context.Hotels.Add(new Hotel { Name = "the kelvin arms" });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(CancellationToken.None));
    }
}
