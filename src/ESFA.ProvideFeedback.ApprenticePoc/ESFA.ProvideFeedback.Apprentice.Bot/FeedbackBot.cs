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
using Microsoft.Bot.Builder.Azure;

namespace ESFA.ProvideFeedback.Apprentice.Bot
{
    public class FeedbackBot : IBot
    {
        private readonly DialogSet _dialogs;
        private readonly ILogger<FeedbackBot> _logger;

        public FeedbackBot(ILogger<FeedbackBot> logger, IApprenticeFeedbackSurvey feedbackDialogSet)
        {
            _logger = logger;
            _dialogs = feedbackDialogSet.Current();
        }

        /// <inheritdoc />
        public async Task OnTurn(ITurnContext context)
        {
            try
            {
                dynamic channelData = context.Activity.ChannelData;
                _logger.LogDebug($"channelData: {channelData}");

                if (channelData?.SlackMessage != null)
                {
                    // only reply in threads
                    if (channelData.SlackMessage.@event.thread_ts == null)
                    {
                        context.Activity.Conversation.Id += $":{channelData.SlackMessage.@event.ts}";
                    }
                }

                switch (context.Activity.Type)
                {
                    case ActivityTypes.Message:
                        var convoState = ConversationState<Dictionary<string, object>>.Get(context);
                        var dc = _dialogs.CreateContext(context, convoState);

                        if (context.Activity.Text.ToLowerInvariant().Contains("stop"))
                        {
                            await dc.Context.SendActivity($"Feedback cancelled");
                            dc.EndAll();
                        }
                        else
                        {
                            await dc.Continue();

                            if (!context.Responded)
                            {
                                if (context.Activity.Text.ToLowerInvariant().Contains("feedback"))
                                {
                                    await dc.Begin("start");
                                }
                            }
                        }

                        break;

                    case ActivityTypes.ConversationUpdate:
                        foreach (var newMember in context.Activity.MembersAdded)
                        {
                            if (newMember.Id != context.Activity.Recipient.Id)
                            {
                                await context.SendActivity($"Hello! I'm Bertie the Apprentice Feedback Bot. Please reply with 'help' if you would like to see a list of my capabilities");
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error while handling conversation response: {e.Message}", e);
            }
        }
    }
}
