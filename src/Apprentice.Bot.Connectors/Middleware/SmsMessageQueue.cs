namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services;

    using Microsoft.Azure.ServiceBus;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Options;

    using Newtonsoft.Json;

    public class SmsMessageQueue : IMessageQueueMiddleware
    {
        private readonly ISmsQueueProvider smsQueueProvider;
        private readonly Notify notifyConfig;

        public SmsMessageQueue(ISmsQueueProvider smsQueueProvider, IOptions<Notify> notifyConfigOptions)
        {
            this.notifyConfig = notifyConfigOptions.Value;
            this.smsQueueProvider = smsQueueProvider;
        }

        public async Task EnqueueMessageAsync(ITurnContext context, Activity activity)
        {
            OutgoingSms sms = new OutgoingSms
                {
                    From = context.Activity.From,
                    Recipient = context.Activity.Recipient,
                    Conversation = context.Activity.Conversation,
                    ChannelData = context.Activity.ChannelData,
                    ChannelId = context.Activity.ChannelId,
                    Time = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                    Message = activity.Text,
                };

            var message = JsonConvert.SerializeObject(sms);

            await this.smsQueueProvider.SendAsync(message, this.notifyConfig.OutgoingMessageQueueName);
        }

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
                            dynamic channelData = turnContext.Activity.ChannelData;
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

                                await this.EnqueueMessageAsync(turnContext, activity);
                            }

                            return await activityNext();
                        });
            }

            // Pass execution on to the next layer in the pipeline.
            await next.Invoke(cancellationToken);
        }
    }
}