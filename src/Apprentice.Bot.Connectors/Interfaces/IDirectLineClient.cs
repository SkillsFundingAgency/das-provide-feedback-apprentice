namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Interfaces
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using ESFA.DAS.ProvideFeedback.Apprentice.Domain.Dto;

    public interface IDirectLineClient : IDisposable
    {
        Task<HttpResponseMessage> StartConversationAsync();

        Task<HttpResponseMessage> PostToConversationAsync(string conversationId, BotMessage content);

    }
}