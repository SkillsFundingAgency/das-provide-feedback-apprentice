namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    using System;
    using System.Dynamic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;
    using ESFA.DAS.ProvideFeedback.Apprentice.Data;
    using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Dto;
    using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Services;
    using Microsoft.Azure.Documents.SystemFunctions;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// A function that delivers a message to the bot, using the DirectLine API and a Azure Storage Queue Trigger
    /// </summary>
    public static class DeliverMessageToBot
    {
        private static readonly Lazy<SettingsProvider> LazyConfigProvider = new Lazy<SettingsProvider>(Configure);

        private static readonly Lazy<HttpClient> LazyDirectLineClient = new Lazy<HttpClient>(InitializeHttpClient);

        //private static readonly Lazy<CosmosDbRepository> LazyDocClient = new Lazy<CosmosDbRepository>(InitializeDocumentClient);

        private static ExecutionContext currentContext;

        public static SettingsProvider Configuration => LazyConfigProvider.Value;

        private static HttpClient DirectLineClient => LazyDirectLineClient.Value;

        private static CosmosDbRepository DocumentClient => InitializeDocumentClient();

        /// <summary>
        /// Queue based trigger. Delivers incoming SMS messages from a queue to our bot using the DirectLine connector
        /// </summary>
        /// <param name="queueMessage">The incoming SMS payload.</param>
        /// <param name="log">the <see cref="ILogger"/></param>
        /// <param name="context">the function <see cref="ExecutionContext"/></param>
        /// <returns> the <see cref="Task"/> </returns>
        [FunctionName("DeliverMessageToBot")]
        [Singleton("IncomingSmsQueue", Mode = SingletonMode.Listener)]
        public static async Task Run(
        [ServiceBusTrigger("sms-incoming-messages", Connection = "ServiceBusConnection")]
        string queueMessage,
        ILogger log,
        ExecutionContext context)
        {
            currentContext = context;
            dynamic incomingSms = JsonConvert.DeserializeObject<dynamic>(queueMessage);

            try
            {
                log.LogInformation($"Response received from {incomingSms?.source_number}, sending to bot...");

                string userId = incomingSms?.source_number; // TODO: [security] hash me please!
                BotConversation conversation = await GetConversationByUserId(userId);

                if (conversation == null)
                {
                    await StartNewConversation(incomingSms, log);
                }
                else
                {
                    await PostToConversation(incomingSms, conversation, log);
                }
            }
            catch (MessageLockLostException e)
            {
                log.LogError($"DeliverMessageToBot MessageLockLostException [{context.FunctionName}|{context.InvocationId}]", e, e.Message);
            }
            catch (Exception e)
            {
                log.LogError($"DeliverMessageToBot ERROR", e, e.Message);
                DirectLineClient.CancelPendingRequests();
                throw new BotConnectorException("Something went wrong when relaying the message to the bot framework", e);
            }
        }

        private static async Task<BotConversation> GetConversationByUserId(string userId)
        {
            // TODO: extract this and inject an instance of IBotConversationProvider
            await DocumentClient.GetDocumentCollectionAsync();
            BotConversation conversation = await DocumentClient.GetItemAsync<BotConversation>(c => c.UserId == userId);
            return conversation;
        }

        private static SettingsProvider Configure()
        {
            if (currentContext == null)
            {
                throw new BotConnectorException("Could not initialize the settings provider, ExecutionContext is null");
            }

            return new SettingsProvider(currentContext);
        }

        private static CosmosDbRepository InitializeDocumentClient()
        {
            string endpoint = Configuration.Get("AzureCosmosEndpoint");
            string authKey = Configuration.Get("AzureCosmosKey");
            string database = Configuration.Get("DatabaseName");
            string collection = Configuration.Get("ConversationLogTable");

            CosmosDbRepository repo = CosmosDbRepository.Instance
                .ConnectTo(endpoint)
                .WithAuthKeyOrResourceToken(authKey)
                .UsingDatabase(database)
                .UsingCollection(collection);

            return repo;
        }

        private static HttpClient InitializeHttpClient()
        {
            var client = new HttpClient { BaseAddress = new Uri(Configuration.Get("DirectLineAddress")) };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Configuration.Get("BotClientAuthToken"));

            return client;
        }

        private static async Task PostToConversation(dynamic incomingSms, BotConversation conversation, ILogger log)
        {
            log.LogInformation($"Posting message to conversationId {conversation.ConversationId}");

            dynamic from = new ExpandoObject();
            from.id = incomingSms?.source_number;
            from.name = incomingSms?.source_number;
            from.role = null;

            dynamic channelData = new ExpandoObject();
            channelData.UniqueLearnerNumber = conversation.UniqueLearnerNumber ?? incomingSms?.Uln;
            channelData.NotifyMessage = new NotifyMessage()
                                             {
                                                 Id = incomingSms?.id,
                                                 DateReceived = incomingSms?.date_received,
                                                 DestinationNumber =
                                                     incomingSms?.destination_number,
                                                 SourceNumber = incomingSms?.source_number,
                                                 Message = incomingSms?.message,
                                                 Type = "callback",
                                             };

            var messageContent = new BotConversationMessage()
                                     {
                                         Type = "message",
                                         From = from,
                                         Text = incomingSms?.message,
                                         ChannelData = channelData
                                     };

            var json = JsonConvert.SerializeObject(messageContent);
            HttpContent content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage postMessageTask = await DirectLineClient.PostAsync($"/v3/directline/conversations/{conversation.ConversationId}/activities", content);

            if (postMessageTask.IsSuccessStatusCode)
            {
                string response = await postMessageTask.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(response);
                log.LogInformation($"Received response from Bot Client: {jsonResponse.id}");
            }
            else
            {
                var message = $"Could not post conversation to DirectLineClient. {postMessageTask.StatusCode}: {postMessageTask.ReasonPhrase}";
                throw new BotConnectorException(message);
            }
        }

        private static async Task StartNewConversation(dynamic incomingSms, ILogger log)
        {
            log.LogInformation($"Starting new conversation with {incomingSms?.source_number}");

            var content = new StringContent(string.Empty);

            var startConversationTask = await DirectLineClient.PostAsync("/v3/directline/conversations", content);

            var conversation = new BotConversation();
            if (startConversationTask.IsSuccessStatusCode)
            {
                string response = await startConversationTask.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(response);
                log.LogInformation($"Started new conversation with id {jsonResponse.conversationId}");

                // TODO: write the conversation ID to a session log with the mobile phone number
                conversation.UserId = incomingSms?.ource_number; // TODO: [security] hash this please!
                conversation.ConversationId = jsonResponse.conversationId;
                conversation.UniqueLearnerNumber = incomingSms?.Uln;

                BotConversation newSession = await DocumentClient.UpsertItemAsync(conversation);
                if (newSession.IsNull())
                {
                    var message = $"Could not create session object for conversation id {conversation.ConversationId}";
                    throw new BotConnectorException(message);
                }

                if (incomingSms != null)
                {
                    await PostToConversation(incomingSms, conversation, log);
                }
            }
            else
            {
                var message = $"Could not start new conversation with DirectLineClient. {startConversationTask.StatusCode}: {startConversationTask.ReasonPhrase}";
                throw new BotConnectorException(message);
            }
        }
    }
}