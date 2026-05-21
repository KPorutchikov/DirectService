namespace Shared.DTO;

public class DepartmentsLockDto
{
    public Guid Id { get; init; }
    
    public Guid? ParentId { get; init; } = Guid.Empty;

    public string Identifier { get; init; } = string.Empty;
    
    public string Path { get; init; } = string.Empty;
}