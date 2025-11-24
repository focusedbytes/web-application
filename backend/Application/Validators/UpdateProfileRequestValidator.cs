using FluentValidation;
using FocusedBytes.Api.Application.DTOs;

namespace FocusedBytes.Api.Application.Validators;

/// <summary>
/// FluentValidation validator for UpdateProfileRequest.
/// </summary>
public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        // DisplayName validation (optional)
        When(x => !string.IsNullOrWhiteSpace(x.DisplayName), () =>
        {
            RuleFor(x => x.DisplayName)
                .MaximumLength(100)
                .WithMessage("Display name must not exceed 100 characters");
        });
    }
}
