



namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Surveys
{
    using System.Collections.Generic;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;

    public class InMemoryApprenticeFeedbackSurveyV5 : ISurvey
    {
        private const string FinishKeepUpTheGoodWork = "That's the end of the survey for now. Keep up the good work!";

        private const string FinishSpeakToYourEmployer =
            "If you have a problem with your apprenticeship, it’s a good idea to speak to your employer’s ‘Human Resources’ staff";

        private const string FinishWeWillBeInTouch =
            "That's the end of the survey for now. We'll be in touch again later in the year.";

        private const string IntroJustThreeQuestions =
            "It's just 3 questions and it'll really help us improve apprenticeships.";

        private const string IntroOptOut =
            "Normal SMS charges apply. To stop receiving these messages, please type ‘Stop’";

        private const string IntroWelcome = "Here’s your apprenticeship survey from the Department for Education.";

        private const string QuestionsGettingSupport = "Next question, are you gettting the support you need?";

        private const string QuestionsHelpingWithJob = "Is your apprenticeship helping you with your job?";

        private const string QuestionsOverallSatisfaction = "Overall, are you satisfied with your apprenticeship?";

        private const string QuestionsPleaseTypeYesOrNo = "Please type 'Yes' or 'No'";

        private const string ResponsesNegative01 = "Okay, thanks for the feedback.";

        private const string ResponsesNegative02 = "Okay, thanks for that.";

        private const string ResponsesNegative03 =
            "Okay, thanks for letting us know. That's the end of the survey for now.";

        private const string ResponsesPositive01 = "Thanks!";

        private const string ResponsesPositive02 = "Thanks";

        private const string ResponsesPositive03 = "Great, thanks for your feedback. It’s really helpful";

        public InMemoryApprenticeFeedbackSurveyV5()
        {
            this.Id = "afb-v5";
            this.Steps = new List<ISurveyStep>()
                {
                    this.CreateStartStep(),
                    this.CreateQuestion1(),
                    this.CreateQuestion2(),
                    this.CreateQuestion3(),
                    this.CreateEndStep(),
                };
        }

        public InMemoryApprenticeFeedbackSurveyV5(string id)
        {
            this.Id = id;
            this.Steps = new List<ISurveyStep>()
                {
                    this.CreateStartStep(),
                    this.CreateQuestion1(),
                    this.CreateQuestion2(),
                    this.CreateQuestion3(),
                    this.CreateEndStep(),
                };
        }

        public string Id { get; set; }

        public ICollection<ISurveyStep> Steps { get; set; }

        public EndStep CreateEndStep()
        {
            var id = "feedback-end";
            var responses = new List<IResponse>
                {
                    new PredicateResponse
                        {
                            Id = nameof(FinishKeepUpTheGoodWork),
                            Predicate = u => u.Score >= 300,
                            Prompt = FinishKeepUpTheGoodWork,
                        },
                    new PredicateResponse
                        {
                            Id = nameof(FinishSpeakToYourEmployer),
                            Predicate = u => u.Score < 300 && u.Score >= 200,
                            Prompt = FinishWeWillBeInTouch,
                        },
                    new PredicateResponse
                        {
                            Id = nameof(FinishSpeakToYourEmployer),
                            Predicate = u => u.Score < 200,
                            Prompt = FinishSpeakToYourEmployer,
                        },
                };
            return new EndStep() { Id = id, Responses = responses };
        }

        public QuestionStep CreateQuestion1()
        {
            var id = "feedback-q1";
            var responses = new List<IResponse>
                {
                    new PositiveResponse { Prompt = ResponsesPositive01 },
                    new NegativeResponse { Prompt = ResponsesNegative01 },
                };
            var prompt = $"{QuestionsHelpingWithJob} \n {QuestionsPleaseTypeYesOrNo}";
            var score = 100;

            return new BinaryQuestion { Id = id, Responses = responses, Prompt = prompt, Score = score };
        }

        public QuestionStep CreateQuestion2()
        {
            var id = "feedback-q2";
            var responses = new List<IResponse>
                {
                    new PositiveResponse { Prompt = ResponsesPositive02 },
                    new NegativeResponse { Prompt = ResponsesNegative02 },
                };
            var prompt = QuestionsGettingSupport;
            var score = 100;

            return new BinaryQuestion { Id = id, Responses = responses, Prompt = prompt, Score = score };
        }

        public QuestionStep CreateQuestion3()
        {
            var id = "feedback-q3";
            var responses = new List<IResponse>
                {
                    new PositiveResponse { Prompt = ResponsesPositive03 },
                    new NegativeResponse { Prompt = ResponsesNegative03 },
                };
            var prompt = QuestionsOverallSatisfaction;
            var score = 100;

            return new BinaryQuestion { Id = id, Responses = responses, Prompt = prompt, Score = score };
        }

        public StartStep CreateStartStep()
        {
            var id = "feedback-start";
            var responses = new List<IResponse>
                {
                    new StaticResponse() { Id = nameof(IntroWelcome), Prompt = IntroWelcome, },
                    new StaticResponse()
                        {
                            Id = nameof(IntroJustThreeQuestions), Prompt = IntroJustThreeQuestions,
                        },
                    new StaticResponse() { Id = nameof(IntroOptOut), Prompt = IntroOptOut, },
                };
            return new StartStep() { Id = id, Responses = responses };
        }
    }
}