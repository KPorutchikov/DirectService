namespace Shared.Exceptions;

public class ConflictException : Exception
{
    public Error Error { get; } = null!;
    
    public ConflictException(Error error) : base(error.GetMessage())
    {
        Error = error;
    }

    ConflictException() { }
    
    ConflictException(string message) : base(message)
    {
    }

    ConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }
}