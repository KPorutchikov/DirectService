using CSharpFunctionalExtensions;
using DirectService.Domain.Departments;
using Shared;

namespace DirectService.Domain.Positions;

public class Position
{
    private Position(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
    
    // EF Core
    private Position() { }
    
    private List<Department> _departments = [];
    
    public IReadOnlyList<Department> Departments => _departments;
    
    public Guid Id { get; private set; }
    
    public string Name { get; private set; }
    
    public string? Description { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime? UpdatedAt { get; private  set; }

    public void SetActive(bool active)
    {
        IsActive = active;
        
        UpdatedAt= DateTime.UtcNow;
    }
    
    public void SetDepartment(Department department)
    {
        _departments!.Add(department);
    }
    
    public Result<Guid, Error> DeletePositions(Department department)
    {
        if (_departments != null)
        {
            foreach (var currentDepartment in _departments)
            {
                if (currentDepartment.Id == department.Id)
                {
                    _departments.Remove(currentDepartment);

                    return Result.Success<Guid, Error>(currentDepartment.Id);
                }
            }
        }
        return Error.NotFound(null, $"Department with id: {department.Id} does not exist.", null);
    }
    
    public Result<Position, Error> Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 100) 
            return Error.Validation(null, "Name must be between 3-100 characters.", "Name");
        
        if (!string.IsNullOrWhiteSpace(description) && description.Length > 1000) 
            return Error.Validation(null, "Description must be less 1000 characters.", "Description");
        
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success<Position, Error>(this);
    }
    
    public Result<Position, Error> Create(Guid id, string name, string? description)
    {
        if (id == Guid.Empty) return Error.Validation(null, "ID cannot be null or empty.", "Id");
        
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 100) 
            return Error.Validation(null, "Name must be between 3-100 characters.", "Name");
        
        if (!string.IsNullOrWhiteSpace(description) && description.Length > 1000) 
            return Error.Validation(null, "Description must be less 1000 characters.", "Description");

        return new Position(id, name, description!);
    }
}