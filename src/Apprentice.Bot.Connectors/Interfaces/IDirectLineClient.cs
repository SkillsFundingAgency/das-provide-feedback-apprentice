namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IDirectLineClient : IDisposable
    {
        Task<HttpResponseMessage> StartConversationAsync();

        Task<HttpResponseMessage> PostToConversationAsync(string conversationId, BotMessage content);

    }
}