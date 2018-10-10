namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands
{
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;

    public class SaveFeedbackCommandHandler : ICommandHandlerAsync<SaveFeedbackCommand>
    {
        private readonly IFeedbackRepository repository;

        public SaveFeedbackCommandHandler(IFeedbackRepository repository)
        {
            this.repository = repository;
        }

        public void Handle(SaveFeedbackCommand command)
        {
            throw new System.NotImplementedException();
        }

        public async Task HandleAsync(SaveFeedbackCommand command, CancellationToken cancellationToken = new CancellationToken())
        {
            await command
                .Using(this.repository)
                .ExecuteAsync(cancellationToken);
        }
    }
}