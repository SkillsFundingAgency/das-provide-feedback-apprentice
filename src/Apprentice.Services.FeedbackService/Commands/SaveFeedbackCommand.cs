namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Feedback;
    using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;

    using ApprenticeFeedbackDto = ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto.ApprenticeFeedback;

    public class SaveFeedbackCommand : ICommandAsync
    {
        private Apprentice Apprentice { get; set; }

        private Apprenticeship Apprenticeship { get; set; }

        private DateTime FinishTime { get; set; }

        private IFeedbackRepository Repository { get; set; }

        private List<ApprenticeResponse> Responses { get; set; }

        private DateTime StartTime { get; set; }

        public SaveFeedbackCommand CompletedOn(DateTime time)
        {
            this.FinishTime = time;
            return this;
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (this.Repository == null)
            {
                throw new Exception("Repository is not configured.");
            }

            var feedbackDto = new ApprenticeFeedbackDto()
            {
                StartTime = this.StartTime,
                FinishTime = this.FinishTime,
                Apprentice = this.Apprentice,
                Apprenticeship = this.Apprenticeship,
                Responses = this.Responses
            };

            await this.Repository.SaveFeedback(feedbackDto);
        }

        public SaveFeedbackCommand ForApprenticeship(Apprenticeship apprenticeship)
        {
            this.Apprenticeship = apprenticeship;
            return this;
        }

        public SaveFeedbackCommand StartedOn(DateTime time)
        {
            this.StartTime = time;
            return this;
        }

        public SaveFeedbackCommand SubmittedBy(Apprentice apprentice)
        {
            this.Apprentice = apprentice;
            return this;
        }

        public SaveFeedbackCommand Using(IFeedbackRepository repo)
        {
            this.Repository = repo;
            return this;
        }

        public SaveFeedbackCommand WithResponses(List<ApprenticeResponse> responses)
        {
            this.Responses = responses;
            return this;
        }
    }
}