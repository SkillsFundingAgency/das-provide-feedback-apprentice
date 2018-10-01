namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;

    using Microsoft.Azure.ServiceBus;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Options;
    using Microsoft.WindowsAzure.Storage.Queue;

    using Newtonsoft.Json;

    using AzureConfiguration = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Azure;
    using ConnectionStrings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.ConnectionStrings;
    using DataConfiguration = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Data;
    using NotifyConfiguration = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Notify;

    public class AzureServiceBusQueueSmsRelay : IMessageQueueMiddleware
    {
        private readonly AzureConfiguration azureConfig;

        private readonly DataConfiguration dataConfig;

        private readonly NotifyConfiguration notifyConfig;

        private readonly ConnectionStrings connectionStrings;

        private static IQueueClient queueClient;

        public AzureServiceBusQueueSmsRelay(
            IOptions<AzureConfiguration> azureConfigOptions,
            IOptions<DataConfiguration> dataConfigOptions,
            IOptions<NotifyConfiguration> notifyConfigOptions,
            IOptions<ConnectionStrings> connectionStrings)
        {
            this.azureConfig = azureConfigOptions.Value;
            this.dataConfig = dataConfigOptions.Value;
            this.notifyConfig = notifyConfigOptions.Value;
            this.connectionStrings = connectionStrings.Value;

            queueClient = new QueueClient(this.connectionStrings.ServiceBus, this.notifyConfig.OutgoingMessageQueueName);
        }

        ~AzureServiceBusQueueSmsRelay()
        {
            queueClient.CloseAsync();
        }

        /// <inheritdoc />
        public async Task OnTurnAsync(
            ITurnContext turnContext,
            NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Create a send activity handler to grab all response activities 
                // from the activity list.
                turnContext.OnSendActivities(
                    async (activityContext, activityList, activityNext) =>
                    {
                        //dynamic channelData = context.Activity.ChannelData;
                        //if (channelData?.NotifyMessage == null)
                        //{
                        //    return await activityNext();
                        //}

                        foreach (Activity activity in activityList)
                        {
                            if (activity.Type != ActivityTypes.Message || !activity.HasContent())
                            {
                                continue;
                            }

                            await this.EnqueueMessageAsync(turnContext, activity);
                        }

                        return await activityNext();
                    });
            }

            // Pass execution on to the next layer in the pipeline.
            await next.Invoke(cancellationToken);
        }

        public async Task EnqueueMessageAsync(ITurnContext context, Activity activity)
        {
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

            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sms)));
            await queueClient.SendAsync(message);
        }
    }
}