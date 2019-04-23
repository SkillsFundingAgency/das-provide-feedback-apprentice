namespace ESFA.DAS.ProvideFeedback.Apprentice.Services
{
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;

    public interface ISmsQueueProvider
    {
        void Send(string message, string queue);

        Task SendAsync(string conversationId, Message message, string queue);
    }
}