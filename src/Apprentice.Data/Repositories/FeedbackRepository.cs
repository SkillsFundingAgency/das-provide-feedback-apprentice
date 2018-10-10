namespace ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories
{
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;

    using Microsoft.Extensions.Options;

    using ApprenticeFeedbackDto = ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto.ApprenticeFeedback;
    using AzureOptions = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Azure;
    using DataOptions = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Data;

    public interface IFeedbackRepository
    {
        Task SaveFeedback(ApprenticeFeedbackDto feedback);
    }

    public class CosmosFeedbackRepository : IFeedbackRepository
    {
        private readonly DataOptions dataOptions;

        private readonly AzureOptions azureOptions;

        private readonly IDataRepository repo;

        public CosmosFeedbackRepository(IOptions<AzureOptions> azureOptions, IOptions<DataOptions> dataOptions)
        {
            this.dataOptions = dataOptions.Value;
            this.azureOptions = azureOptions.Value;
            this.repo = this.CreateRepository();
        }

        public async Task SaveFeedback(ApprenticeFeedbackDto feedback)
        {
            await this.repo.UpsertItemAsync(feedback);
        }

        private IDataRepository CreateRepository()
        {
            var cosmosEndpoint = this.azureOptions.CosmosEndpoint;
            var cosmosKey = this.azureOptions.CosmosKey;
            var databaseName = this.dataOptions.DatabaseName;
            var collection = this.dataOptions.FeedbackTable;

            return CosmosDbRepository
                .Instance
                .ConnectTo(cosmosEndpoint)
                .WithAuthKeyOrResourceToken(cosmosKey)
                .UsingDatabase(databaseName)
                .UsingCollection(collection);
        }
    }
}
