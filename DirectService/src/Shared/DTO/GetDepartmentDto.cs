namespace Shared.DTO;

public record GetDepartmentDto
{
    public Guid Id { get; init; }
    
    public string DepartmentName { get; init; } = string.Empty;
    
    public string Identifier { get; init; } = string.Empty;
    
    public bool IsActive { get; init; }
    
    public DateTime CreatedAt { get; init; }
    
    public DateTime? UpdatedAt { get; init; }
}