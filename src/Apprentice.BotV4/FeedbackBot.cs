using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;

    using Microsoft.Bot;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;

    public class FeedbackBot : IBot
    {
        private readonly IEnumerable<IBotDialogCommand> commands;

        private readonly ILogger<FeedbackBot> logger;

        private readonly IDialogFactory dialogFactory;

        private readonly List<ISurvey> surveys = new List<ISurvey>();

        public FeedbackBot(ILogger<FeedbackBot> logger, IEnumerable<IBotDialogCommand> commands)
        {
            this.logger = logger;
            this.commands = commands;

            this.dialogFactory = new DialogFactory(); // TODO: Inject
            this.surveys.Add(new InMemoryApprenticeFeedbackSurvey()); // TODO: List resolve

            this.Dialogs = this.BuildDialogs();
        }

        private DialogSet Dialogs { get; }

        public async Task OnTurn(ITurnContext context)
        {
            try
            {
                BotChannel channel = await Task.Run(() => this.ConfigureChannel(context));

                switch (context.Activity.Type)
                {
                    case ActivityTypes.Message:
                        await this.HandleMessage(context);
                        break;

                    case ActivityTypes.ConversationUpdate:
                        await this.HandleConversationUpdate(context, channel);
                        break;

                    default:
                        this.logger.LogInformation($"Encountered an unknown activity type of {context.Activity.Type}");
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
            DialogSet dialogs = new DialogSet();

            dialogs.Add(RootDialog.Id, RootDialog.Instance);

            foreach (ISurvey survey in this.surveys)
            {
                LinearSurveyDialog dialog = this.dialogFactory.Create<LinearSurveyDialog>(survey);
                dialogs.Add(survey.Id, dialog);
            }

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

        private async Task HandleCommands(ITurnContext context)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(context);
            ConversationInfo conversationInfo = ConversationState<ConversationInfo>.Get(context);

            DialogContext dc = this.Dialogs.CreateContext(context, conversationInfo);

            IBotDialogCommand command = this.commands.FirstOrDefault(c => c.IsTriggered(dc));
            if (command != null)
            {
                await command.ExecuteAsync(dc);
            }
            else
            {
                if (userInfo.SurveyState.StartDate != default(DateTime))
                {
                    if (userInfo.SurveyState.StartDate <= DateTime.Now.AddDays(-7))
                    {
                        userInfo.SurveyState.Progress = ProgressState.Expired;
                    }
                }

                switch (userInfo.SurveyState.Progress)
                {
                    case ProgressState.NotStarted:
                        // Not sure how they got here, fix the session!
                        break;

                    case ProgressState.InProgress:
                        // Continue as normal
                        await dc.Continue();
                        break;

                    case ProgressState.Complete:
                        // Survey already completed, so let them know
                        await dc.Context.SendActivity($"Thanks for your interest, but it looks like you've already given us some feedback!");

                        dc.EndAll();
                        break;

                    case ProgressState.Expired:
                        // User took too long to reply
                        await dc.Context.SendActivity($"Thanks for that - but I'm afraid you've missed the deadline this time.");
                        await dc.Context.SendActivity($"I'll get in touch when it's time to give feedback again. Thanks for your help so far");

                        dc.EndAll();
                        break;

                    case ProgressState.OptedOut:
                        dc.EndAll();
                        break;

                    case ProgressState.BlackListed:
                        dc.EndAll();
                        break;

                    default:
                        await dc.Continue();
                        break;
                }
            }
        }
        
        private async Task HandleConversationUpdate(ITurnContext context, BotChannel channel)
        {
            foreach (ChannelAccount newMember in context.Activity.MembersAdded)
            {
                // Show welcome messages for those channels that support it
                if (channel == BotChannel.Slack || channel == BotChannel.Emulator)
                {
                    if (newMember.Id != context.Activity.Recipient.Id)
                    {
                        await context.SendActivity(
                            $"Hello! I'm Bertie the Apprentice Feedback Bot. Please reply with 'help' if you would like to see a list of my capabilities");
                    }
                }
            }
        }

        private async Task HandleMessage(ITurnContext context)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(context);
            ConversationInfo conversationInfo = ConversationState<ConversationInfo>.Get(context);

            DialogContext dc = this.Dialogs.CreateContext(context, conversationInfo);

            await this.HandleCommands(context);

            if (!context.Responded)
            {
                await dc.Begin(RootDialog.Id);
            }
        }
    }
}