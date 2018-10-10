namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.UnitTests.Stubs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Feedback;

    using ApprenticeFeedbackDto = ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto.ApprenticeFeedback;

    internal static class FakeFeedbackRepository
    {
        internal static Dictionary<string, ApprenticeFeedbackDto> Feedback { get; set; } = new Dictionary<string, ApprenticeFeedbackDto>()
            {
                {
                    "aaa-bbb-ccc-ddd",
                    new ApprenticeFeedbackDto()
                        {
                            StartTime = DateTime.UtcNow.AddDays(-30),
                            FinishTime = DateTime.UtcNow.AddDays(-30).AddMinutes(5),
                            Apprentice = CreateApprentice("ULN100000"),
                            Apprenticeship = CreateStandard("1000190", "STN-999"),
                            Responses = new List<ApprenticeResponse>
                                {
                                    CreateResponse(100, "Do you like cats?", "hell yeah!", "yes"),
                                    CreateResponse(
                                        100,
                                        "Do you like dogs?",
                                        "No, I'm more of a cat person",
                                        "no")
                                }
                        }
                }
            };

        internal static Apprentice CreateApprentice(string uln) => new Apprentice() { UniqueLearnerNumber = uln };

        internal static Framework CreateFramework(
            string ukprn,
            string frameworkId,
            string pathway,
            string programmeType) =>
            new Framework
                {
                    Provider = ukprn,
                    FrameworkId = frameworkId,
                    Pathway = pathway,
                    ProgrammeType = programmeType
                };

        internal static ApprenticeResponse CreateResponse(int score, string question, string answer, string intent) =>
            new ApprenticeResponse { Score = score, Question = question, Answer = answer, Intent = intent };

        internal static Standard CreateStandard(string ukprn, string standardId) =>
            new Standard { Provider = ukprn, StandardId = standardId };

        internal static Task SaveFeedback(ApprenticeFeedbackDto feedbackToSave)
        {
            return Task.Run(
                () =>
                    {
                        string key = Guid.NewGuid().ToString();
                        Feedback.Add(key, feedbackToSave);
                    });
        }
    }
}