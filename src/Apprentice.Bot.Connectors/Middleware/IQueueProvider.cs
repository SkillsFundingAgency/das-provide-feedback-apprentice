namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System.Threading.Tasks;

    public interface IQueueProvider
    {
        void Send(object message);

        Task SendAsync(object message);
    }
}