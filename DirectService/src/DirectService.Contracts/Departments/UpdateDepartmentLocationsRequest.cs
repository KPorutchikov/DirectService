namespace DirectService.Contracts.Departments;

public record UpdateDepartmentLocationsRequest(Guid[] OldLocationIds, Guid[] NewLocationIds);