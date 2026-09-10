using Microsoft.Extensions.Time.Testing;
using Roomy.API.Common;
using Roomy.API.Endpoints.Bookings.CreateBooking;
using Roomy.API.Endpoints.Bookings.GetBookingByRef;
using Roomy.API.Endpoints.Hotels.GetHotelByName;
using Roomy.API.Endpoints.Rooms.GetAvailableRooms;

namespace Roomy.API.EndpointTests;

public class ValidatorTests
{
    private static readonly DateOnly Today = new(2026, 1, 15);

    private static readonly FakeTimeProvider Clock = new(new DateTimeOffset(Today, TimeOnly.MinValue, TimeSpan.Zero));

    private static readonly DateOnly Soon = Today.AddDays(30);

    private static CreateBookingValidator BookingValidator() => new(Clock);

    private static GetAvailableRoomsValidator SearchValidator() => new(Clock);

    private static CreateBookingRequest Booking(
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        int numberOfGuests = 2,
        string leadGuestName = "Ada Lovelace",
        string hotelName = "The Kelvin Arms",
        int roomNumber = 2) =>
        new()
        {
            HotelName = hotelName,
            RoomNumber = roomNumber,
            StartDate = startDate ?? Soon,
            EndDate = endDate ?? Soon.AddDays(2),
            NumberOfGuests = numberOfGuests,
            LeadGuestName = leadGuestName
        };

    private static GetAvailableRoomsRequest Search(
        DateOnly? from = null,
        DateOnly? to = null,
        int guests = 2,
        string hotelName = "The Kelvin Arms") =>
        new()
        {
            HotelName = hotelName,
            From = from ?? Soon,
            To = to ?? Soon.AddDays(2),
            Guests = guests
        };

    [Fact]
    public void AWellFormedBookingRequestPasses() =>
        Assert.True(BookingValidator().Validate(Booking()).IsValid);

    [Fact]
    public void ABookingEndingOnItsStartDateIsRejected() =>
        Assert.False(BookingValidator().Validate(Booking(Soon, Soon)).IsValid);

    [Fact]
    public void ABookingEndingBeforeItStartsIsRejected() =>
        Assert.False(BookingValidator().Validate(Booking(Soon, Soon.AddDays(-1))).IsValid);

    [Fact]
    public void ABookingArrivingInThePastIsRejected() =>
        Assert.False(BookingValidator().Validate(Booking(Today.AddDays(-1), Soon)).IsValid);

    [Fact]
    public void ABookingArrivingTodayIsAccepted() =>
        Assert.True(BookingValidator().Validate(Booking(Today, Today.AddDays(1))).IsValid);

    [Fact]
    public void ABookingForNoGuestsIsRejected() =>
        Assert.False(BookingValidator().Validate(Booking(numberOfGuests: 0)).IsValid);

    [Fact]
    public void ABookingWithNoLeadGuestNameIsRejected() =>
        Assert.False(BookingValidator().Validate(Booking(leadGuestName: string.Empty)).IsValid);

    [Fact]
    public void ABookingWithNoHotelNameIsRejected() =>
        Assert.False(BookingValidator().Validate(Booking(hotelName: string.Empty)).IsValid);

    [Fact]
    public void ABookingWithNoRoomNumberIsRejected() =>
        Assert.False(BookingValidator().Validate(Booking(roomNumber: 0)).IsValid);

    [Fact]
    public void ABookingLongerThanTheMaximumStayIsRejected() =>
        Assert.False(BookingValidator().Validate(Booking(Soon, Soon.AddDays(StayRules.MaximumNights + 1))).IsValid);

    [Fact]
    public void ABookingOfExactlyTheMaximumStayIsAccepted() =>
        Assert.True(BookingValidator().Validate(Booking(Soon, Soon.AddDays(StayRules.MaximumNights))).IsValid);

    [Fact]
    public void AWellFormedRoomSearchPasses() =>
        Assert.True(SearchValidator().Validate(Search()).IsValid);

    [Fact]
    public void ARoomSearchEndingOnItsStartDateIsRejected() =>
        Assert.False(SearchValidator().Validate(Search(Soon, Soon)).IsValid);

    [Fact]
    public void ARoomSearchStartingInThePastIsRejected() =>
        Assert.False(SearchValidator().Validate(Search(Today.AddDays(-1), Soon)).IsValid);

    [Fact]
    public void ARoomSearchForNoGuestsIsRejected() =>
        Assert.False(SearchValidator().Validate(Search(guests: 0)).IsValid);

    [Fact]
    public void ARoomSearchWithNoHotelNameIsRejected() =>
        Assert.False(SearchValidator().Validate(Search(hotelName: string.Empty)).IsValid);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Kelvin")]
    public void AHotelNameSearchIsValidWithOrWithoutATerm(string? name) =>
        Assert.True(new GetHotelByNameValidator().Validate(new GetHotelByNameRequest { Name = name }).IsValid);

    [Fact]
    public void AHotelNameSearchTermLongerThanTheColumnIsRejected() =>
        Assert.False(new GetHotelByNameValidator().Validate(new GetHotelByNameRequest { Name = new string('x', 201) }).IsValid);

    [Fact]
    public void AnEmptyBookingReferenceIsRejected() =>
        Assert.False(new GetBookingByRefValidator().Validate(new GetBookingByRefRequest { RefNumber = string.Empty }).IsValid);
}
