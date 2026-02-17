namespace DirectService.Domain.Departments;

public class DepartmentLocation
{
    public DepartmentLocation(Guid id, Department department, Guid locationId )
    {
        Id = id;
        Department = department;
        LocationId = locationId;
        CreatedAt = DateTime.UtcNow;
    }
    
    public Guid Id { get; }
    public Department Department { get; set; }
    public Guid LocationId { get; set; }
    public DateTime CreatedAt { get; }
}