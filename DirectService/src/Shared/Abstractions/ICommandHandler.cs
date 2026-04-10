using CSharpFunctionalExtensions;

namespace Shared.Abstractions;

public interface ICommandHandler<TResponse, in TCommand> where TCommand : ICommand
{
    public  Task<Result<TResponse, Errors>> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    public  Task<Result<Errors>> Handle(TCommand command, CancellationToken cancellationToken);
}