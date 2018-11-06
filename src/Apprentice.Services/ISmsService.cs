namespace ESFA.DAS.ProvideFeedback.Apprentice.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;

    public interface ISmsService
    {
        Task SendSmsAsync(string destinationNumber, string content, string reference = null);
    }

    public interface IConversationLogService
    {
        Task WriteLogAsync(ConversationLog log);
    }

    public interface IBotService
    {
        Task CreateNewSession(BotSession session);
    }
}
