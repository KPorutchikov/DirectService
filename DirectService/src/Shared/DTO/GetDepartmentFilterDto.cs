namespace Shared.DTO;

public record GetDepartmentFilterDto
{
    public Guid Id { get; init; }
    
    public string DepartmentName { get; init; } = string.Empty;
    
    public string Path { get; init; } = string.Empty;
    
    public DateTime CreatedAt { get; init; }
}