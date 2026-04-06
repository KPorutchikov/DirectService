using System.Text.Json;

namespace Shared.Exceptions;

public class NotFoundException : Exception
{
    public Error Error { get; } = null!;
    
    public NotFoundException(Error error) : base(error.GetMessage())
    {
        Error = error;
    }

    NotFoundException() { }
    
    NotFoundException(string message) : base(message)
    {
    }

    NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}