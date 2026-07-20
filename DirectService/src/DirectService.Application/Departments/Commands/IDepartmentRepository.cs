using CSharpFunctionalExtensions;
using DirectService.Domain.Departments;
using Shared;
using Shared.DTO;

namespace DirectService.Application.Departments.Commands;

public interface IDepartmentRepository
{

    public Task<Result<int, Error>> UpdateDepartmentPathTree(string rootPath, Guid? newParentId, CancellationToken cancellationToken);
    public Task<Result<List<DepartmentsLockDto>, Error>> SetLockDepartmentTree(Guid departmentId, Guid? newParentId, CancellationToken cancellationToken = default);
    public Task<Result<Guid, Error>> SetLockDepartmentLocationSql(Guid departmentId, CancellationToken cancellationToken = default);
    public Task<Result<Guid, Error>> Add(Department department, CancellationToken cancellationToken = default);

    public Task<Result<Department, Error>> GetById(Guid departmentId, CancellationToken cancellationToken = default);

    public Task<Result<Department, Error>> GetByIdWithLocations(Guid departmentId, CancellationToken cancellationToken = default);

    public Task<Result<int, Error>> AddLocationsSql(Guid departmentId, IEnumerable<DepartmentLocation> departmentLocations, CancellationToken cancellationToken = default);

    public Task<Result<int, Error>> DeleteLocationsSql(Guid departmentId, IEnumerable<Guid> locationsIds, CancellationToken cancellationToken = default);
}