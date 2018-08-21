namespace ESFA.ProvideFeedback.Apprentice.Bot.Models
{
    using Microsoft.Extensions.Localization;

    public interface IApprenticeFeedbackResources
    {
        string FinishFormalComplaint { get; }

        string FinishKeepUpTheGoodWork { get; }

        string FinishSpeakToYourEmployer { get; }

        string IntroOptOut { get; }

        string IntroWelcome { get; }

        string QuestionsDaysOfTraining { get; }

        string QuestionsOverallSatisfaction { get; }

        string QuestionsTrainerKnowledge { get; }

        string ResponsesItsReallyHelpful { get; }

        string ResponsesNegative01 { get; }

        string ResponsesNegative02 { get; }

        string ResponsesPositive01 { get; }

        string ResponsesPositive02 { get; }

        string ResponsesSorryToHearThat { get; }
    }

    public class ApprenticeFeedbackResourcesV3 : IApprenticeFeedbackResources
    {
        private readonly IStringLocalizer<ApprenticeFeedbackResourcesV3> localizer;

        public ApprenticeFeedbackResourcesV3(IStringLocalizer<ApprenticeFeedbackResourcesV3> localizer) =>
            this.localizer = localizer;

        public string FinishFormalComplaint => this.GetString(nameof(this.FinishFormalComplaint));

        public string FinishKeepUpTheGoodWork => this.GetString(nameof(this.FinishKeepUpTheGoodWork));

        public string FinishSpeakToYourEmployer => this.GetString(nameof(this.FinishSpeakToYourEmployer));

        public string IntroOptOut => this.GetString(nameof(this.IntroOptOut));

        public string IntroWelcome => this.GetString(nameof(this.IntroWelcome));

        public string QuestionsDaysOfTraining => this.GetString(nameof(this.QuestionsDaysOfTraining));

        public string QuestionsOverallSatisfaction => this.GetString(nameof(this.QuestionsOverallSatisfaction));

        public string QuestionsTrainerKnowledge => this.GetString(nameof(this.QuestionsTrainerKnowledge));

        public string ResponsesItsReallyHelpful => this.GetString(nameof(this.ResponsesItsReallyHelpful));

        public string ResponsesNegative01 => this.GetString(nameof(this.ResponsesNegative01));

        public string ResponsesNegative02 => this.GetString(nameof(this.ResponsesNegative02));

        public string ResponsesPositive01 => this.GetString(nameof(this.ResponsesPositive01));

        public string ResponsesPositive02 => this.GetString(nameof(this.ResponsesPositive02));

        public string ResponsesSorryToHearThat => this.GetString(nameof(this.ResponsesSorryToHearThat));

        private string GetString(string name) => this.localizer[name];
    }

}