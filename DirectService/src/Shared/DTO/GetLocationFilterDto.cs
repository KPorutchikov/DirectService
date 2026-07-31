namespace Shared.DTO;

public record GetLocationFilterDto
{
    public Guid Id { get; init; }
    
    public string LocationName { get; init; } = string.Empty;
    
    public string Address { get; init; } = string.Empty;
    
    public DateTime CreatedAt { get; init; }

    public int? DepartmentCount { get; init; }
}