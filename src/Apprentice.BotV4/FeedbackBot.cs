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
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Newtonsoft.Json;

    using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;

    public class FeedbackBot : IBot
    {
        private readonly BotSettings botSettings;

        private readonly IEnumerable<IBotDialogCommand> commands;

        private readonly IDialogFactory dialogFactory;

        private readonly Features featureToggles;

        private readonly ILogger<FeedbackBot> logger;

        private readonly IFeedbackBotStateRepository stateRepository;

        private readonly IEnumerable<ISurveyDefinition> surveys;

        public FeedbackBot(
            IFeedbackBotStateRepository stateRepository,
            ILoggerFactory loggerFactory,
            IEnumerable<IBotDialogCommand> commands,
            IEnumerable<ISurveyDefinition> surveys,
            IDialogFactory dialogFactory,
            IOptions<Features> featureToggles,
            IOptions<BotSettings> botSettings)
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
            this.botSettings = botSettings.Value ?? throw new ArgumentNullException(nameof(botSettings));

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
            catch (JsonSerializationException e)
            {
                this.logger.LogCritical("Encountered a bad session state object, could not rebuild from JSON.", new { Exception = e });

                await this.stateRepository.ConversationState.ClearStateAsync(dc.Context, cancellationToken);
                await this.stateRepository.ConversationState.LoadAsync(dc.Context, true, cancellationToken);

                await this.stateRepository.UserState.ClearStateAsync(dc.Context, cancellationToken);
                await this.stateRepository.UserState.LoadAsync(dc.Context, true, cancellationToken);
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

        private async Task<DialogTurnResult> ContinueConversationAsync(
            DialogContext dialog,
            UserProfile userProfile,
            CancellationToken cancellationToken)
        {
            await this.EnsureNotExpired(dialog, cancellationToken, userProfile);

            switch (userProfile.SurveyState.Progress)
            {
                case ProgressState.NotStarted:
                    // Not sure how they got here, fix the session!
                    return await dialog.ContinueDialogAsync(cancellationToken);

                case ProgressState.InProgress:
                    // Continue as normal
                    return await dialog.ContinueDialogAsync(cancellationToken);

                case ProgressState.Expired:
                    return new DialogTurnResult(DialogTurnStatus.Waiting);

                case ProgressState.Complete:
                    return await dialog.EndDialogAsync(cancellationToken);
                case ProgressState.BlackListed:
                case ProgressState.OptedOut:
                    return await dialog.CancelAllDialogsAsync(cancellationToken);

                default:
                    return await dialog.ContinueDialogAsync(cancellationToken);
            }
        }

        /// <summary>
        /// TODO: Add to middleware intercepts
        /// </summary>
        /// <param name="dialog">
        /// the currently active dialog context
        /// </param>
        /// <param name="cancellationToken">
        /// the cancellation token
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task HandleCommandsAsync(
            DialogContext dialog,
            CancellationToken cancellationToken)
        {
            var userProfile = await this.stateRepository.UserProfile.GetAsync(dialog.Context, () => new UserProfile(), cancellationToken);
            IBotDialogCommand command = this.commands.FirstOrDefault(c => c.IsTriggered(dialog, userProfile.SurveyState.Progress));
            if (command != null)
            {
                await command.ExecuteAsync(dialog, cancellationToken);
            }
            else
            {
                await this.ContinueConversationAsync(dialog, userProfile, cancellationToken).ConfigureAwait(false);
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

            // TODO: swap out for channel ?
            if (!dialog.Context.Responded && dialog.Context.Activity.ChannelId == "emulator")
            {
                await dialog.BeginDialogAsync(nameof(RootDialog), cancellationToken: cancellationToken);
            }
        }

        private async Task EnsureNotExpired(DialogContext dialog, CancellationToken cancellationToken, UserProfile userProfile)
        {
            var reply = dialog.Context.Activity.CreateReply();

            // Check for conversation expiry
            if (userProfile.SurveyState.StartDate != default(DateTime))
            {
                if (userProfile.SurveyState.StartDate <= DateTime.Now.AddMinutes(-this.botSettings.ConversationExpiryHours)
                    && (userProfile.SurveyState.Progress == ProgressState.InProgress || userProfile.SurveyState.Progress == ProgressState.NotStarted))
                {
                    reply.Text = $"Thanks for that - but I'm afraid you've missed the deadline this time."
                        + $"\n"
                        + $"I'll get in touch when it's time to give feedback again. Thanks for your help so far";

                    dialog.Context.Activity.Id = "Expired";
                    await dialog.Context.SendActivityAsync(reply, cancellationToken);
                    userProfile.SurveyState.Progress = ProgressState.Expired;
                }
            }

            // Check for spam
            // TODO: set Blacklist flag here
        }
    }
}