namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Surveys
{
    using System.Collections.Generic;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;

    public class InMemoryApprenticeFeedbackSurvey : ISurvey
    {
        private const string FinishFormalComplaint =
            "If you’ve talked to them already, you might want to make a formal complaint: https://www.gov.uk/complainfurthereducationapprenticeship";

        private const string FinishKeepUpTheGoodWork = "Keep up the good work!";

        private const string FinishSpeakToYourEmployer =
            "If you have a problem with your apprenticeship, it’s a good idea to speak to your employer’s ‘Human Resources’ staff";

        private const string IntroOptOut =
            "It's just 3 questions and it'll really help us improve things. But if you want to opt out, please type ‘stop’";

        private const string IntroWelcome =
            "Here’s your quarterly apprenticeship survey. You agreed to participate when you started your apprenticeship";

        private const string QuestionsDaysOfTraining =
            "Over the last 6 months, have you received at least 25 days of training? Please type ‘yes’ or ‘no’";

        private const string QuestionsOverallSatisfaction = "Overall, are you satisfied with your apprenticeship?";

        private const string QuestionsTrainerKnowledge = "Next question, is your trainer good?";

        private const string ResponsesItsReallyHelpful = "Great, thanks for your feedback. It’s really helpful";

        private const string ResponsesNegative01 = "Okay, thanks for the feedback";

        private const string ResponsesNegative02 = "Okay, thanks for that";

        private const string ResponsesPositive01 = "Thanks!";

        private const string ResponsesPositive02 = "Thanks";

        private const string ResponsesSorryToHearThat = "Okay, sorry to hear that";

        public InMemoryApprenticeFeedbackSurvey()
        {
            this.Id = "afb-v3";
            this.Steps = new List<ISurveyStepDefinition>()
                {
                    this.CreateStartStep(),
                    this.CreateQuestionOne(),
                    this.CreateQuestionTwo(),
                    this.CreateQuestionThree(),
                    this.CreateEndStep()
                };
        }

        public InMemoryApprenticeFeedbackSurvey(string id)
        {
            this.Id = id;
            this.Steps = new List<ISurveyStepDefinition>()
                {
                    this.CreateStartStep(),
                    this.CreateQuestionOne(),
                    this.CreateQuestionTwo(),
                    this.CreateQuestionThree(),
                    this.CreateEndStep()
                };
        }

        public string Id { get; set; }

        public ICollection<ISurveyStepDefinition> Steps { get; set; }

        public EndStepDefinition CreateEndStep()
        {
            var id = "feedback-end";
            var responses = new List<IResponse>
                {
                    new PredicateResponse
                        {
                            Id = nameof(FinishSpeakToYourEmployer),
                            Predicate = u => u.Score < 300,
                            Prompt = FinishSpeakToYourEmployer,
                        },
                    new PredicateResponse
                        {
                            Id = nameof(FinishKeepUpTheGoodWork),
                            Predicate = u => u.Score >= 300,
                            Prompt = FinishKeepUpTheGoodWork,
                        },
                    new PredicateResponse
                        {
                            Id = nameof(FinishFormalComplaint),
                            Predicate = u => u.Score < 0,
                            Prompt = FinishFormalComplaint,
                        },
                };
            return new EndStepDefinition() { Id = id, Responses = responses };
        }

        public QuestionStepDefinition CreateQuestionOne()
        {
            var id = "feedback-q1";
            var responses = new List<IResponse>
                {
                    new PositiveResponse { Prompt = ResponsesPositive01 },
                    new NegativeResponse { Prompt = ResponsesNegative01 },
                };
            var prompt = QuestionsDaysOfTraining;
            var score = 100;

            return new BinaryQuestion { Id = id, Responses = responses, Prompt = prompt, Score = score };
        }

        public QuestionStepDefinition CreateQuestionThree()
        {
            var id = "feedback-q3";
            var responses = new List<IResponse>
                {
                    new PositiveResponse { Prompt = ResponsesItsReallyHelpful },
                    new NegativeResponse { Prompt = ResponsesSorryToHearThat },
                };
            var prompt = QuestionsOverallSatisfaction;
            var score = 100;

            return new BinaryQuestion { Id = id, Responses = responses, Prompt = prompt, Score = score };
        }

        public QuestionStepDefinition CreateQuestionTwo()
        {
            var id = "feedback-q2";
            var responses = new List<IResponse>
                {
                    new PositiveResponse { Prompt = ResponsesPositive02 },
                    new NegativeResponse { Prompt = ResponsesNegative02 },
                };
            var prompt = QuestionsTrainerKnowledge;
            var score = 100;

            return new BinaryQuestion { Id = id, Responses = responses, Prompt = prompt, Score = score };
        }

        public StartStepDefinition CreateStartStep()
        {
            var id = "feedback-start";
            var responses = new List<IResponse>
                {
                    new StaticResponse() { Id = nameof(IntroWelcome), Prompt = IntroWelcome },
                    new StaticResponse() { Id = nameof(IntroOptOut), Prompt = IntroOptOut },
                };
            return new StartStepDefinition() { Id = id, Responses = responses };
        }
    }
}