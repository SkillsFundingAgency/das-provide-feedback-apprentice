using System;
using System.Dynamic;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Dto;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Helpers
{
    internal static class BotMessageTransformer
    {
        internal static BotConversationMessage TransformToBotMessage(dynamic incomingSms)
        {
            dynamic from = new ExpandoObject();
            from.id = incomingSms?.Value?.source_number;
            from.name = incomingSms?.Value?.source_number;
            from.role = null;

            dynamic channelData = new ExpandoObject();
            channelData.NotifyMessage = new NotifyMessage()
            {
                Id = incomingSms?.Value?.id,
                DateReceived = incomingSms?.Value?.date_received,
                DestinationNumber =
                                                     incomingSms?.Value?.destination_number,
                SourceNumber = incomingSms?.Value?.source_number,
                Message = incomingSms?.Value?.message,
                Type = "callback",
            };

            return new BotConversationMessage()
            {
                Type = "message",
                From = from,
                Text = incomingSms?.Value?.message,
                ChannelData = channelData
            };
        }
    }
}
