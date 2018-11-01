using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
using ESFA.DAS.ProvideFeedback.Apprentice.Data;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    public static class DailySurveyTrigger
    {
        private static readonly Lazy<IStoreApprenticeSurveyDetails> LazyDataStoreProvider = new Lazy<IStoreApprenticeSurveyDetails>(ConfigureDataStore);

        private static readonly Lazy<SettingsProvider> LazyConfigProvider = new Lazy<SettingsProvider>(Configure);
        private static ExecutionContext currentContext;
        public static SettingsProvider Configuration => LazyConfigProvider.Value;
        private static IStoreApprenticeSurveyDetails _surveyDetailsRepo => LazyDataStoreProvider.Value;

        [FunctionName("DailySurveyTrigger")]
        public static async Task Run(
            [TimerTrigger("0 0 11 * * MON-FRI")]TimerInfo myTimer,
            ILogger log,
            [ServiceBus("sms-incoming-messages", Connection = "ServiceBusConnection", EntityType = Microsoft.Azure.WebJobs.ServiceBus.EntityType.Queue)] ICollector<string> outputSbQueue,
            ExecutionContext executionContext)
        {
            currentContext = executionContext;
            log.LogInformation($"Daily survey trigger started");

            var batchSize = 100;
            var apprenticeDetails = await GetApprenticeDetailsToSendSurvey(batchSize);

            foreach (var apprenticeDetail in apprenticeDetails)
            {
                var now = DateTime.Now;
                var trigger = new SmsConversationTrigger()
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceNumber = apprenticeDetail.MobileNumber.ToString(),
                    DestinationNumber = null,
                    Message = $"start {apprenticeDetail.SurveyCode}",
                    DateReceived = now
                };

                var payload = new KeyValuePair<string, SmsConversationTrigger>("bot-manual-trigger", trigger);

                outputSbQueue.Add(JsonConvert.SerializeObject(payload));

                await _surveyDetailsRepo.SetApprenticeSurveySentAsync(apprenticeDetail.MobileNumber, apprenticeDetail.SurveyCode);
            }
        }

        private static Task<IEnumerable<ApprenticeSurveyDetail>> GetApprenticeDetailsToSendSurvey(int batchSize)
        {
            return _surveyDetailsRepo.GetApprenticeSurveyDetailsAsync(batchSize);
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

        private static IStoreApprenticeSurveyDetails ConfigureDataStore()
        {
            return new ApprenticeSurveyDetailsRepository(new SqlConnection(Configuration.Get("SqlConnectionString"));
        }
    }
}
