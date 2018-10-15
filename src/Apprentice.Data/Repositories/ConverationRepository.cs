using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;
using Microsoft.Extensions.Options;

using ApprenticeFeedbackDto = ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto.ApprenticeFeedback;
using AzureOptions = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Azure;
using DataOptions = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Data;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories
{
    public interface IConversationRepository
    {
        Task SaveConversation(ConversationLog conversationLog);
    }

    public class CosmosConversationRepository : IConversationRepository
    {
        private readonly DataOptions dataOptions;

        private readonly AzureOptions azureOptions;

        private readonly IDataRepository repo;

        public CosmosConversationRepository(IOptions<AzureOptions> azureOptions, IOptions<DataOptions> dataOptions)
        {
            this.dataOptions = dataOptions.Value;
            this.azureOptions = azureOptions.Value;
            this.repo = this.CreateRepository();
        }

        public async Task SaveConversation(ConversationLog conversationLog)
        {
            await this.repo.UpsertItemAsync(conversationLog);
        }

        private IDataRepository CreateRepository()
        {
            return CosmosDbRepository.Instance
                .ConnectTo(this.azureOptions.CosmosEndpoint)
                .WithAuthKeyOrResourceToken(this.azureOptions.CosmosKey)
                .UsingDatabase(this.dataOptions.DatabaseName)
                .UsingCollection(this.dataOptions.ConversationLogTable);
        }
    }
}
