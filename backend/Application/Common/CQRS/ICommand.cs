namespace FocusedBytes.Api.Application.Common.CQRS;

public interface ICommand
{
}

public interface ICommand<out TResult> : ICommand
{
}
