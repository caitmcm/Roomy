using FastEndpoints;
using FluentValidation;
using Roomy.API.Common;

namespace Roomy.API.Endpoints.Bookings.CreateBooking;

public class CreateBookingValidator : Validator<CreateBookingRequest>
{
    public CreateBookingValidator(TimeProvider timeProvider)
    {
        RuleFor(request => request.HotelName)
            .NotEmpty().WithMessage("A hotel name is required.")
            .MaximumLength(200).WithMessage("A hotel name may not exceed 200 characters.");

        RuleFor(request => request.RoomNumber)
            .GreaterThanOrEqualTo(1).WithMessage("A room number is required.");

        RuleFor(request => request.StartDate)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime)).WithMessage("The arrival date may not be in the past.");

        RuleFor(request => request.EndDate)
            .GreaterThan(request => request.StartDate).WithMessage("The departure date must be later than the arrival date.");

        RuleFor(request => request)
            .Must(request => request.EndDate.DayNumber - request.StartDate.DayNumber <= StayRules.MaximumNights)
            .WithName("EndDate")
            .WithMessage($"A stay may not exceed {StayRules.MaximumNights} nights.");

        RuleFor(request => request.NumberOfGuests)
            .GreaterThanOrEqualTo(1).WithMessage("A booking must be for at least one guest.");

        RuleFor(request => request.LeadGuestName)
            .NotEmpty().WithMessage("A lead guest name is required.")
            .MaximumLength(200).WithMessage("A lead guest name may not exceed 200 characters.");
    }
}
