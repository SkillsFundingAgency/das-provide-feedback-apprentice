namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces
{
    public interface IExecutable
    {
        void Execute();
    }

    public interface IExecutable<in TInputs>
    {
        void Execute(TInputs input);
    }

    public interface IExecutable<in TInputs, out TOutput>
    {
        TOutput Execute(TInputs input);
    }
}