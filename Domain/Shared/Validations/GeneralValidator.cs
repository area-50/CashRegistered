using FluentValidation;
using Domain.Shared.Exceptions;

namespace Domain.Shared.Validations;

public abstract class GeneralValidator
{
    protected static void Validate<T>(
        T? model,
        AbstractValidator<T?> validator,
        Func<List<string>,
            BaseException> exceptionFactory,
        List<string>? caseModelNullMessage = null) where T : class
    {
        if (model is null)
        {
            var errorMessage = caseModelNullMessage ?? new List<string> { "Erro não especificado." };
            throw exceptionFactory(errorMessage);
        }
        var result = validator.Validate(model);

        if (result!.IsValid) return;
        var errors = result.Errors.Select(x => x.ErrorMessage).ToList();
        throw exceptionFactory(errors);
    }
}