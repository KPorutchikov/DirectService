using CSharpFunctionalExtensions;
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
    
    public Guid Id { get; private set; }
    
    public LocationName Name { get; private set; }
    
    public Address Address { get; private set; }
    
    public TimeZone TimeZone { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime? UpdatedAt { get; private set; }

    public void SetActive(bool active)
    {
        IsActive = active;
        
        UpdatedAt= DateTime.UtcNow;
    }
    
    public Result<Location, Error> Update(LocationName name, Address address, TimeZone timeZone)
    {
        Name = name;
        Address = address;
        TimeZone = timeZone;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success<Location, Error>(this);
    }

    public static Result<Location, Error> Create(Guid id, LocationName name, Address address, TimeZone timeZone)
    {
        if (id == Guid.Empty) return GeneralErrors.ValueIsInvalid("Id");
        
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
        if (string.IsNullOrWhiteSpace(name) || name.Length < LengthConstants.Length3 || name.Length > LengthConstants.Length120) 
            return GeneralErrors.ValueIsInvalid("LocationName","Name must be between 3 and 120 characters");
        
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
            return GeneralErrors.ValueIsInvalid("Address"); 
        
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