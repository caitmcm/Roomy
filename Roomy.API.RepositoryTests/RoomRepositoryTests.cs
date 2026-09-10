using Roomy.API.Repositories;

namespace Roomy.API.RepositoryTests;

public class RoomRepositoryTests
{
    [Theory]
    [InlineData(14, 16, true, "turnover after: arrives the day the existing booking departs")]
    [InlineData(8, 10, true, "turnover before: departs the day the existing booking arrives")]
    [InlineData(20, 22, true, "clear gap after the existing booking")]
    [InlineData(3, 5, true, "clear gap before the existing booking")]
    [InlineData(10, 14, false, "exact match")]
    [InlineData(11, 13, false, "contained within the existing booking")]
    [InlineData(9, 15, false, "envelops the existing booking")]
    [InlineData(8, 11, false, "starts before, ends inside")]
    [InlineData(13, 16, false, "starts inside, ends after")]
    public async Task RoomWithAnExistingBookingIsOfferedOnlyWhenTheRangeDoesNotOverlap(
        int fromDay,
        int toDay,
        bool expectedAvailable,
        string because)
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        var hotel = TestData.SeedHotel(context, "The Kelvin Arms");
        TestData.SeedBooking(context, hotel, roomNumber: 2, TestData.OnDay(10), TestData.OnDay(14));

        var repository = new RoomRepository(context);

        var available = await repository.FindAvailableAsync(
            hotel.Name,
            TestData.OnDay(fromDay),
            TestData.OnDay(toDay),
            guests: 2,
            CancellationToken.None);

        var bookedRoomOffered = available.Any(room => room.Number == 2);

        Assert.True(expectedAvailable == bookedRoomOffered, $"Room 2 should be {(expectedAvailable ? "available" : "unavailable")} — {because}.");
    }

    [Theory]
    [InlineData(1, new[] { 1, 2, 3 })]
    [InlineData(2, new[] { 2, 3 })]
    [InlineData(3, new[] { 3 })]
    [InlineData(4, new int[0])]
    public async Task OnlyRoomsLargeEnoughForThePartyAreOffered(int guests, int[] expectedRoomNumbers)
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        var hotel = TestData.SeedHotel(context, "The Kelvin Arms");
        var repository = new RoomRepository(context);

        var available = await repository.FindAvailableAsync(
            hotel.Name,
            TestData.OnDay(10),
            TestData.OnDay(12),
            guests,
            CancellationToken.None);

        Assert.Equal(expectedRoomNumbers, available.Select(room => room.Number).ToArray());
    }

    [Fact]
    public async Task RoomsBelongingToAnotherHotelAreNeverOffered()
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        var kelvin = TestData.SeedHotel(context, "The Kelvin Arms");
        TestData.SeedHotel(context, "Riverside Lodge");

        var repository = new RoomRepository(context);

        var available = await repository.FindAvailableAsync(
            kelvin.Name,
            TestData.OnDay(10),
            TestData.OnDay(12),
            guests: 1,
            CancellationToken.None);

        Assert.NotEmpty(available);
        Assert.All(available, room => Assert.Equal("The Kelvin Arms", room.HotelName));
    }

    [Fact]
    public async Task TwoHotelsSharingARoomNumberAreKeptApart()
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        var kelvin = TestData.SeedHotel(context, "The Kelvin Arms");
        var riverside = TestData.SeedHotel(context, "Riverside Lodge");

        TestData.SeedBooking(context, kelvin, roomNumber: 3, TestData.OnDay(10), TestData.OnDay(14));

        var repository = new RoomRepository(context);

        var kelvinRooms = await repository.FindAvailableAsync(kelvin.Name, TestData.OnDay(10), TestData.OnDay(14), guests: 3, CancellationToken.None);
        var riversideRooms = await repository.FindAvailableAsync(riverside.Name, TestData.OnDay(10), TestData.OnDay(14), guests: 3, CancellationToken.None);

        Assert.Empty(kelvinRooms);
        Assert.Equal(3, Assert.Single(riversideRooms).Number);
    }

    [Fact]
    public async Task AHotelNameIsMatchedIgnoringCase()
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        TestData.SeedHotel(context, "The Kelvin Arms");
        var repository = new RoomRepository(context);

        var available = await repository.FindAvailableAsync(
            "the kelvin arms",
            TestData.OnDay(10),
            TestData.OnDay(12),
            guests: 1,
            CancellationToken.None);

        Assert.Equal(3, available.Count);
    }

    [Fact]
    public async Task APartialHotelNameMatchesNothing()
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        TestData.SeedHotel(context, "The Kelvin Arms");
        var repository = new RoomRepository(context);

        var available = await repository.FindAvailableAsync(
            "Kelvin",
            TestData.OnDay(10),
            TestData.OnDay(12),
            guests: 1,
            CancellationToken.None);

        Assert.Empty(available);
    }

    [Fact]
    public async Task AnAvailableRoomCarriesItsHotelRoomTypeAndCapacity()
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        var hotel = TestData.SeedHotel(context, "The Kelvin Arms");
        var repository = new RoomRepository(context);

        var available = await repository.FindAvailableAsync(
            hotel.Name,
            TestData.OnDay(10),
            TestData.OnDay(12),
            guests: 3,
            CancellationToken.None);

        var deluxe = Assert.Single(available);

        Assert.Equal("The Kelvin Arms", deluxe.HotelName);
        Assert.Equal("Deluxe", deluxe.RoomType.Name);
        Assert.Equal(3, deluxe.RoomType.Capacity);
    }

    [Theory]
    [InlineData("The Kelvin Arms", 2, true)]
    [InlineData("the kelvin arms", 2, true)]
    [InlineData("The Kelvin Arms", 9, false)]
    [InlineData("Riverside Lodge", 2, false)]
    public async Task GetResolvesARoomOnlyFromItsOwnHotelName(string hotelName, int roomNumber, bool expectedFound)
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        TestData.SeedHotel(context, "The Kelvin Arms");
        var repository = new RoomRepository(context);

        var room = await repository.GetAsync(hotelName, roomNumber, CancellationToken.None);

        Assert.Equal(expectedFound, room is not null);
    }
}
