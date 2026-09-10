using FastEndpoints;
using FluentValidation;
using Roomy.API.Common;

namespace Roomy.API.Endpoints.Rooms.GetAvailableRooms;

public class GetAvailableRoomsValidator : Validator<GetAvailableRoomsRequest>
{
    public GetAvailableRoomsValidator(TimeProvider timeProvider)
    {
        RuleFor(request => request.HotelName)
            .NotEmpty().WithMessage("A hotel name is required.")
            .MaximumLength(200).WithMessage("A hotel name may not exceed 200 characters.");

        RuleFor(request => request.From)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime)).WithMessage("The arrival date may not be in the past.");

        RuleFor(request => request.To)
            .GreaterThan(request => request.From).WithMessage("The departure date must be later than the arrival date.");

        RuleFor(request => request)
            .Must(request => request.To.DayNumber - request.From.DayNumber <= StayRules.MaximumNights)
            .WithName("To")
            .WithMessage($"A stay may not exceed {StayRules.MaximumNights} nights.");

        RuleFor(request => request.Guests)
            .GreaterThanOrEqualTo(1).WithMessage("A stay must be for at least one guest.");
    }
}
