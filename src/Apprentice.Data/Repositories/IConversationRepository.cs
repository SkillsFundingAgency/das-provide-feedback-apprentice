using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;
using System;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories
{
    public interface IConversationRepository
    {
        Task<Conversation> Get(Guid id);
        Task Save(Conversation conversation);
    }
}
