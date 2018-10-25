using System;
using System.Collections.Generic;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
using ESFA.DAS.ProvideFeedback.Apprentice.Data;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Dto;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    public static class DailySurveyTrigger
    {
        private static readonly Lazy<SettingsProvider> LazyConfigProvider = new Lazy<SettingsProvider>(Configure);
        private static ExecutionContext currentContext;
        public static SettingsProvider Configuration => LazyConfigProvider.Value;
        private static CosmosDbRepository DocumentClient => InitializeDocumentClient();

        [FunctionName("DailySurveyTrigger")]
        public static async void Run(
            [TimerTrigger("0 0 11 * * MON-FRI")]TimerInfo myTimer,
            ILogger log,
            [ServiceBus("sms-incoming-messages", Connection = "ServiceBusConnection", EntityType = Microsoft.Azure.WebJobs.ServiceBus.EntityType.Queue)] ICollector<string> outputSbQueue,
            ExecutionContext executionContext)
        {
            currentContext = executionContext;
            log.LogInformation($"Daily survey trigger started");

            var batchSize = 100;
            var apprenticeDetails = await DocumentClient.GetAllItemsAsync<ApprenticeDetail>(new FeedOptions { MaxItemCount = batchSize });

            foreach (var apprenticeDetail in apprenticeDetails)
            {
                var now = DateTime.Now;
                var trigger = new SmsConversationTrigger()
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceNumber = apprenticeDetail.MobileNumber,
                    DestinationNumber = null,
                    Message = $"start {apprenticeDetail.SurveyCode}",
                    DateReceived = now
                };

                var payload = new KeyValuePair<string, SmsConversationTrigger>("bot-manual-trigger", trigger);

                outputSbQueue.Add(JsonConvert.SerializeObject(payload));

                apprenticeDetail.SentDate = now;
                await DocumentClient.UpsertItemAsync(apprenticeDetail);
            }
        }

        private static CosmosDbRepository InitializeDocumentClient()
        {
            string endpoint = Configuration.Get("AzureCosmosEndpoint");
            string authKey = Configuration.Get("AzureCosmosKey");
            string database = Configuration.Get("DatabaseName");
            string collection = Configuration.Get("ApprenticeSurveyDetailTable");

            CosmosDbRepository repo = CosmosDbRepository.Instance
                .ConnectTo(endpoint)
                .WithAuthKeyOrResourceToken(authKey)
                .UsingDatabase(database)
                .UsingCollection(collection);

            return repo;
        }

        private static SettingsProvider Configure()
        {
            if (currentContext == null)
            {
                throw new Exception("Could not initialize the settings provider, ExecutionContext is null");
            }

            return new SettingsProvider(currentContext);
        }
    }
}
