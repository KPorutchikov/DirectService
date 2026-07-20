namespace Shared.DTO;

public record GetLocationDto
{
    public Guid Id { get; init; }

    public string LocationName { get; init; } = null!;
    
    public string Address { get; init; } = string.Empty;
    
    public string TimeZone { get; init; } = string.Empty;
    
    public bool IsActive { get; init; }
    
    public DateTime CreatedAt { get; init; }
    
    public DateTime? UpdatedAt { get; init; }
    
}