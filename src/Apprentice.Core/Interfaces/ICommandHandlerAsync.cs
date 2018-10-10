namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICommandHandlerAsync<in TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        Task HandleAsync(TCommand command, CancellationToken cancellationToken = new CancellationToken());
    }
}