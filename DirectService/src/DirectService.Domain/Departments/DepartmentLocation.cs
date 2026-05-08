using CSharpFunctionalExtensions;
using Shared;

namespace DirectService.Domain.Departments;

public class DepartmentLocation
{
    private DepartmentLocation(Guid id, Department department, Guid locationId )
    {
        Id = id;
        Department = department;
        LocationId = locationId;
        CreatedAt = DateTime.UtcNow;
    }

    // EF Core
    private DepartmentLocation() { }
    public Guid Id { get; private set;}
    public Department Department { get; private set;}
    public Guid LocationId { get; private set;}
    public DateTime CreatedAt { get; }

    public static Result<DepartmentLocation, Error> Create(Department department, Guid locationId)
    {
        return new DepartmentLocation(Guid.NewGuid(), department, locationId);
    }
}