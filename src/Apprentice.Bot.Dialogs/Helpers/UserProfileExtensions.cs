using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Feedback;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
using System.Linq;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Helpers
{
    public static class UserProfileExtensions
    {
        public static ApprenticeFeedback ToApprenticeFeedback(this UserProfile userProfile)
        {
           return new ApprenticeFeedback
           {
               Id = userProfile.Id,
               Apprentice = new Core.Models.Feedback.Apprentice
               {

                   UniqueLearnerNumber = userProfile.IlrNumber,
                   ApprenticeId = userProfile.UserId,
               },
               Apprenticeship = new Apprenticeship
               {
                   StandardCode = userProfile.StandardCode.GetValueOrDefault(),
                   ApprenticeshipStartDate = userProfile.ApprenticeshipStartDate.GetValueOrDefault()
               },
               SurveyId = userProfile.SurveyState.SurveyId,
               StartTime = userProfile.SurveyState.StartDate,
               FinishTime = userProfile.SurveyState.EndDate.GetValueOrDefault(),
               Responses = userProfile.SurveyState.Responses.Select(ConvertToResponseData).ToList()
           };
        }

        private static ApprenticeResponse ConvertToResponseData(IQuestionResponse questionResponse) =>
            new ApprenticeResponse
            {
                Question = questionResponse.Question,
                Answer = questionResponse.Answer,
                Intent = questionResponse.Intent,
                Score = questionResponse.Score
            };
    }
}
