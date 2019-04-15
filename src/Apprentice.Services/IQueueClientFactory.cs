using Microsoft.Azure.ServiceBus;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services
{
    public interface IQueueClientFactory
    {
        IQueueClient CreateIncomingSmsQueueClient();
        IQueueClient CreateOutgoingSmsQueueClient();
    }
}
