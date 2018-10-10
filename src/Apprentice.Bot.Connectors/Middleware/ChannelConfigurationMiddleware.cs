namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder;

    public class ChannelConfigurationMiddleware : IMiddleware
    {
        public async Task OnTurnAsync(
            ITurnContext turnContext,
            NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            dynamic channelData = turnContext.Activity.ChannelData;
            var supported = Enum.TryParse(turnContext.Activity.ChannelId, true, out BotChannel channelId);
            if (!supported)
            {
                channelId = BotChannel.Unsupported;
            }

            switch (channelId)
            {
                case BotChannel.Slack:
                    if (channelData?.SlackMessage != null)
                    {
                        // only reply in threads
                        if (channelData.SlackMessage.@event.thread_ts == null)
                        {
                            turnContext.Activity.Conversation.Id += $":{channelData.SlackMessage.@event.ts}";
                        }
                    }

                    break;

                case BotChannel.DirectLine:
                    if (channelData.NotifyMessage != null)
                    {
                        turnContext.Activity.Conversation.ConversationType = "personal";
                        turnContext.Activity.Conversation.IsGroup = false;
                    }

                    break;
                case BotChannel.Sms:
                    // The notify SMS channel doesn't actually use this channel type - it hangs off the DirectLine API (above)
                    break;

                case BotChannel.Emulator:
                    break;

                case BotChannel.Unsupported:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Pass execution on to the next layer in the pipeline.
            await next.Invoke(cancellationToken);
        }
    }
}