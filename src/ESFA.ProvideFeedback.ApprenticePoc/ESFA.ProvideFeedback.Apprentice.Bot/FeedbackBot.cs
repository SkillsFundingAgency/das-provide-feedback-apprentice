// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FeedbackBot.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   Defines the FeedbackBot type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.ProvideFeedback.Apprentice.Bot
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.ProvideFeedback.Apprentice.Bot.Models;

    using Microsoft.Bot;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;

    using NLog.LayoutRenderers;

    /// <inheritdoc />
    /// <summary>
    /// A simple Bot Framework v4 bot for collecting feedback from Apprentices.
    /// </summary>
    public class FeedbackBot : IBot
    {
        /// <summary>
        /// The set of dialogs to be used for the bot conversation
        /// </summary>
        private readonly DialogSet dialogs;

        /// <summary>
        /// The logging service
        /// </summary>
        private readonly ILogger<FeedbackBot> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackBot"/> class.
        /// </summary>
        /// <param name="logger"> The log service. </param>
        /// <param name="feedbackDialogSet"> The set of dialogs to be used for the bot conversation </param>
        public FeedbackBot(ILogger<FeedbackBot> logger, IApprenticeFeedbackSurvey feedbackDialogSet)
        {
            this.logger = logger;
            this.dialogs = feedbackDialogSet.Dialogs;
        }

        /// <inheritdoc />
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
                        await HandleConversationUpdate(context, channel);
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

        /// <summary>
        ///     Handles updates to the conversation. Includes things like addition of new users, new channels, etc
        /// </summary>
        /// <param name="context"> The context object for this turn </param>
        /// <param name="channel"> The <see cref="BotChannel"/> </param>
        /// <returns>
        /// the <see cref="Task"/>
        /// </returns>
        private static async Task HandleConversationUpdate(ITurnContext context, BotChannel channel)
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

        /// <summary>
        /// Configures channel specific settings
        /// </summary>
        /// <param name="context"> The context object for this turn  </param>
        /// <returns> The <see cref="Task"/> with an enumerated value for the supported channel. </returns>
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
        ///     Handles messages sent to the conversation
        /// </summary>
        /// <param name="context"> The context object for this turn </param>
        /// <returns>
        /// the <see cref="Task"/>
        /// </returns>
        private async Task HandleMessage(ITurnContext context)
        {
            var conversationState = ConversationState<Dictionary<string, object>>.Get(context);
            var dc = this.dialogs.CreateContext(context, conversationState);

            if (context.Activity.Text.ToLowerInvariant().Contains("stop"))
            {
                await dc.Context.SendActivity($"Feedback canceled");
                dc.EndAll();
            }
            else
            {
                await dc.Continue();

                if (!context.Responded)
                {
                    if (context.Activity.Text.ToLowerInvariant().Equals("geronimo"))
                    {
                        await dc.Begin("start");
                    }
                }
            }
        }
    }
}