namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;

    public class ActivityIdMiddleware : IMiddleware
    {
        private readonly IFeedbackBotStateRepository feedbackBotStateRepository;

        public ActivityIdMiddleware(IFeedbackBotStateRepository feedbackBotStateRepository)
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

                            if (activity.Text != "OK. Resetting conversation...")
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