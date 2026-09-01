namespace Domain.Shared.Exceptions;

public class DomainException : BaseException
{
    public DomainException(string message = "Um ou mais erros de validação de domínio ocorreram.") : base(message)
    {
    }
    
    public DomainException(
        List<string> errors,
        string message = "Um ou mais erros de validação de domínio ocorreram.") : base(message, errors)
    {
    }
}