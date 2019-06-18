namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Surveys
{
    using System.Collections.Generic;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;

    public class InMemoryApprenticeFeedbackSurveyV6 : ISurveyDefinition
    {
        private const string FinishKeepUpTheGoodWork = "That's the end of the survey for now. Keep up the good work!";

        private const string FinishSpeakToYourEmployer =
            "If you have a problem with your apprenticeship, it’s a good idea to speak to your employer’s ‘Human Resources’ staff";

        private const string FinishWeWillBeInTouch =
            "That's the end of the survey for now. We'll be in touch again later in the year.";

        private const string IntroJustFourQuestions =
            "It's just 4 questions and it'll really help us improve apprenticeships.";

        private const string IntroOptOut =
            "Normal SMS charges apply. To stop receiving these messages, please type ‘Stop’";

        private const string IntroWelcome = "Here’s your apprenticeship survey from the Department for Education.";

        private const string QuestionsGettingSupportFromTrainingProvider = "Next question, are you getting the support you need from your training provider?";

        private const string QuestionsGettingSupportFromEmployer = "Next question, are you getting the support you need from your employer?";

        private const string QuestionsHelpingWithJob = "Is your apprenticeship training helping you with your job?";

        private const string QuestionsOverallSatisfaction = "Overall, are you satisfied with your apprenticeship?";

        private const string QuestionsPleaseTypeYesOrNo = "Please type 'Yes' or 'No'";

        private const string ResponsesNegative01 = "Okay, thanks for the feedback.";

        private const string ResponsesNegative02 = "Okay, thanks for the feedback.";

        private const string ResponsesNegative03 = "Okay, thanks for that.";

        private const string ResponsesNegative04 = "Okay, thanks for letting us know. That's the end of the survey for now.";

        private const string ResponsesPositive01 = "Thanks!";

        private const string ResponsesPositive02 = "Thanks";

        private const string ResponsesPositive03 = "Thanks";

        private const string ResponsesPositive04 = "Great, thanks for your feedback. It’s really helpful";

        public InMemoryApprenticeFeedbackSurveyV6()
        {
            this.Id = "afb-v6";
            this.StepDefinitions = new List<ISurveyStepDefinition>()
                {
                    this.CreateStartStep(),
                    this.CreateQuestion1(),
                    this.CreateQuestion2(),
                    this.CreateQuestion3(),
                    this.CreateQuestion4(),
                    this.CreateEndStep(),
                };
        }

        public InMemoryApprenticeFeedbackSurveyV6(string id)
        {
            this.Id = id;
            this.StepDefinitions = new List<ISurveyStepDefinition>()
                {
                    this.CreateStartStep(),
                    this.CreateQuestion1(),
                    this.CreateQuestion2(),
                    this.CreateQuestion3(),
                    this.CreateQuestion4(),
                    this.CreateEndStep(),
                };
        }

        public string Id { get; set; }

        public ICollection<ISurveyStepDefinition> StepDefinitions { get; set; }

        public EndStepDefinition CreateEndStep()
        {
            var id = "feedback-end";
            var responses = new List<IBotResponse>
                {
                    new PredicateBotResponse
                        {
                            Id = nameof(FinishKeepUpTheGoodWork),
                            Predicate = u => u.Score >= 300,
                            Prompt = FinishKeepUpTheGoodWork,
                        },
                    new PredicateBotResponse
                        {
                            Id = nameof(FinishWeWillBeInTouch),
                            Predicate = u => u.Score < 300 && u.Score > 0,
                            Prompt = FinishWeWillBeInTouch,
                        },
                    new PredicateBotResponse
                        {
                            Id = nameof(FinishSpeakToYourEmployer),
                            Predicate = u => u.Score <= 0,
                            Prompt = FinishSpeakToYourEmployer,
                        },
                };
            return new EndStepDefinition() { Id = id, Responses = responses };
        }

        public QuestionStepDefinition CreateQuestion1()
        {
            var id = "feedback-q1";
            var responses = new List<IBotResponse>
                {
                    new PositiveBotResponse { Prompt = ResponsesPositive01 },
                    new NegativeBotResponse { Prompt = ResponsesNegative01 },
                };
            var prompt = $"{QuestionsHelpingWithJob}\n{QuestionsPleaseTypeYesOrNo}";
            var score = 100;

            return new BinaryQuestion { Id = id, Responses = responses, Prompt = prompt, Score = score };
        }

        public QuestionStepDefinition CreateQuestion2()
        {
            var id = "feedback-q2";
            var responses = new List<IBotResponse>
                {
                    new PositiveBotResponse { Prompt = ResponsesPositive02 },
                    new NegativeBotResponse { Prompt = ResponsesNegative02 },
                };
            var prompt = QuestionsGettingSupportFromTrainingProvider;
            var score = 100;

            return new BinaryQuestion { Id = id, Responses = responses, Prompt = prompt, Score = score };
        }

        public QuestionStepDefinition CreateQuestion3()
        {
            var id = "feedback-q3";
            var responses = new List<IBotResponse>
                {
                    new PositiveBotResponse { Prompt = ResponsesPositive03 },
                    new NegativeBotResponse { Prompt = ResponsesNegative03 },
                };
            var prompt = QuestionsGettingSupportFromEmployer;
            var score = 100;

            return new BinaryQuestion { Id = id, Responses = responses, Prompt = prompt, Score = score };
        }

        public QuestionStepDefinition CreateQuestion4()
        {
            var id = "feedback-q4";
            var responses = new List<IBotResponse>
                {
                    new PositiveBotResponse { Prompt = ResponsesPositive04 },
                    new NegativeBotResponse { Prompt = ResponsesNegative04 },
                };
            var prompt = QuestionsOverallSatisfaction;
            var score = 100;

            return new BinaryQuestion { Id = id, Responses = responses, Prompt = prompt, Score = score };
        }

        public StartStepDefinition CreateStartStep()
        {
            var id = "feedback-start";
            var responses = new List<IBotResponse>
                {
                    new StaticBotResponse() { Id = nameof(IntroWelcome), Prompt = IntroWelcome, },
                    new StaticBotResponse()
                        {
                            Id = nameof(IntroJustFourQuestions), Prompt = IntroJustFourQuestions,
                        },
                    new StaticBotResponse() { Id = nameof(IntroOptOut), Prompt = IntroOptOut, },
                };
            return new StartStepDefinition() { Id = id, Responses = responses };
        }
    }
}