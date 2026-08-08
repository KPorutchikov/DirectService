namespace Shared.DTO;

public record GetDepartmentsTreeDto
{
    public Guid Id { get; init; }
    
    public string DepartmentName { get; init; } = string.Empty;
    
    public string Identifier { get; init; } = string.Empty;
    
    public string Path { get; init; } = string.Empty;
    
    public int Depth { get; init; }
    
    public bool HasChildren { get; init; }
}