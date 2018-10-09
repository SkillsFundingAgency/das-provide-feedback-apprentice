namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using ESFA.DAS.ProvideFeedback.Apprentice.Data;

    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    /// <inheritdoc />
    public class ConversationLogMiddleware : IMiddleware
    {
        /// <summary>
        /// The configuration of the Azure instance
        /// </summary>
        private readonly Azure azureConfig;

        /// <summary>
        /// The database configuration
        /// </summary>
        private readonly Data dataConfig;

        /// <summary>
        /// The cosmos Db client
        /// </summary>
        private readonly DocumentClient docClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationLogMiddleware"/> class. 
        /// </summary>
        /// <param name="azureConfig">
        /// the azure configuration options
        /// </param>
        /// <param name="dataConfig">
        /// the database configuration options
        /// </param>
        public ConversationLogMiddleware(Azure azureConfig, Data dataConfig)
        {
            this.azureConfig = azureConfig;
            this.dataConfig = dataConfig;

            string endpoint = this.azureConfig.CosmosEndpoint;
            string key = this.azureConfig.CosmosKey;
            this.docClient = new DocumentClient(
                new Uri(endpoint),
                key,
                new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp });

            this.CreateDatabaseAndCollection().ConfigureAwait(false);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ConversationLogMiddleware"/> class. 
        /// </summary>
        ~ConversationLogMiddleware()
        {
            this.docClient.Dispose();
        }

        public async Task OnTurnAsync(
            ITurnContext context,
            NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            string botReply = string.Empty;

            if (context.Activity.Type == ActivityTypes.Message)
            {
                if (context.Activity.Text == "history")
                {
                    // Read last 3 responses from the database, and short circuit future execution.
                    await context.SendActivityAsync(await this.ReadFromDatabase(3), cancellationToken: cancellationToken);
                    return;
                }

                // Create a send activity handler to grab all response activities 
                // from the activity list.
                context.OnSendActivities(
                    async (activityContext, activityList, activityNext) =>
                    {
                        foreach (Activity activity in activityList)
                        {
                            botReply += $"{activity.Text} ";
                        }

                        return await activityNext();
                    });
            }

            // Pass execution on to the next layer in the pipeline.
            await next(cancellationToken);

            // Save logs for each conversational exchange only.
            if (context.Activity.Type == ActivityTypes.Message)
            {
                // Build a log object to write to the database.
                ConversationLog logData = new ConversationLog
                {
                    From = context.Activity.From,
                    Recipient = context.Activity.Recipient,
                    Conversation = context.Activity.Conversation,
                    ChannelData = context.Activity.ChannelData,
                    ChannelId = context.Activity.ChannelId,
                    Time = DateTime.Now.ToString(
                                                      CultureInfo.InvariantCulture),
                    Message = context.Activity.Text,
                    Reply = botReply
                };

                // Write our log to the database.
                try
                {
                    await this.docClient.CreateDocumentAsync(
                        UriFactory.CreateDocumentCollectionUri(
                            this.dataConfig.DatabaseName,
                            this.dataConfig.ConversationLogTable),
                        logData,
                        cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    // More logic for what to do on a failed write can be added here
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Read a number of history items from the configured CosmosDb database
        /// </summary>
        /// <param name="numberOfRecords">the number of records to return</param>
        /// <returns>A string representation of the log items</returns>
        public async Task<string> ReadFromDatabase(int numberOfRecords)
        {
            var documents = this.docClient.CreateDocumentQuery<ConversationLog>(
                UriFactory.CreateDocumentCollectionUri(
                    this.dataConfig.DatabaseName,
                    this.dataConfig.ConversationLogTable)).AsDocumentQuery();
            var messages = new List<ConversationLog>();
            while (documents.HasMoreResults)
            {
                messages.AddRange(await documents.ExecuteNextAsync<ConversationLog>());
            }

            // Create a sublist of messages containing the number of requested records.
            var messageSublist = messages.GetRange(messages.Count - numberOfRecords, numberOfRecords);

            string history = string.Empty;

            // Send the last 3 messages.
            foreach (ConversationLog logEntry in messageSublist)
            {
                history += $"Message was: {logEntry.Message} Reply was: {logEntry.Reply} ";
            }

            return history;
        }

        /// <summary>
        /// Create the cosmos Db database and collection
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        private async Task CreateDatabaseAndCollection()
        {
            try
            {
                await this.docClient.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(this.dataConfig.DatabaseName));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await this.docClient.CreateDatabaseAsync(new Database { Id = this.dataConfig.DatabaseName });
                }
                else
                {
                    throw;
                }
            }

            try
            {
                await this.docClient.ReadDocumentCollectionAsync(
                    UriFactory.CreateDocumentCollectionUri(
                        this.dataConfig.DatabaseName,
                        this.dataConfig.ConversationLogTable));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    DocumentCollection conversationCollection =
                        new DocumentCollection { Id = this.dataConfig.ConversationLogTable };
                    if (this.dataConfig.ConversationLogPartitionKey != string.Empty)
                    {
                        conversationCollection.PartitionKey.Paths.Add(this.dataConfig.ConversationLogPartitionKey);
                    }

                    RequestOptions collectionSettings =
                        new RequestOptions { OfferThroughput = this.azureConfig.CosmosDefaultThroughput };

                    await this.docClient.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(this.dataConfig.DatabaseName),
                        conversationCollection,
                        collectionSettings);
                }
                else
                {
                    throw;
                }
            }
        }
    }

    public class CosmosFeedbackProvider
    {
        private readonly IDataRepository conversationLog;
    }
}