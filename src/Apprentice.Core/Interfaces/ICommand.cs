namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces
{
    public interface ICommand : IExecutable
    {
    }

    public interface ICommand<in T> : ICommand, IExecutable<T>
    {
    }
}