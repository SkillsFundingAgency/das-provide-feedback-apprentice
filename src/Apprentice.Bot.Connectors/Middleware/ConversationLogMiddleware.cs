namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using ESFA.DAS.ProvideFeedback.Apprentice.Data;
    using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;

    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    /// <inheritdoc />
    public class ConversationLogMiddleware : IMiddleware
    {
        private readonly Azure azureConfig;
        
        private readonly Data dataConfig;

        private readonly IDataRepository conversationLog;

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
            this.conversationLog = this.CreateConversationLogClient();
        }

        private IDataRepository CreateConversationLogClient()
        {
            return CosmosDbRepository.Instance
                .ConnectTo(this.azureConfig.CosmosEndpoint)
                .WithAuthKeyOrResourceToken(this.azureConfig.CosmosEndpoint)
                .UsingDatabase(this.dataConfig.DatabaseName)
                .UsingCollection(this.dataConfig.ConversationLogTable);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ConversationLogMiddleware"/> class. 
        /// </summary>
        ~ConversationLogMiddleware()
        {
            // cleanup
        }

        /// <inheritdoc />
        public async Task OnTurnAsync(
            ITurnContext context,
            NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            string botReply = string.Empty;

            if (context.Activity.Type == ActivityTypes.Message)
            {
                // Create a send activity handler to grab all response activities 
                // from the activity list.
                context.OnSendActivities(
                    async (activityContext, activityList, activityNext) =>
                        {
                            botReply = string.Join("\n\n", activityList.SelectMany(a => a.Text));
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
                    await this.conversationLog.UpsertItemAsync(logData);
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