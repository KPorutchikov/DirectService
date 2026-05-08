using CSharpFunctionalExtensions;
using Shared;

namespace DirectService.Application.Database;

public interface ITransactionScope : IDisposable
{
    UnitResult<Error> Commit();
    
    UnitResult<Error> Rollback();
}