namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;

    using ApprenticeFeedbackDto = ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto.ApprenticeFeedback;

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

        public Task HandleAsync(SaveFeedbackCommand command, CancellationToken cancellationToken = new CancellationToken())
        {
            if (this.repository == null)
            {
                throw new Exception("Repository is not configured.");
            }

            // TODO: Automapper
            var feedbackDto = new ApprenticeFeedbackDto()
            {
                Id = command.Feedback.Id.ToString(),
                StartTime = command.Feedback.StartTime,
                FinishTime = command.Feedback.FinishTime,
                SurveyId = command.Feedback.SurveyId,
                Apprentice = command.Feedback.Apprentice,
                Apprenticeship = command.Feedback.Apprenticeship,
                Responses = command.Feedback.Responses,
                PartitionKey = command.Feedback.Apprentice.ApprenticeId
            };

            return this.repository.SaveFeedback(feedbackDto);
        }
    }
}