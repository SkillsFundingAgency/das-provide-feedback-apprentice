namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IExecutableAsync
    {
        Task ExecuteAsync(CancellationToken token = new CancellationToken());
    }
}