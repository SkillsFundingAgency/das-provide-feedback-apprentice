namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class FeedbackBot : IBot
    {
        private readonly ILogger<FeedbackBot> logger;

        private readonly IDialogFactory dialogFactory;

        private readonly IEnumerable<IBotDialogCommand> commands;

        private readonly IEnumerable<ISurvey> surveys;

        private readonly Features featureToggles;

        private readonly IStatePropertyAccessor<DialogState> dialogStateAccessor;
        private readonly IStatePropertyAccessor<UserProfile> userProfileAccessor;

        private readonly FeedbackBotState state;

        public FeedbackBot(FeedbackBotState state, ILoggerFactory loggerFactory, IEnumerable<IBotDialogCommand> commands)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            this.logger = loggerFactory.CreateLogger<FeedbackBot>();

            this.dialogStateAccessor = state.ConversationState.CreateProperty<DialogState>(nameof(DialogState));
            this.userProfileAccessor = state.ConversationState.CreateProperty<UserProfile>(nameof(UserProfile));

            //this.dialogFactory = dialogFactory ?? throw new ArgumentNullException(nameof(dialogFactory));

            this.state = state;
            this.commands = commands ?? throw new ArgumentNullException(nameof(commands));
            //this.surveys = surveys ?? throw new ArgumentNullException(nameof(surveys));

            // this.featureToggles = featureToggles.Value ?? throw new ArgumentNullException(nameof(featureToggles));

            this.Dialogs = this.BuildDialogs();
        }

        private DialogSet Dialogs { get; }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = new CancellationToken())
        {
            var activity = turnContext.Activity;

            // Create a dialog context
            var dc = await this.Dialogs.CreateContextAsync(turnContext, cancellationToken);

            try
            {
                BotChannel channel = this.ConfigureChannel(turnContext);

                switch (turnContext.Activity.Type)
                {
                    case ActivityTypes.Message:
                        await this.HandleMessageAsync(dc, cancellationToken);
                        break;

                    case ActivityTypes.ConversationUpdate:
                        await this.HandleConversationUpdateAsync(turnContext, channel, cancellationToken);
                        break;

                    default:
                        this.logger.LogInformation($"Encountered an unknown activity type of {turnContext.Activity.Type}");
                        break;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error while handling conversation response: {e.Message}", e);
            }
        }

        // TODO: add dynamic user-created dialogs from database 
        private DialogSet BuildDialogs()
        {
            DialogSet dialogs = new DialogSet(this.dialogStateAccessor);
            dialogs.Add(RootDialog.Instance);

            //foreach (ISurvey survey in this.surveys)
            //{
            //    LinearSurveyDialog dialog = this.dialogFactory.Create<LinearSurveyDialog>(survey);
            //    dialogs.Add(survey.Id, dialog);
            //}

            return dialogs;
        } 

        private BotChannel ConfigureChannel(ITurnContext context)
        {
            dynamic channelData = context.Activity.ChannelData;
            bool isSupported = Enum.TryParse(context.Activity.ChannelId, true, out BotChannel channelId);
            if (!isSupported)
            {
                return BotChannel.Unsupported;
            }

            switch (channelId)
            {
                case BotChannel.Slack:
                    if (channelData?.SlackMessage != null)
                    {
                        // only reply in threads
                        if (channelData.SlackMessage.@event.thread_ts == null)
                        {
                            context.Activity.Conversation.Id += $":{channelData.SlackMessage.@event.ts}";
                        }
                    }

                    break;

                case BotChannel.DirectLine:
                    if (channelData.NotifyMessage != null)
                    {
                        context.Activity.Conversation.ConversationType = "personal";
                        context.Activity.Conversation.IsGroup = false;
                    }

                    break;
            }

            return channelId;
        }

        /// <summary>
        /// TODO: Add to middleware intercepts
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task HandleCommandsAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            IBotDialogCommand command = this.commands.FirstOrDefault(c => c.IsTriggered(dc));
            if (command != null)
            {
                await command.ExecuteAsync(dc, cancellationToken);
            }
            else
            {
                var userProfile = await this.userProfileAccessor.GetAsync(dc.Context, cancellationToken: cancellationToken);

                if (userProfile.SurveyState.StartDate != default(DateTime))
                {
                    if (userProfile.SurveyState.StartDate <= DateTime.Now.AddDays(-7))
                    {
                        userProfile.SurveyState.Progress = ProgressState.Expired;
                    }
                }

                switch (userProfile.SurveyState.Progress)
                {
                    case ProgressState.NotStarted:
                        // Not sure how they got here, fix the session!
                        await dc.ContinueDialogAsync(cancellationToken);
                        break;

                    case ProgressState.InProgress:
                        // Continue as normal
                        await dc.ContinueDialogAsync(cancellationToken);
                        break;

                    case ProgressState.Complete:
                        // Survey already completed, so let them know
                        await dc.Context.SendActivityAsync($"Thanks for your interest, but it looks like you've already given us some feedback!", cancellationToken: cancellationToken);

                        await dc.CancelAllDialogsAsync(cancellationToken);
                        break;

                    case ProgressState.Expired:
                        // User took too long to reply
                        await dc.Context.SendActivityAsync($"Thanks for that - but I'm afraid you've missed the deadline this time.", cancellationToken: cancellationToken);
                        await dc.Context.SendActivityAsync($"I'll get in touch when it's time to give feedback again. Thanks for your help so far", cancellationToken: cancellationToken);

                        await dc.CancelAllDialogsAsync(cancellationToken);
                        break;

                    case ProgressState.OptedOut:
                        await dc.CancelAllDialogsAsync(cancellationToken);
                        break;

                    case ProgressState.BlackListed:
                        await dc.CancelAllDialogsAsync(cancellationToken);
                        break;

                    default:
                        await dc.ContinueDialogAsync(cancellationToken);
                        break;
                }
            }
        }
        
        private async Task HandleConversationUpdateAsync(ITurnContext context, BotChannel channel, CancellationToken cancellationToken)
        {
            foreach (ChannelAccount newMember in context.Activity.MembersAdded)
            {
                // Show welcome messages for those channels that support it
                if (channel == BotChannel.Slack || channel == BotChannel.Emulator)
                {
                    if (newMember.Id != context.Activity.Recipient.Id)
                    {
                        await context.SendActivityAsync(
                            $"Hello! I'm Bertie the Apprentice Feedback Bot. Please reply with 'help' if you would like to see a list of my capabilities",
                            cancellationToken: cancellationToken);
                    }
                }
            }
        }

        private async Task HandleMessageAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            await this.HandleCommandsAsync(dc, cancellationToken);

            if (!dc.Context.Responded)
            {
                await dc.BeginDialogAsync(RootDialog.Id, cancellationToken: cancellationToken);
            }
        }

    }
}