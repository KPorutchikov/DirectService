namespace DirectService.Contracts.Departments;

public record GetDepartmentRequest(string? Name, string? SortByColumns, int? SortDir, int? Page, int? PageSize);