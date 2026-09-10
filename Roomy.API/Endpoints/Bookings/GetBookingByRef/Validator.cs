using FastEndpoints;
using FluentValidation;

namespace Roomy.API.Endpoints.Bookings.GetBookingByRef;

public class GetBookingByRefValidator : Validator<GetBookingByRefRequest>
{
    public GetBookingByRefValidator()
    {
        RuleFor(request => request.RefNumber)
            .NotEmpty().WithMessage("A booking reference is required.");
    }
}
