using Shared.Exceptions;
using Shared.Validations;

namespace Shared.ValueObjects;

public class Name : ValueObject
{
    public Name(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        
        // Validação via FluentValidation desativada temporariamente.
        // O Flunt agora trata essas regras na entidade Person.
        // Validate(this, new NameValidator()!, errors => new DomainException(errors));
    }
    
    protected Name() { }
    
    public string FirstName { get; set; }

    public string LastName { get; set; }
}