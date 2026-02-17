using CSharpFunctionalExtensions;
using DirectService.Domain.Departments;
using Shared;

namespace DirectService.Domain.Locations;

public class Location
{
    private Location(Guid id, LocationName name, Address address, TimeZone timeZone)
    {
        Id = id;
        Name = name;
        Address = address;
        TimeZone = timeZone;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    // EF Core
    private Location() { }
    
    private List<Department> _departments = [];
    
    public IReadOnlyList<Department> Departments => _departments;
    
    public Guid Id { get; private set; }
    
    public LocationName Name { get; private set; }
    
    public Address Address { get; private set; }
    
    public TimeZone TimeZone { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime? UpdatedAt { get; private set; }

    public void SetDepartments(Department department)
    {
        _departments.Add(department);
        
        UpdatedAt= DateTime.UtcNow;
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        
        UpdatedAt= DateTime.UtcNow;
    }
    
    public Result<Guid, Error> DeleteDepartments(Department department)
    {
        if (_departments != null)
            foreach (var currentDepartment in _departments)
            {
                if (currentDepartment.Id == department.Id)
                {
                    _departments.Remove(currentDepartment);
                    
                    UpdatedAt= DateTime.UtcNow;

                    return Result.Success<Guid, Error>(currentDepartment.Id);
                }
            }
        return Error.NotFound(null, $"Department with id: {department.Id} does not exist.", null);
    }

    public Result<Location, Error> Update(LocationName name, Address address, TimeZone timeZone)
    {
        Name = name;
        Address = address;
        TimeZone = timeZone;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success<Location, Error>(this);
    }

    public Result<Location, Error> Create(Guid id, LocationName name, Address address, TimeZone timeZone)
    {
        if (id == Guid.Empty) return Error.Validation(null, "ID cannot be null or empty.", "Id");
        
        return new Location(id, name, address, timeZone);
    }
}

public record LocationName
{
    public string Value { get; }

    private LocationName(string value)
    {
        Value = value;
    }

    public static Result<LocationName, Error> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 120) 
            return Error.Validation(null, "Name must be between 3-120 characters.", "Name");
        
        return new LocationName(name);
    }
}

public record Address
{
    public string Value { get; }

    private Address(string value)
    {
        Value = value;
    }

    public static Result<Address, Error> Create(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) 
            return Error.Validation(null, "Address cannot be null or empty.", "Address");
        
        return new Address(address);
    }
}

public record TimeZone
{
    public string Value { get; }

    private TimeZone(string value)
    {
        Value = value;
    }

    public static Result<TimeZone, Error> Create(string timeZone)
    {
        return new TimeZone(timeZone);
    }
}