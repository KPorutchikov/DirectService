using CSharpFunctionalExtensions;
using DirectService.Domain.Locations;
using DirectService.Domain.Positions;
using Shared;

namespace DirectService.Domain.Departments;

public class Department
{
    private Department(
        Guid id, Guid? parentId, DepartmentName departmentName, Identifier identifier, Path path, short depth, 
        IEnumerable<Guid> locations)
    {
        Id = id;
        DepartmentName = departmentName;
        Identifier = identifier;
        ParentId = parentId;
        Path = path;
        Depth = depth;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        
        var newLocations = locations
            .Select(l => DepartmentLocation.Create(this, l).Value)
            .ToList();
        _locations = newLocations;
        
        // var newPositions = positions
        //     .Select(l => new DepartmentPosition(Guid.NewGuid(),this,l))
        //     .ToList();
        // _positions = newPositions;
    }
    
    // EF Core
    private Department() { }
    
    private List<DepartmentLocation> _locations = [];
    
    public IReadOnlyList<DepartmentLocation> Locations => _locations;

    private List<DepartmentPosition> _positions = [];
    
    public IReadOnlyList<DepartmentPosition> Positions => _positions;
    
    public Guid Id { get; private set; }
    
    public DepartmentName DepartmentName { get; private set; } = null!;

    public Identifier Identifier { get; private set; } = null!;
    
    public Guid? ParentId { get; private set; }
    
    public Department? Parent { get; private set; }
    
    public Path Path { get; private set; } = null!;
    //public string Path { get; private set; } = null!;
    
    public short? Depth { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime? UpdatedAt { get; private set; }


    public void SetActive(bool active)
    {
        IsActive = active;
        
        UpdatedAt= DateTime.UtcNow;
    }

    public void SetParent(Guid parentId)
    {
        ParentId = parentId;
        
        UpdatedAt= DateTime.UtcNow;
    }
    
    public void SetPositions(IEnumerable<Guid> positions)
    {
        var newPositions = positions.Select(l => new DepartmentPosition(Guid.NewGuid(),this, l)).ToList();
        foreach (var position in newPositions)
        {
            _positions.Add(position);
        }
    }
    
    public Result<Guid, Error> DeletePositions(Position position)
    {
        if (_positions != null)
            foreach (var currentPosition in _positions)
            {
                if (currentPosition.PositionId == position.Id)
                {
                    _positions.Remove(currentPosition);

                    return Result.Success<Guid, Error>(currentPosition.PositionId);
                }
            }
        return Error.NotFound(null, $"Position with id: {position.Id} does not exist.", null);
    }
    
    public void SetLocations(IEnumerable<DepartmentLocation> locations)
    {
        //var newLocations = locations.Select(l => DepartmentLocation.Create(this, l).Value).ToList();
        foreach (var location in locations)
        {
            _locations.Add(location);
        }
    }

    public Result<Guid, Error> DeleteLocations(Guid locationId)
    {
        if (_locations != null)
            foreach (var currentLocation in _locations)
            {
                if (currentLocation.LocationId == locationId)
                {
                    _locations.Remove(currentLocation);

                    return Result.Success<Guid, Error>(currentLocation.LocationId);
                }
            }
        
        return Error.NotFound(null, $"Location with id: {locationId} does not exist.", null);
    }

    public Result<Department, Error> Update(DepartmentName departmentName, Identifier identifier, Guid parentId, Path path, short depth, bool isActive)
    {
        DepartmentName = departmentName;
        Identifier = identifier;
        ParentId = parentId;
        Path = path;
        Depth = depth;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success<Department, Error>(this);
    }
    
    public static Result<Department, Error> Create( 
        Guid id, Guid? parentId, DepartmentName departmentName, Identifier identifier, Path path, short depth, 
        IEnumerable<Guid> locations)
    {
        if (id == Guid.Empty) return GeneralErrors.ValueIsInvalid("DepartmentId");
        
        return new Department( id, parentId, departmentName, identifier, path, depth, locations);
    }
}

public record DepartmentName
{
    public string Value { get; }

    private DepartmentName(string value)
    {
        Value = value;
    }

    public static Result<DepartmentName, Error> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < LengthConstants.Length3 || name.Length > LengthConstants.Length150) 
            return GeneralErrors.ValueIsInvalid("DepartmentName","Name must be between 3 and 150 characters");;
        
        return new DepartmentName(name);  
    }
}

public record Path
{
    private const char Separator = '.';
    public string Value { get; }

    private Path(string value)
    {
        Value = value;
    }

    public static Path Create(string value)
    {
        return new Path(value);
    }

    public Path CreateChild(string childIdentifier)
    {
        return new Path(Value + Separator + childIdentifier);
    }
}

public record Identifier
{
    public string Value { get; }

    private Identifier(string value)
    {
        Value = value;
    }

    public static Result<Identifier, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < LengthConstants.Length3 || value.Length > LengthConstants.Length150) 
            return GeneralErrors.ValueIsInvalid("Identifier","Identifier must be between 3 and 150 characters");

        if (!value.All(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
            return GeneralErrors.ValueIsInvalid("Identifier", "Identifier must contain only latinize letters");
        
        return new Identifier(value);
    }
}