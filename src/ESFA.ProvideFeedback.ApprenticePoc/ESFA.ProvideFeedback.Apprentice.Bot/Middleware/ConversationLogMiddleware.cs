using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ESFA.ProvideFeedback.Apprentice.Bot.Config;
using Microsoft.Bot.Connector;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace ESFA.ProvideFeedback.Apprentice.Bot.Middleware
{
    public class NotifyQueueMiddleware : IMiddleware
    {
        public CloudStorageAccount storageAccount;
        public CloudQueueClient QueueClient;

        public IConfiguration Configuration;

        public NotifyQueueMiddleware()
        {

        }

        ~NotifyQueueMiddleware()
        {
            //storageAccount.Dispose();
        }

        public Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            throw new NotImplementedException();
        }
    }


    public class ConversationLogMiddleware : IMiddleware

    {
        private AzureConfig _azureConfig;
        private DataConfig _dataConfig;

        public DocumentClient DocClient;

        public ConversationLogMiddleware(IOptions<AzureConfig> azureConfig, IOptions<DataConfig> dataConfig)
        {
            _azureConfig = azureConfig.Value;
            _dataConfig = dataConfig.Value;

            var endpoint = _azureConfig.CosmosEndpoint;
            var key = _azureConfig.CosmosKey;
            DocClient = new DocumentClient(new Uri(endpoint), key);
            CreateDatabaseAndCollection().ConfigureAwait(false);
        }

        ~ConversationLogMiddleware()
        {
            DocClient.Dispose();
        }

        private async Task CreateDatabaseAndCollection()
        {
            try
            {
                await DocClient.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_dataConfig.DatabaseName));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await DocClient.CreateDatabaseAsync(new Database {Id = _dataConfig.DatabaseName});
                }
                else
                {
                    throw;
                }
            }

            try
            {
                await DocClient.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri
                    (_dataConfig.DatabaseName, _dataConfig.ConversationLogTable));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    DocumentCollection conversationCollection = new DocumentCollection
                    {
                        Id = _dataConfig.ConversationLogTable
                    };
                    conversationCollection.PartitionKey.Paths.Add("/conversation/id");

                    RequestOptions collectionSettings = new RequestOptions
                    {
                        OfferThroughput = 1000
                    };

                    await DocClient.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(_dataConfig.DatabaseName),
                        conversationCollection,
                        collectionSettings);
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<string> ReadFromDatabase(int numberOfRecords)
        {
            var documents = DocClient.CreateDocumentQuery<ConversationLog>(
                    UriFactory.CreateDocumentCollectionUri(_dataConfig.DatabaseName, _dataConfig.ConversationLogTable))
                .AsDocumentQuery();
            List<ConversationLog> messages = new List<ConversationLog>();
            while (documents.HasMoreResults)
            {
                messages.AddRange(await documents.ExecuteNextAsync<ConversationLog>());
            }

            // Create a sublist of messages containing the number of requested records.
            List<ConversationLog> messageSublist = messages.GetRange(messages.Count - numberOfRecords, numberOfRecords);

            string history = "";

            // Send the last 3 messages.
            foreach (ConversationLog logEntry in messageSublist)
            {
                history += ("Message was: " + logEntry.Message + " Reply was: " + logEntry.Reply + "  ");
            }

            return history;
        }

        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            string botReply = "";

            if (context.Activity.Type == ActivityTypes.Message)
            {
                if (context.Activity.Text == "history")
                {
                    // Read last 3 responses from the database, and short circuit future execution.
                    await context.SendActivity(await ReadFromDatabase(3));
                    return;
                }

                // Create a send activity handler to grab all response activities 
                // from the activity list.
                context.OnSendActivities(async (activityContext, activityList, activityNext) =>
                {
                    foreach (Activity activity in activityList)
                    {
                        botReply += (activity.Text + " ");
                    }
                    return await activityNext();
                });
            }

            // Pass execution on to the next layer in the pipeline.
            await next();

            // Save logs for each conversational exchange only.
            if (context.Activity.Type == ActivityTypes.Message)
            {
                // Build a log object to write to the database.
                var logData = new ConversationLog
                {
                    From = context.Activity.From,
                    Recipient = context.Activity.Recipient,
                    Conversation = context.Activity.Conversation,
                    ChannelData = context.Activity.ChannelData,
                    ChannelId = context.Activity.ChannelId,
                    Time = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                    Message = context.Activity.Text,
                    Reply = botReply
                };

                // Write our log to the database.
                try
                    {
                    var document = await DocClient.CreateDocumentAsync(UriFactory.
                        CreateDocumentCollectionUri(_dataConfig.DatabaseName, _dataConfig.ConversationLogTable), logData);
                }
                catch (Exception ex)
                {
                    // More logic for what to do on a failed write can be added here
                    throw ex;
                }
            }
        }
    }
}