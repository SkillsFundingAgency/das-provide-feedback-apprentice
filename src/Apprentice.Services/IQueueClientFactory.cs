using Microsoft.Azure.ServiceBus;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services
{
    public interface IQueueClientFactory
    {
        IQueueClient Create<T>() where T : Message;
    }
}
