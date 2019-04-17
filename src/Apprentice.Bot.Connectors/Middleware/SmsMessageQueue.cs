namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using ESFA.DAS.ProvideFeedback.Apprentice.Domain.Dto;
    using ESFA.DAS.ProvideFeedback.Apprentice.Domain.Messages;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Options;

    using Newtonsoft.Json;

    public class SmsMessageQueue : IMessageQueueMiddleware
    {
        private readonly ISmsQueueProvider smsQueueProvider;
        private readonly Notify notifyConfig;
        private readonly FeedbackBotStateRepository feedbackBotStateRepository;

        public SmsMessageQueue(ISmsQueueProvider smsQueueProvider, IOptions<Notify> notifyConfigOptions, FeedbackBotStateRepository feedbackBotStateRepository)
        {
            this.notifyConfig = notifyConfigOptions.Value;
            this.smsQueueProvider = smsQueueProvider;
            this.feedbackBotStateRepository = feedbackBotStateRepository;
        }

        public async Task EnqueueMessageAsync(ITurnContext context, Activity activity)
        {
            var turnProperty = feedbackBotStateRepository.ConversationState.CreateProperty<long>("turnId");
            var turnId = await turnProperty.GetAsync(context, () => -1);

            OutgoingSms sms = new OutgoingSms
            {
                From = new Participant { UserId = context.Activity.From.Id },
                Recipient = new Participant { UserId = context.Activity.Recipient.Id },
                Conversation = new BotConversation
                {
                    ConversationId = context.Activity.Conversation.Id,
                    UserId = context.Activity.From.Id,
                    ActivityId = activity.Id,
                    TurnId = turnId
                },
                ChannelData = context.Activity.ChannelData,
                ChannelId = context.Activity.ChannelId,
                Time = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                Message = activity.Text,
            };

            var message = new SmsOutgoingMessage(sms);

            await this.smsQueueProvider.SendAsync(activity.Conversation.Id, message, this.notifyConfig.OutgoingMessageQueueName);
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