namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    public class TurnIdMiddleware : IMiddleware
    {
        private readonly FeedbackBotStateRepository feedbackBotStateRepository;

        public TurnIdMiddleware(FeedbackBotStateRepository feedbackBotStateRepository)
        {
            this.feedbackBotStateRepository = feedbackBotStateRepository;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="TurnIdMiddleware"/> class. 
        /// </summary>
        ~TurnIdMiddleware()
        {
            // cleanup
        }

        /// <inheritdoc />
        public async Task OnTurnAsync(
            ITurnContext context,
            NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                context.OnSendActivities(
                    async (activityContext, activityList, activityNext) =>
                    {
                        foreach (Activity activity in activityList)
                        {
                            if (activity.Type != ActivityTypes.Message || !activity.HasContent())
                            {
                                continue;
                            }

                            var turnProperty = feedbackBotStateRepository.ConversationState.CreateProperty<long>("turnId");
                            var turnId = await turnProperty.GetAsync(activityContext, defaultValueFactory: () => 0, cancellationToken: cancellationToken);
                            await turnProperty.SetAsync(activityContext, ++turnId, cancellationToken);
                            await feedbackBotStateRepository.ConversationState.SaveChangesAsync(activityContext, cancellationToken: cancellationToken);
                        }
                        return await activityNext();
                    });
            }

            await next.Invoke(cancellationToken);
        }    
    }
}