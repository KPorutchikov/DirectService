namespace DirectService.Contracts.Locations;

public record GetLocationFilterRequest(string? Name, int? minDepartmentCount, string? SortByColumns, int? SortDir, int? Page, int? PageSize);