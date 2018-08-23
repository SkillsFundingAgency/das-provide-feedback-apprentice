namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces
{
    public interface ICommandHandler<in TCommand>
        where TCommand : ICommand
    {
        void Handle(TCommand command);
    }
}