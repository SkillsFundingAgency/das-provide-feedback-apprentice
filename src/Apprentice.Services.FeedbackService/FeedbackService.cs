namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService
{
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Feedback;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands;

    using ApprenticeFeedbackDto = ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto.ApprenticeFeedback;

    public class FeedbackService : IFeedbackService
    {
        private readonly ICommandHandlerAsync<SaveFeedbackCommand> saveFeedbackCommandHandler;

        public FeedbackService(ICommandHandlerAsync<SaveFeedbackCommand> saveFeedbackCommandHandler)
        {
            this.saveFeedbackCommandHandler = saveFeedbackCommandHandler;
        }

        public Task SaveFeedbackAsync(ApprenticeFeedback feedback)
        {
            SaveFeedbackCommand command = new SaveFeedbackCommand()
            {
                Feedback = feedback
            };

            return this.saveFeedbackCommandHandler.HandleAsync(command);
        }

        public Task UpdateFeedbackAsync(ApprenticeFeedback feedback)
        {
            throw new System.NotImplementedException();
        }
    }
}