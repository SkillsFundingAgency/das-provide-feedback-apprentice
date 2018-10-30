namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Commands.Dialog;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Root;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Survey;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class FeedbackBot : IBot
    {
        private readonly IEnumerable<IBotDialogCommand> commands;

        private readonly IEnumerable<ISurvey> surveys;

        private readonly IDialogFactory dialogFactory;

        private readonly Features featureToggles;

        private readonly ILogger<FeedbackBot> logger;

        private readonly FeedbackBotStateRepository stateRepository;

        public FeedbackBot(
            FeedbackBotStateRepository stateRepository,
            ILoggerFactory loggerFactory,
            IEnumerable<IBotDialogCommand> commands,
            IEnumerable<ISurvey> surveys,
            IDialogFactory dialogFactory,
            IOptions<Features> featureToggles)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            this.logger = loggerFactory.CreateLogger<FeedbackBot>();

            this.dialogFactory = dialogFactory ?? throw new ArgumentNullException(nameof(dialogFactory));
            this.stateRepository = stateRepository ?? throw new ArgumentNullException(nameof(stateRepository));
            this.commands = commands ?? throw new ArgumentNullException(nameof(commands));
            this.surveys = surveys ?? throw new ArgumentNullException(nameof(surveys));
            this.featureToggles = featureToggles.Value ?? throw new ArgumentNullException(nameof(featureToggles));

            this.Dialogs = this.BuildDialogs();
        }

        private DialogSet Dialogs { get; }

        /// <inheritdoc />
        /// <summary>
        /// This is where the most of the magic happens. Every time the bot receives a message, this method is executed and the message processed accordingly.
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task OnTurnAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Create a dialog context
            DialogContext dc = await this.Dialogs.CreateContextAsync(turnContext, cancellationToken);

            try
            {
                switch (turnContext.Activity.Type)
                {
                    case ActivityTypes.Message:
                        await this.HandleMessageAsync(dc, cancellationToken);
                        break;

                    case ActivityTypes.ConversationUpdate:
                        await this.HandleConversationUpdateAsync(turnContext, cancellationToken);
                        break;

                    default:
                        this.logger.LogInformation(
                            $"Encountered an unknown activity type of {turnContext.Activity.Type}");
                        break;
                }

                await this.stateRepository.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                await this.stateRepository.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
            }
            catch (Exception e)
            {
                throw new Exception($"Error while handling conversation response: {e.Message}", e);
            }
        }

        // TODO: add dynamic user-created dialogs from database
        private DialogSet BuildDialogs()
        {
            DialogSet dialogs = new DialogSet(this.stateRepository.ConversationDialogState);
            dialogs.Add(RootDialog.Instance);

            foreach (var survey in this.surveys)
            {
                var dialog = this.dialogFactory.Create<SurveyDialog>(survey);
                dialogs.Add(dialog);
            }

            return dialogs;
        }

        /// <summary>
        /// TODO: Add to middleware intercepts
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task HandleCommandsAsync(DialogContext dialog, CancellationToken cancellationToken)
        {
            var userProfile = await this.stateRepository.UserProfile.GetAsync(dialog.Context, () => new UserProfile(), cancellationToken);
            IBotDialogCommand command = this.commands.FirstOrDefault(c => c.IsTriggered(dialog, userProfile.SurveyState.Progress));
            if (command != null)
            {
                await command.ExecuteAsync(dialog, cancellationToken);
            }
            else
            {
                if (userProfile.SurveyState.StartDate != default(DateTime))
                {
                    if (userProfile.SurveyState.StartDate <= DateTime.Now.AddDays(-7))
                    {
                        userProfile.SurveyState.Progress = ProgressState.Expired;
                    }
                }

                await ContinueConversation(dialog, userProfile, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task HandleConversationUpdateAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken)
        {
            var supported = Enum.TryParse(turnContext.Activity.ChannelId, true, out BotChannel channelId);

            foreach (ChannelAccount newMember in turnContext.Activity.MembersAdded)
            {
                // Show welcome messages for those channels that support it
                if (channelId == BotChannel.Slack || channelId == BotChannel.Emulator)
                {
                    if (newMember.Id != turnContext.Activity.Recipient.Id)
                    {
                        await turnContext.SendActivityAsync(
                            $"Hello! I'm Bertie the Apprentice Feedback Bot. Please reply with 'help' if you would like to see a list of my capabilities",
                            cancellationToken: cancellationToken);
                    }
                }
            }
        }

        private async Task HandleMessageAsync(DialogContext dialog, CancellationToken cancellationToken)
        {
            await this.HandleCommandsAsync(dialog, cancellationToken);

            if (!dialog.Context.Responded)
            {
                await dialog.BeginDialogAsync(nameof(RootDialog), cancellationToken: cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> ContinueConversation(DialogContext dialog, UserProfile userProfile, CancellationToken cancellationToken)
        {
            var reply = dialog.Context.Activity.CreateReply();

            switch (userProfile.SurveyState.Progress)
            {
                case ProgressState.NotStarted:
                    // Not sure how they got here, fix the session!
                    return await dialog.ContinueDialogAsync(cancellationToken);

                case ProgressState.InProgress:
                    // Continue as normal
                    return await dialog.ContinueDialogAsync(cancellationToken);

                case ProgressState.Complete:
                    // Survey already completed, so let them know
                    reply.Text = $"Thanks for your interest, but it looks like you've already given us some feedback!";

                    await dialog.Context.SendActivityAsync(reply, cancellationToken);
                    return await dialog.CancelAllDialogsAsync(cancellationToken);

                case ProgressState.Expired:
                    // Survey already completed, so let them know
                    reply.Text = $"Thanks for that - but I'm afraid you've missed the deadline this time."
                                 + $"\n"
                                 + $"I'll get in touch when it's time to give feedback again. Thanks for your help so far";

                    await dialog.Context.SendActivityAsync(reply, cancellationToken);
                    return await dialog.CancelAllDialogsAsync(cancellationToken);

                case ProgressState.OptedOut:
                    reply.Text = "You have opted out of surveys.";

                    await dialog.Context.SendActivityAsync(reply, cancellationToken);
                    return await dialog.CancelAllDialogsAsync(cancellationToken);

                case ProgressState.BlackListed:
                    // Survey user blacklisted. Let them know
                    reply.Text = $"You'll get another chance to leave feedback in about 3 months. Thanks and goodbye!";

                    await dialog.Context.SendActivityAsync(reply, cancellationToken);
                    return await dialog.CancelAllDialogsAsync(cancellationToken);

                default:
                    return await dialog.ContinueDialogAsync(cancellationToken);
            }
        }
    }
}