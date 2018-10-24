namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands.Dialog
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Commands.Dialog;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder.Dialogs;

    public sealed class StartDialogCommand : AdminCommand, IBotDialogCommand
    {
        private readonly FeedbackBotStateRepository state;

        public StartDialogCommand(FeedbackBotStateRepository state)
            : base("start")
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            try
            {
                UserProfile userProfile = await this.state.UserProfile.GetAsync(dc.Context, () => new UserProfile(), cancellationToken);
                userProfile.SurveyState = new SurveyState();

                string message = dc.Context.Activity.Text.ToLowerInvariant();

                var strings = message.Split(new[] { " ", "|" }, StringSplitOptions.RemoveEmptyEntries);

                if (strings.Length > 1)
                {
                    string dialogId = strings[1];

                    dynamic channelData = dc.Context.Activity.ChannelData;
                    userProfile.IlrNumber = channelData?.UniqueLearnerNumber;
                    userProfile.SurveyState.SurveyId = dialogId;
                    userProfile.SurveyState.StartDate = DateTime.Now;
                    userProfile.SurveyState.Progress = ProgressState.InProgress;

                    // TODO: check dialog collection
                    return await dc.BeginDialogAsync(dialogId, null, cancellationToken);
                }
                else
                {
                    // this.logger.LogError($"could not find dialogId in command \"{ message }\"");
                    return await dc.CancelAllDialogsAsync(cancellationToken);
                }
            }
            catch (Exception e)
            {
                // this.logger.LogError(e.Message);
#if DEBUG
                await dc.Context.SendActivityAsync($"DEBUG: {e}", cancellationToken: cancellationToken);
#endif
                return await dc.CancelAllDialogsAsync(cancellationToken);
            }
        }
    }
}