using System.Text.Json;

namespace Shared.Exceptions;

public class BadRequestException : Exception
{
    public Error Error { get; } = null!;
    
    public BadRequestException(Error error) : base(error.GetMessage())
    {
        Error = error;
    }

    BadRequestException() { }
    
    BadRequestException(string message) : base(message)
    {
    }

    BadRequestException(string message, Exception innerException) : base(message, innerException)
    {
    }
}