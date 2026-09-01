using FluentValidation;
using Domain.Shared.ValueObjects;

namespace Domain.Shared.Validations;

public class NameValidator : AbstractValidator<Name>
{
    public NameValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(20);
        RuleFor(x => x.LastName).MaximumLength(20);
    }
}