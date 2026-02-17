namespace DirectService.Domain.Departments;

public class DepartmentPosition
{
    public DepartmentPosition(Guid id, Department department, Guid positionId )
    {
        Id = id;
        Department = department;
        PositionId = positionId;
        CreatedAt = DateTime.UtcNow;
    }
    
    public Guid Id { get; }
    public Department Department { get; set; }
    public Guid PositionId { get; set; }
    public DateTime CreatedAt { get; }
}