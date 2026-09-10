using Roomy.API.Common;
using Roomy.API.Data;
using Roomy.API.Data.Entities;

namespace Roomy.API.RepositoryTests;

public static class TestData
{
    public const int SingleRoomTypeId = 1;

    public const int DoubleRoomTypeId = 2;

    public const int DeluxeRoomTypeId = 3;

    public static DateOnly OnDay(int dayOfMonth) => new(2030, 6, dayOfMonth);

    public static Hotel SeedHotel(RoomyDbContext context, string name)
    {
        var hotel = new Hotel { Name = name };

        hotel.Rooms.Add(new Room { Number = 1, RoomTypeId = SingleRoomTypeId });
        hotel.Rooms.Add(new Room { Number = 2, RoomTypeId = DoubleRoomTypeId });
        hotel.Rooms.Add(new Room { Number = 3, RoomTypeId = DeluxeRoomTypeId });

        context.Hotels.Add(hotel);
        context.SaveChanges();

        return hotel;
    }

    public static Booking SeedBooking(
        RoomyDbContext context,
        Hotel hotel,
        int roomNumber,
        DateOnly startDate,
        DateOnly endDate,
        int numberOfGuests = 2,
        string leadGuestName = "Existing Guest")
    {
        var room = hotel.Rooms.Single(candidate => candidate.Number == roomNumber);

        var booking = new Booking
        {
            RefNumber = BookingReference.Generate(),
            HotelId = hotel.Id,
            RoomId = room.Id,
            StartDate = startDate,
            EndDate = endDate,
            NumberOfGuests = numberOfGuests,
            LeadGuestName = leadGuestName
        };

        context.Bookings.Add(booking);
        context.SaveChanges();

        return booking;
    }
}
