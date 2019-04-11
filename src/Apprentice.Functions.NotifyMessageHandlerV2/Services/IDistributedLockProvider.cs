using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Services
{
    public interface IDistributedLockProvider
    {
        Task<bool> AcquireLock(string Id, CancellationToken cancellationToken);

        Task ReleaseLock(string Id);

        Task Start();

        Task Stop();
    }
}
