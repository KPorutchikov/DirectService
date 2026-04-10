using System.Text.Json.Serialization;

namespace Shared;

public record ErrorMessage(string Code, string Message, string? InvalidField = null);

public record Error
{
    public const string SEPARATOR = "||";
    public IReadOnlyList<ErrorMessage> Messages { get; } = [];

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ErrorType Type { get; }

    private Error(IEnumerable<ErrorMessage> messages, ErrorType type)
    {
        Messages = messages.ToArray();
        Type = type;
    }
    
    public string GetMessage() => string.Join("; ", Messages.Select(m => m.ToString()));

    public static Error NotFound(string code, string messages, string? invalidField = null) =>
        new([new ErrorMessage(code, messages, invalidField)], ErrorType.NOT_FOUND);

    public static Error Validation(string code, string messages, string? invalidField = null) =>
        new([new ErrorMessage(code, messages, invalidField)], ErrorType.VALIDATION);

    public static Error Conflict(string code, string messages, string? invalidField = null) =>
        new([new ErrorMessage(code, messages, invalidField)], ErrorType.CONFLICT);

    public static Error Failure(string code, string messages, string? invalidField = null) =>
        new([new ErrorMessage(code, messages, invalidField)], ErrorType.FAILURE);
    
    public static Error Authentication(string code, string messages, string? invalidField = null) =>
        new([new ErrorMessage(code, messages, invalidField)], ErrorType.AUTHENTICATION);
    
    public static Error Authorization(string code, string messages, string? invalidField = null) =>
        new([new ErrorMessage(code, messages, invalidField)], ErrorType.AUTHORIZATION);
    
    public static Error NotFound(params IEnumerable<ErrorMessage> messages) => 
        new(messages, ErrorType.NOT_FOUND);

    public static Error Validation(params IEnumerable<ErrorMessage> messages) => 
        new(messages, ErrorType.VALIDATION);

    public static Error Conflict(params IEnumerable<ErrorMessage> messages) => 
        new(messages, ErrorType.CONFLICT);

    public static Error Failure(params IEnumerable<ErrorMessage> messages) => 
        new(messages, ErrorType.FAILURE);
    
    public static Error Authentication(params IEnumerable<ErrorMessage> messages) => 
        new(messages, ErrorType.AUTHENTICATION);
    
    public static Error Authorization(params IEnumerable<ErrorMessage> messages) => 
        new(messages, ErrorType.AUTHORIZATION);
    
    public Errors ToErrors() => new([this]);
    
    public string Serialize()
    {
        return string.Join(SEPARATOR, GetMessage());
    }
}