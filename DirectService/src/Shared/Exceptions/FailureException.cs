namespace Shared.Exceptions;

public class FailureException : Exception
{
    public Error Error { get; } = null!;
    
    public FailureException(Error error) : base(error.GetMessage())
    {
        Error = error;
    }

    FailureException() { }
    
    FailureException(string message) : base(message)
    {
    }

    FailureException(string message, Exception innerException) : base(message, innerException)
    {
    }
}