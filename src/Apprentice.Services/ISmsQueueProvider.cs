namespace ESFA.DAS.ProvideFeedback.Apprentice.Services
{
    using System.Threading.Tasks;

    public interface ISmsQueueProvider
    {
        void Send(string message, string queue);

        Task SendAsync(string message, string queue);
    }
}