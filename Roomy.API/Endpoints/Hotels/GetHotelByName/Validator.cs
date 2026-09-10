using FastEndpoints;
using FluentValidation;

namespace Roomy.API.Endpoints.Hotels.GetHotelByName;

public class GetHotelByNameValidator : Validator<GetHotelByNameRequest>
{
    public GetHotelByNameValidator()
    {
        RuleFor(request => request.Name)
            .MaximumLength(200).WithMessage("A hotel name search term may not exceed 200 characters.");
    }
}
