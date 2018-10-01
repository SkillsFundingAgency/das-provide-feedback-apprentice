﻿using System;
using System.Globalization;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System.Threading;

    using AzureConfiguration = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Azure;
    using ConnectionStrings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.ConnectionStrings;
    using DataConfiguration = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Data;
    using NotifyConfiguration = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Notify;
    
    /// <inheritdoc />
    /// <summary>
    /// Adds middleware to write conversation responses to a Queue for the purposes of using Notify to relay the message.
    /// </summary>
    public class AzureStorageQueueSmsRelay : IMessageQueueMiddleware
    {
        private readonly AzureConfiguration azureConfig;

        private readonly DataConfiguration dataConfig;

        private readonly NotifyConfiguration notifyConfig;

        private readonly ConnectionStrings connectionStrings;

        private readonly CloudQueueClient queueClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageQueueSmsRelay"/> class.
        /// </summary>
        /// <param name="azureConfigOptions"> the azure configuration options </param>
        /// <param name="dataConfigOptions"> the database configuration options </param>
        /// <param name="notifyConfigOptions"> the Notify service configuration options </param>
        /// <param name="connectionStringsOptions"> the connections strings options </param>
        public AzureStorageQueueSmsRelay(
            IOptions<AzureConfiguration> azureConfigOptions,
            IOptions<DataConfiguration> dataConfigOptions,
            IOptions<NotifyConfiguration> notifyConfigOptions,
            IOptions<ConnectionStrings> connectionStringsOptions)
        {
            this.azureConfig = azureConfigOptions.Value;
            this.dataConfig = dataConfigOptions.Value;
            this.notifyConfig = notifyConfigOptions.Value;
            this.connectionStrings = connectionStringsOptions.Value;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.connectionStrings.StorageAccount);

            this.queueClient = storageAccount.CreateCloudQueueClient();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AzureStorageQueueSmsRelay"/> class. 
        /// </summary>
        ~AzureStorageQueueSmsRelay()
        {
            // Tidy up
        }

        /// <inheritdoc />
        /// <summary>
        /// Intercepts each turn to determine whether the source of the message is from the bespoke DirectLine based NotifyConnector.
        /// Pushes the outgoing message on to a queue if so.
        /// </summary>
        /// <param name="context">The turn context</param>
        /// <param name="next">the next OnTurn operation in the pipeline</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The <see cref="T:System.Threading.Tasks.Task" /></returns>
        public async Task OnTurnAsync(
            ITurnContext context,
            NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                // Create a send activity handler to grab all response activities 
                // from the activity list.
                context.OnSendActivities(
                    async (activityContext, activityList, activityNext) =>
                        {
                            dynamic channelData = context.Activity.ChannelData;
                            if (channelData?.NotifyMessage == null)
                            {
                                return await activityNext();
                            }

                            foreach (Activity activity in activityList)
                            {
                                if (activity.Type != ActivityTypes.Message || !activity.HasContent())
                                {
                                    continue;
                                }

                                await this.EnqueueMessageAsync(context, activity);
                            }

                            return await activityNext();
                        });
            }

            // Pass execution on to the next layer in the pipeline.
            await next.Invoke(cancellationToken);
        }

        /// <inheritdoc />
        /// <summary>
        /// Adds a message on to the Azure Storage Queue, ready for processing </summary>
        /// <param name="context"> The <see cref="T:Microsoft.Bot.Builder.ITurnContext" /> of the conversation turn/&gt; </param>
        /// <param name="activity"> The <see cref="T:Microsoft.Bot.Schema.Activity" /> that will be used for the queue message </param>
        /// <returns> The <see cref="T:System.Threading.Tasks.Task" />. </returns>
        public async Task EnqueueMessageAsync(ITurnContext context, Activity activity)
        {
            CloudQueue messageQueue = this.queueClient.GetQueueReference(this.notifyConfig.OutgoingMessageQueueName);
            await messageQueue.CreateIfNotExistsAsync();

            NotifySms sms = new NotifySms
            {
                From = context.Activity.From,
                Recipient = context.Activity.Recipient,
                Conversation = context.Activity.Conversation,
                ChannelData = context.Activity.ChannelData,
                ChannelId = context.Activity.ChannelId,
                Time = DateTime.Now.ToString(
                                        CultureInfo.InvariantCulture),
                Message = activity.Text,
            };

            CloudQueueMessage message =
                new CloudQueueMessage(JsonConvert.SerializeObject(sms));
            await messageQueue.AddMessageAsync(message);
        }
    }
}