namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.State;

    using Microsoft.Bot;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;

    public class FeedbackBot : IBot
    {
        private readonly ILogger<FeedbackBot> logger;

        public FeedbackBot(ILogger<FeedbackBot> logger)
        {
            this.logger = logger;
        }

        private DialogSet Dialogs { get; } = BuildDialogs();

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

        private static DialogSet BuildDialogs()
        {
            DialogSet dialogs = new DialogSet();

            dialogs.Add(RootDialog.Id, RootDialog.Instance);
            dialogs.Add(SurveyRunner.Id, SurveyRunner.Instance);
            dialogs.Add(ApprenticeFeedbackV3.Id, ApprenticeFeedbackV3.Instance);
            dialogs.Add(ApprenticeFeedbackV4.Id, ApprenticeFeedbackV4.Instance);
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

        private async Task HandleCommands(ITurnContext context, string message)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(context);
            ConversationInfo conversationInfo = ConversationState<ConversationInfo>.Get(context);

            DialogContext dc = this.Dialogs.CreateContext(context, conversationInfo);

            var strings = message.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            var command = strings.First();

            switch (command)
            {
                case "stop":
                    {
                        // TODO: Create an IBotCommandHandler implementation to deal with this
                        await dc.Context.SendActivity($"Feedback canceled");
                        dc.EndAll();
                        break;
                    }

                case "status":
                    {
                        // TODO: Create an IBotCommandHandler implementation to deal with this
                        await dc.Context.SendActivity($"{JsonConvert.SerializeObject(userInfo)}");
                        break;
                    }

                case "reset":
                    {
                        // TODO: Create an IBotCommandHandler implementation to deal with this
                        userInfo.ApprenticeFeedback = new ApprenticeFeedback();
                        await dc.Context.SendActivity($"OK. Starting again!");
                        await dc.Begin(RootDialog.Id);
                        break;
                    }

                case "expire":
                    {
                        // TODO: Create an IBotCommandHandler implementation to deal with this
                        if (userInfo.ApprenticeFeedback.StartDate != default(DateTime))
                        {
                            userInfo.ApprenticeFeedback.StartDate = userInfo.ApprenticeFeedback.StartDate.AddDays(-30);
                        }
                        break;
                    }

                case "start":
                    {
                        // TODO: Create an IBotCommandHandler implementation to deal with this
                        try
                        {
                            userInfo.ApprenticeFeedback = new ApprenticeFeedback();

                            if (strings.Length > 1)
                            {
                                string dialogId = strings[1];
                                await dc.Begin(dialogId);
                            }
                            else
                            {
                                await dc.Begin(SurveyRunner.Id);
                            }
                        }
                        catch (Exception e)
                        {
                            this.logger.LogError(e.Message);
#if DEBUG
                            await dc.Context.SendActivity($"DEBUG: {e.ToString()}");
#endif
                        }

                        break;
                    }

                case "help":
                    {
                        var menu = new List<string> { "status", "reset", "start" };
                        await dc.Context.SendActivity(MessageFactory.SuggestedActions(menu, "How can I help you?"));
                        break;
                    }

                default:
                    {
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

            string message = context.Activity.Text.ToLowerInvariant();

            await this.HandleCommands(context, message);

            if (userInfo.ApprenticeFeedback.StartDate != default(DateTime))
            await this.CheckForCompleteFlag(userInfo, dc);
            await this.CheckForExpiry(userInfo, dc);

            if (!context.Responded)
            {
                await this.StartFeedback(userInfo, dc);
            }
        }

        private async Task CheckForCompleteFlag(UserInfo userInfo, DialogContext dc)
        {
            if (userInfo.ApprenticeFeedback.Progress == ProgressState.Complete)
            {
                await dc.Context.SendActivity(
                    $"Thanks for your interest, but it looks like you've already given us some feedback!");

                dc.EndAll();
            }
        }

        private async Task CheckForExpiry(UserInfo userInfo, DialogContext dc)
        {
            if (userInfo.ApprenticeFeedback.Progress == ProgressState.Expired || userInfo.ApprenticeFeedback.StartDate < DateTime.Now.AddDays(-7)) // TODO: add to config
            {
                await dc.Context.SendActivity(
                    $"Thanks for that - but I'm afraid you've missed the deadline this time. \n "
                    + $"I'll get in touch when it's time to give feedback again. Thanks for your help so far ");

                userInfo.ApprenticeFeedback.Progress = ProgressState.Expired;

                dc.EndAll();
            }
        }

        private async Task StartFeedback(UserInfo userInfo, DialogContext dc)
        {
            await dc.Begin(RootDialog.Id);
        }
    }
}