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

    using BotConfiguration = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;

    public sealed class StartDialogCommand : AdminCommand, IBotDialogCommand
    {
        private readonly FeedbackBotStateRepository state;

        public StartDialogCommand(FeedbackBotStateRepository state, BotConfiguration botConfiguration)
            : base("bot_dialog_start", botConfiguration)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public override async Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            try
            {
                UserProfile userProfile = await this.state.UserProfile.GetAsync(dc.Context, () => new UserProfile(), cancellationToken);
                userProfile.SurveyState = new SurveyState();

                if (userProfile.SurveyState.Progress == ProgressState.NotStarted)
                {
                    string message = dc.Context.Activity.Text.ToLowerInvariant();

                    var strings = message.Split(new[] { " ", "|" }, StringSplitOptions.RemoveEmptyEntries);

                    if (strings.Length > 1)
                    {
                        string dialogId = strings[1];

                        dynamic channelData = dc.Context.Activity.ChannelData;
                        userProfile.IlrNumber = channelData?.UniqueLearnerNumber;
                        userProfile.StandardCode = channelData?.StandardCode;
                        userProfile.ApprenticeshipStartDate = channelData.ApprenticeshipStartDate;
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
                else
                {
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

        public override bool IsTriggered(DialogContext dc, ProgressState conversationProgress)
        {
            return conversationProgress != ProgressState.OptedOut
                   && conversationProgress != ProgressState.InProgress
                   && conversationProgress != ProgressState.BlackListed
                   && base.IsTriggered(dc, conversationProgress);
        }
    }
}