using System;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;
namespace ESFA.DAS.ProvideFeedback.Apprentice.Services
{
    public static class ConversationExtensions
    {
        public static Conversation ToConversation(this BotConversation botConversation)
        {
            return new Conversation
            {
                Id = botConversation.ConversationId,
                UserId = Guid.Parse(botConversation.UserId),
                ActivityId = botConversation.ActivityId,
                TurnId = botConversation.TurnId
            };
        }
    }
}
