namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;

    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Options;

    using Newtonsoft.Json;

    public class BotConnector : IBotConnector, IDisposable
    {
        private readonly DirectLine botClientSettings;

        private readonly IDirectLineClient client;

        public BotConnector(IDirectLineClient client, IOptions<DirectLine> botClientSettings)
        {
            this.client = client;
            this.botClientSettings = botClientSettings.Value;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.client?.Dispose();
        }

        public async Task<StartConversationResponse> StartConversationAsync()
        {
            try
            {
                var conversation = this.client.StartConversationAsync();
                conversation.Result.EnsureSuccessStatusCode();

                dynamic json = await conversation.Result.Content.ReadAsStringAsync();
                StartConversationResponse result = JsonConvert.DeserializeObject<StartConversationResponse>(json);

                if (result.ConversationId == null)
                {
                    throw new Exception($"Could not convert JSON object to {nameof(StartConversationResponse)}");
                }

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<PostToBotResponse> PostToBotAsync(string conversationId, BotMessage message)
        {
            try
            {
                var conversation = this.client.PostToConversationAsync(conversationId, message);
                conversation.Result.EnsureSuccessStatusCode();

                dynamic json = await conversation.Result.Content.ReadAsStringAsync();
                PostToBotResponse result = JsonConvert.DeserializeObject<PostToBotResponse>(json);

                if (result.Id == null)
                {
                    throw new Exception($"Could not convert JSON object to {nameof(PostToBotResponse)}");
                }

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public Task<IEnumerable<Activity>> GetMessages()
        {
            throw new NotImplementedException();
        }

        public Task EndConversation()
        {
            throw new NotImplementedException();
        }
    }
}