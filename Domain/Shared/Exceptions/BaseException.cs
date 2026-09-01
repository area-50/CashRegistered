namespace Domain.Shared.Exceptions;

public abstract class BaseException : Exception
{
    public IReadOnlyCollection<string> Errors { get; }

    public BaseException(string message) : base(message) 
    { 
        Errors = new List<string>();
    }

    public BaseException(string message, List<string> errors) : base(message)
    {
        Errors = errors.AsReadOnly();
    }
}
