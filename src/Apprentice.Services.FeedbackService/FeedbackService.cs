namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService
{
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Feedback;
    using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands;

    using ApprenticeFeedbackDto = Data.Dto.ApprenticeFeedback;

    public class FeedbackService : IFeedbackService
    {
        private readonly ICommandHandlerAsync<SaveFeedbackCommand> saveFeedbackCommandHandler;

        public FeedbackService(ICommandHandlerAsync<SaveFeedbackCommand> saveFeedbackCommandHandler)
        {
            this.saveFeedbackCommandHandler = saveFeedbackCommandHandler;
        }

        public async Task SaveFeedbackAsync(ApprenticeFeedback feedback)
        {
            SaveFeedbackCommand command = new SaveFeedbackCommand()
                .StartedOn(feedback.StartTime)
                .CompletedOn(feedback.FinishTime)
                .SubmittedBy(feedback.Apprentice)
                .WithResponses(feedback.Responses)
                .ForApprenticeship(feedback.Apprenticeship);

            await this.saveFeedbackCommandHandler.HandleAsync(command);
        }

        public Task UpdateFeedbackAsync(ApprenticeFeedback feedback)
        {
            throw new System.NotImplementedException();
        }
    }
}
