namespace Shared.Exceptions;

public class ValidationException : Exception
{
    public Error Error { get; } = null!;
    
    public ValidationException(Error error) : base(error.GetMessage())
    {
        Error = error;
    }

    ValidationException() { }
    
    ValidationException(string message) : base(message)
    {
    }

    ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}