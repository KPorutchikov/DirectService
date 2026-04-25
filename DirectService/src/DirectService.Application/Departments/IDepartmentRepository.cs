using CSharpFunctionalExtensions;
using DirectService.Domain.Departments;
using Shared;

namespace DirectService.Application.Departments;

public interface IDepartmentRepository
{
    public Task<Result<Guid, Error>> Add(Department department, CancellationToken cancellationToken = default);

    public Task<Result<Department, Error>> GetById(Guid departmentId, CancellationToken cancellationToken = default);

}