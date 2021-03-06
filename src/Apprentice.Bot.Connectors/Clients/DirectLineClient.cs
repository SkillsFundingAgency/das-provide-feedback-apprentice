﻿namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Clients
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Domain.Dto;
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

        public async Task<HttpResponseMessage> StartConversationAsync()
        {
            var newToken = await GenerateToken();

            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
            return await this.client.GetAsync("/v3/directline/conversations");
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

        private async Task<string> GenerateToken()
        {
            var content = new StringContent("");
            content.Headers.ContentType.MediaType = "application/json";

            var result = await this.client.PostAsync($"/v3/directline/tokens/generate", content);

            await EnsureSuccessStatusCode(result);

            var json = await result.Content.ReadAsStringAsync();
            var tokenResult = JsonConvert.DeserializeObject<GenerateTokenResponse>(json);

            if (tokenResult?.Token == null)
            {
                throw new Exception($"Could not convert JSON object to {nameof(GenerateTokenResponse)}");
            }

            return tokenResult.Token;
        }

        private async Task EnsureSuccessStatusCode(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Response code {response.StatusCode} returned from call to 'directline' with error {error}");
            }
        }        
    }
}
