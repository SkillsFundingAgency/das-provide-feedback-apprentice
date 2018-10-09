namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System.Threading.Tasks;

    public interface ISmsQueueProvider
    {
        void Send(string message, string queue);

        Task SendAsync(string message, string queue);
    }
}