using System;
using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;
namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Services
{
    public static class ConversationExtensions
    {
        public static Conversation ToConversation(this BotConversation botConversation)
        {
            return new Conversation
            {
                Id = Guid.Parse(botConversation.ConversationId.Substring(0, 36)),
                UserId = Guid.Parse(botConversation.UserId),
                ActivityId = botConversation.ActivityId,
                TurnId = botConversation.TurnId
            };
        }
    }
}
