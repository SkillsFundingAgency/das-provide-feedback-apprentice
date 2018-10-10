﻿namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Clients
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Interfaces;

    using Newtonsoft.Json;

    /// <summary>
    /// TODO: Polly
    /// </summary>
    public class DirectLineClient : IDirectLineClient
    {
        private readonly HttpClient client;

        public DirectLineClient(Uri botAddress, string authToken)
        {
            this.client = new HttpClient { BaseAddress = botAddress };

            this.client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authToken);
        }

        public Task<HttpResponseMessage> StartConversationAsync()
        {
            return this.client.GetAsync("/v3/directline/conversations");
        }

        public Task<HttpResponseMessage> PostToConversationAsync(string conversationId, BotMessage message)
        {
            HttpContent content = new StringContent(JsonConvert.SerializeObject(message));
            content.Headers.ContentType.MediaType = "application/json";

            return this.client.PostAsync($"/v3/directline/conversations/{conversationId}/activities", content);
        }

        public void Dispose()
        {
            this.client.CancelPendingRequests();
            this.client?.Dispose();
        }
    }
}
