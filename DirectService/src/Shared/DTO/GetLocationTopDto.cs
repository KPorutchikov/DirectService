namespace Shared.DTO;

public record GetLocationTopDto
{
    public Guid Id { get; init; }

    public string LocationName { get; init; } = null!;
    
    public string Address { get; init; } = string.Empty;
    
    public int DepartmentCount { get; init; }
    
}