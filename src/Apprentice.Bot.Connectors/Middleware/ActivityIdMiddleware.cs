namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System.Threading;
    using System.Threading.Tasks;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using System.Linq;
    using Microsoft.Bot.Builder.Dialogs;

    public class ActivityIdMiddleware : IMiddleware
    {
        private readonly FeedbackBotStateRepository feedbackBotStateRepository;

        public ActivityIdMiddleware(FeedbackBotStateRepository feedbackBotStateRepository)
        {
            this.feedbackBotStateRepository = feedbackBotStateRepository;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ActivityIdMiddleware"/> class. 
        /// </summary>
        ~ActivityIdMiddleware()
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

                            if (activity.Id == null)
                            {
                                var dialogState = await feedbackBotStateRepository.ConversationDialogState.GetAsync(context);

                                var dialogInstance = dialogState.DialogStack?.FirstOrDefault()?.State.First().Value as DialogState;
                                activity.Id = dialogInstance?.DialogStack?.FirstOrDefault()?.Id;
                            }

                        }
                        return await activityNext();
                    });
            }

            await next.Invoke(cancellationToken);
        }
    }
}