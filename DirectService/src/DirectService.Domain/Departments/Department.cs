using CSharpFunctionalExtensions;
using DirectService.Domain.Locations;
using DirectService.Domain.Positions;
using Shared;

namespace DirectService.Domain.Departments;

public class Department
{
    private Department(
        Guid id, Guid parentId, DepartmentName departmentName, Identifier identifier, Path path, short depth, 
        IEnumerable<Guid> locations, IEnumerable<Guid> positions)
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
            .Select(l => new DepartmentLocation(Guid.NewGuid(),this,l))
            .ToList();
        _locations = newLocations;
        
        var newPositions = positions
            .Select(l => new DepartmentPosition(Guid.NewGuid(),this,l))
            .ToList();
        _positions = newPositions;
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
    
    public Path Path { get; private set; } = null!;
    
    public short Depth { get; private set; }
    
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
    
    public void SetLocations(IEnumerable<Guid> locations)
    {
        var newLocations = locations.Select(l => new DepartmentLocation(Guid.NewGuid(),this, l)).ToList();
        foreach (var location in newLocations)
        {
            _locations.Add(location);
        }
    }

    public Result<Guid, Error> DeleteLocations(Location location)
    {
        if (_locations != null)
            foreach (var currentLocation in _locations)
            {
                if (currentLocation.LocationId == location.Id)
                {
                    _locations.Remove(currentLocation);

                    return Result.Success<Guid, Error>(currentLocation.LocationId);
                }
            }
        
        return Error.NotFound(null, $"Location with id: {location.Id} does not exist.", null);
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
        Guid id, Guid parentId, DepartmentName departmentName, Identifier identifier, Path path, short depth, 
        IEnumerable<Guid> locations, IEnumerable<Guid> positions)
    {
        if (id == Guid.Empty) return Error.Validation(null, "ID cannot be null or empty.", "Id");
        
        return new Department( id, parentId, departmentName, identifier, path, depth, locations, positions);
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
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 150) 
            return Error.Validation(null, "Name must be between 3-150 characters.", "Name");
        
        return new DepartmentName(name);  
    }
}

public record Path
{
    public string Value { get; }

    private Path(string value)
    {
        Value = value;
    }

    public static Result<Path, Error> Create(string path)
    {
        return new Path(path);
    }
}

public record Identifier
{
    public const int MIN_LENGTH_IDENTIFIER = 3;
    public const int MAX_LENGTH_IDENTIFIER = 150;

    public string Value { get; }

    private Identifier(string value)
    {
        Value = value;
    }

    public static Result<Identifier, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < MIN_LENGTH_IDENTIFIER || value.Length > MAX_LENGTH_IDENTIFIER) 
            return Error.Validation(null, $"Identifier must be between {MIN_LENGTH_IDENTIFIER}-{MAX_LENGTH_IDENTIFIER} characters.", "Identifier");

        if (!value.All(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
            return Error.Validation(null, "Identifier must consist of latin characters only.", "Identifier");
        return new Identifier(value);
    }
}