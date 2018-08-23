namespace ESFA.DAS.ProvideFeedback.Apprentice.Services
{
    using System.Threading.Tasks;

    public interface IBotConversationService
    {
        Task SendToBotAsync(string sourceNumber, string message, IBotChannelData channelData = null);
    }
}