using CSharpFunctionalExtensions;
using DirectService.Application.Departments;
using DirectService.Application.Departments.Commands.Create;
using DirectService.Application.Departments.Commands.Move;
using DirectService.Contracts.Departments;
using DirectService.Domain.Locations;
using DirectService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Shared;
using Xunit;


namespace DirectService.IntegrationTests.Departments;

public class MoveDepartmentTests : DirectBaseTests
{
    public MoveDepartmentTests(DirectTestWebFactory factory) : base(factory)
    {
    }
    
    [Fact]
    public async Task CreateDepartment_with_move_should_succeed()
    {
        // arrange
        Result<Guid, Errors> res;
        List<DepartmentDto> departmentsDtos = new List<DepartmentDto>();
        Guid? parentId = null;
        var locationId = CreateLocation("Локация-1", "Адресс-1", "Moscow").Result;
        var cancellationToken = CancellationToken.None;
        var departments = Enumerable.Range(0, 9)
            .Select(i => new { ID = i, Name = $"Dep_{i}", Identifier = $"ide{Convert.ToChar(65+i)}" })
            .ToList();
        
        
        // act
        foreach (var department in departments)
        {
            res = await ExecuteCreateDepartmentHandler((sut) =>
            {
                var command = new CreateDepartmentCommand(new CreateDepartmentRequest(department.Name, department.Identifier, 
                    (department.ID == 0 || department.ID % 3 == 0) ? null : parentId, [locationId]));
                return sut.Handle(command, cancellationToken);
            });
            departmentsDtos.Add(new DepartmentDto(res.Value, (department.ID == 0 || department.ID % 3 == 0) ? null : parentId));
            parentId = res.Value;
        }

        var resMoveToRoot = await ExecuteMoveDepartmentHandler((sut) =>
        {
            var command = new MoveDepartmentCommand(departmentsDtos[4].Id, null);
            return sut.Handle(command, cancellationToken);
        });
        
        var resMoveChildToChild = await ExecuteMoveDepartmentHandler((sut) =>
        {
            var command = new MoveDepartmentCommand(departmentsDtos[6].Id, departmentsDtos[2].Id);
            return sut.Handle(command, cancellationToken);
        });
        
            
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var resToRoot = await dbContext.Departments.FirstAsync(d => d.Id == resMoveToRoot.Value, cancellationToken);
            var resToChild= await dbContext.Departments.FirstAsync(d => d.Id == resMoveChildToChild.Value, cancellationToken);

            Assert.Null(resToRoot.ParentId);
            Assert.Equal(resToChild.ParentId, departmentsDtos[2].Id);
        });
    }
    
    private async Task<Guid> CreateLocation(string name, string address, string timeZone)
    {
        return await ExecuteInDb(async dbContext =>
            {
                var location = Location.Create( Guid.NewGuid(), 
                    LocationName.Create(name).Value, 
                    Address.Create(address).Value, 
                    Domain.Locations.TimeZone.Create(timeZone).Value).Value;
        
                dbContext.Add(location);
                await dbContext.SaveChangesAsync();
                return location.Id;
            }
        );
    }
    private async Task<T> ExecuteCreateDepartmentHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();

        return await action(sut);
    }
    private async Task<T> ExecuteMoveDepartmentHandler<T>(Func<MoveDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var sut = scope.ServiceProvider.GetRequiredService<MoveDepartmentHandler>();

        return await action(sut);
    }

    private record DepartmentDto(Guid Id, Guid? ParentId);
}