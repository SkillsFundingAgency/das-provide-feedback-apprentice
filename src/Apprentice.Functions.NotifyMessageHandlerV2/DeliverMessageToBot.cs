namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    using System;
    using System.Dynamic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;
    using ESFA.DAS.ProvideFeedback.Apprentice.Data;
    using ESFA.DAS.ProvideFeedback.Apprentice.Functions;
    using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Dto;
    using Microsoft.Azure.Documents.SystemFunctions;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
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
        /// <param name="incomingSms">The incoming SMS payload.</param>
        /// <param name="log">the host <see cref="TraceWriter"/></param>
        /// <param name="context">the function <see cref="ExecutionContext"/></param>
        /// <returns> the <see cref="Task"/> </returns>
        [FunctionName("DeliverMessageToBot")]
        public static async Task Run(
        [ServiceBusTrigger("sms-incoming-messages", Connection = "ServiceBusConnection")]
        string queueMessage,
        ILogger log,
        ExecutionContext context)
        {
            log.LogInformation($"Queue message {queueMessage}");
            currentContext = context;
            dynamic incomingSms = JsonConvert.DeserializeObject<dynamic>(queueMessage);

            try
            {
                string mobileNumber = incomingSms?.Value?.source_number;
                BotConversation conversation = await GetConversationByMobileNumber(mobileNumber);

                if (conversation == null)
                {
                    await StartNewConversation(incomingSms, log);
                }
                else
                {
                    await PostToConversation(incomingSms, conversation, log);
                }
            }
            catch (Exception e)
            {
                log.LogInformation($"Bot Connector Exception: {e.Message}");
                DirectLineClient.CancelPendingRequests();
                throw new BotConnectorException(
                    "Something went wrong when relaying the message to the bot framework",
                    e);
            }
        }

        private static async Task<BotConversation> GetConversationByMobileNumber(string mobileNumber)
        {
            // TODO: extract this and inject an instance of IBotConversationProvider
            await DocumentClient.GetDocumentCollectionAsync();
            BotConversation conversation = await DocumentClient.GetItemAsync<BotConversation>(c => c.MobileNumber == mobileNumber);
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
            log.LogInformation($"Received response from {incomingSms?.Value?.source_number}");

            dynamic from = new ExpandoObject();
            from.id = incomingSms?.Value?.source_number;
            from.name = incomingSms?.Value?.source_number;
            from.role = null;

            dynamic channelData = new ExpandoObject();
            channelData.NotifyMessage = new NotifyMessage()
                                             {
                                                 Id = incomingSms?.Value?.id,
                                                 DateReceived = incomingSms?.Value?.date_received,
                                                 DestinationNumber =
                                                     incomingSms?.Value?.destination_number,
                                                 SourceNumber = incomingSms?.Value?.source_number,
                                                 Message = incomingSms?.Value?.message,
                                                 Type = "callback",
                                             };

            var messageContent = new BotConversationMessage()
                                     {
                                         Type = "message",
                                         From = from,
                                         Text = incomingSms?.Value?.message,
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
                log.LogInformation($"Could not post conversation. {postMessageTask.StatusCode}: {postMessageTask.ReasonPhrase}");
                log.LogInformation($"{JsonConvert.SerializeObject(postMessageTask)}");
            }
        }

        private static async Task StartNewConversation(dynamic incomingSms, ILogger log)
        {
            log.LogInformation($"Starting new conversation with {incomingSms?.Value?.source_number}");

            var content = new StringContent(string.Empty);

            var startConversationTask = await DirectLineClient.PostAsync("/v3/directline/conversations", content);

            var conversation = new BotConversation();
            if (startConversationTask.IsSuccessStatusCode)
            {
                string response = await startConversationTask.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(response);
                log.LogInformation($"Started new conversation with id {jsonResponse.conversationId}");

                // TODO: write the conversation ID to a session log with the mobile phone number
                conversation.MobileNumber = incomingSms?.Value.source_number;
                conversation.ConversationId = jsonResponse.conversationId;

                BotConversation newSession = await DocumentClient.UpsertItemAsync(conversation);
                if (newSession.IsNull())
                {
                    throw new BotConnectorException($"Could not create session object for conversation id {conversation.ConversationId}");
                }

                if (incomingSms != null)
                {
                    await PostToConversation(incomingSms, conversation, log);
                }
            }
            else
            {
                log.LogInformation($"Could not start new conversation. {startConversationTask.StatusCode}: {startConversationTask.ReasonPhrase}");
                log.LogInformation($"{JsonConvert.SerializeObject(startConversationTask)}");
            }
        }
    }
}