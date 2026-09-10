using Roomy.API.Repositories;

namespace Roomy.API.RepositoryTests;

public class BookingRepositoryTests
{
    [Theory]
    [InlineData(14, 16, true)]
    [InlineData(8, 10, true)]
    [InlineData(10, 14, false)]
    [InlineData(11, 13, false)]
    [InlineData(9, 15, false)]
    [InlineData(8, 11, false)]
    [InlineData(13, 16, false)]
    public async Task IsRoomAvailableAgreesWithTheAvailabilitySearch(int fromDay, int toDay, bool expectedAvailable)
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        var hotel = TestData.SeedHotel(context, "The Kelvin Arms");
        TestData.SeedBooking(context, hotel, roomNumber: 2, TestData.OnDay(10), TestData.OnDay(14));

        var bookings = new BookingRepository(context);
        var rooms = new RoomRepository(context);

        var reportedAvailable = await bookings.IsRoomAvailableAsync(
            hotel.Name,
            roomNumber: 2,
            TestData.OnDay(fromDay),
            TestData.OnDay(toDay),
            CancellationToken.None);

        var offeredBySearch = await rooms.FindAvailableAsync(
            hotel.Name,
            TestData.OnDay(fromDay),
            TestData.OnDay(toDay),
            guests: 2,
            CancellationToken.None);

        Assert.Equal(expectedAvailable, reportedAvailable);
        Assert.Equal(reportedAvailable, offeredBySearch.Any(candidate => candidate.Number == 2));
    }

    [Fact]
    public async Task ABookingOnOneRoomDoesNotAffectAnother()
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        var hotel = TestData.SeedHotel(context, "The Kelvin Arms");
        TestData.SeedBooking(context, hotel, roomNumber: 2, TestData.OnDay(10), TestData.OnDay(14));

        var repository = new BookingRepository(context);

        Assert.True(await repository.IsRoomAvailableAsync(
            hotel.Name,
            roomNumber: 3,
            TestData.OnDay(10),
            TestData.OnDay(14),
            CancellationToken.None));
    }

    [Fact]
    public async Task ABookingInOneHotelDoesNotAffectTheSameRoomNumberInAnother()
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        var kelvin = TestData.SeedHotel(context, "The Kelvin Arms");
        var riverside = TestData.SeedHotel(context, "Riverside Lodge");

        TestData.SeedBooking(context, kelvin, roomNumber: 2, TestData.OnDay(10), TestData.OnDay(14));

        var repository = new BookingRepository(context);

        Assert.False(await repository.IsRoomAvailableAsync(kelvin.Name, 2, TestData.OnDay(10), TestData.OnDay(14), CancellationToken.None));
        Assert.True(await repository.IsRoomAvailableAsync(riverside.Name, 2, TestData.OnDay(10), TestData.OnDay(14), CancellationToken.None));
    }

    [Fact]
    public async Task CreatePersistsTheBookingAndIssuesAReference()
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        var hotel = TestData.SeedHotel(context, "The Kelvin Arms");
        var repository = new BookingRepository(context);

        var created = await repository.CreateAsync(
            hotel.Name,
            roomNumber: 2,
            TestData.OnDay(10),
            TestData.OnDay(14),
            guests: 2,
            leadGuestName: "Ada Lovelace",
            CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(created.RefNumber));
        Assert.Equal("The Kelvin Arms", created.HotelName);
        Assert.Equal(2, created.RoomNumber);
        Assert.Equal("Double", created.RoomType.Name);
        Assert.Equal(4, created.Nights);
        Assert.Equal("Ada Lovelace", created.LeadGuestName);

        Assert.False(await repository.IsRoomAvailableAsync(
            hotel.Name,
            roomNumber: 2,
            TestData.OnDay(10),
            TestData.OnDay(14),
            CancellationToken.None));
    }

    [Fact]
    public async Task CreateBooksTheRoomInTheNamedHotelWhenTwoShareARoomNumber()
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        TestData.SeedHotel(context, "The Kelvin Arms");
        var riverside = TestData.SeedHotel(context, "Riverside Lodge");

        var repository = new BookingRepository(context);

        var created = await repository.CreateAsync(
            riverside.Name,
            roomNumber: 2,
            TestData.OnDay(10),
            TestData.OnDay(12),
            guests: 2,
            leadGuestName: "Ada Lovelace",
            CancellationToken.None);

        Assert.Equal("Riverside Lodge", created.HotelName);
        Assert.True(await repository.IsRoomAvailableAsync("The Kelvin Arms", 2, TestData.OnDay(10), TestData.OnDay(12), CancellationToken.None));
    }

    [Theory]
    [InlineData("No Such Hotel", 2)]
    [InlineData("The Kelvin Arms", 9)]
    public async Task CreateRefusesARoomThatDoesNotExist(string hotelName, int roomNumber)
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        TestData.SeedHotel(context, "The Kelvin Arms");
        var repository = new BookingRepository(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.CreateAsync(
            hotelName,
            roomNumber,
            TestData.OnDay(10),
            TestData.OnDay(12),
            guests: 1,
            leadGuestName: "Ada Lovelace",
            CancellationToken.None));

        Assert.Empty(context.Bookings);
    }

    [Fact]
    public async Task SuccessiveBookingsGetDistinctReferences()
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        var hotel = TestData.SeedHotel(context, "The Kelvin Arms");
        var repository = new BookingRepository(context);

        var first = await repository.CreateAsync(hotel.Name, 2, TestData.OnDay(10), TestData.OnDay(12), 2, "Ada Lovelace", CancellationToken.None);
        var second = await repository.CreateAsync(hotel.Name, 2, TestData.OnDay(12), TestData.OnDay(14), 2, "Grace Hopper", CancellationToken.None);

        Assert.NotEqual(first.RefNumber, second.RefNumber);
    }

    [Fact]
    public async Task GetByReferenceReturnsTheBookingWithItsHotelAndRoom()
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        var hotel = TestData.SeedHotel(context, "The Kelvin Arms");
        var seeded = TestData.SeedBooking(context, hotel, roomNumber: 3, TestData.OnDay(10), TestData.OnDay(13), numberOfGuests: 3, leadGuestName: "Alan Turing");

        var repository = new BookingRepository(context);
        var found = await repository.GetByReferenceAsync(seeded.RefNumber, CancellationToken.None);

        Assert.NotNull(found);
        Assert.Equal(seeded.RefNumber, found.RefNumber);
        Assert.Equal("The Kelvin Arms", found.HotelName);
        Assert.Equal(3, found.RoomNumber);
        Assert.Equal("Deluxe", found.RoomType.Name);
        Assert.Equal(3, found.Nights);
        Assert.Equal("Alan Turing", found.LeadGuestName);
    }

    [Fact]
    public async Task GetByReferenceReturnsNullForAnUnknownReference()
    {
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        TestData.SeedHotel(context, "The Kelvin Arms");
        var repository = new BookingRepository(context);

        Assert.Null(await repository.GetByReferenceAsync("BK-NOSUCH", CancellationToken.None));
    }
}
