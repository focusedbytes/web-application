using FluentValidation;
using FocusedBytes.Api.Application.DTOs;

namespace FocusedBytes.Api.Application.Validators;

/// <summary>
/// FluentValidation validator for UpdateAuthMethodRequest.
/// </summary>
public class UpdateAuthMethodRequestValidator : AbstractValidator<UpdateAuthMethodRequest>
{
    public UpdateAuthMethodRequestValidator()
    {
        // Identifier validation
        RuleFor(x => x.Identifier)
            .NotEmpty()
            .WithMessage("Identifier is required")
            .MaximumLength(255)
            .WithMessage("Identifier must not exceed 255 characters");

        // NewSecret validation (when provided)
        When(x => !string.IsNullOrWhiteSpace(x.NewSecret), () =>
        {
            RuleFor(x => x.NewSecret)
                .MinimumLength(6)
                .WithMessage("New secret must be at least 6 characters")
                .MaximumLength(100)
                .WithMessage("New secret must not exceed 100 characters");
        });
    }
}
