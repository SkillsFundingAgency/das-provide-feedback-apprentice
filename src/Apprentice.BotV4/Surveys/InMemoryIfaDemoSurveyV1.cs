namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Surveys
{
    using System.Collections.Generic;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;

    public class InMemoryIfaDemoSurveyV1 : ISurvey
    {
        private const string Intro = "Here's a new survey from the Institute for Apprenticeships - they make sure apprenticeships teach people the right skills for their job." +
            "It's just 3 questions and it'll really help to improve Data Analyst apprenticeships. " +
            "This isn't a test - there are no right or wrong answers :)";

        private const string IntroOptOut =
            "Normal SMS charges apply.To stop receiving these messages, please type ‘Stop’";

        private const string Question1 = "Is your apprenticeship helping you to do your job?";

        private const string Question2 = "Does your experience of your apprenticeship match the standard?";

        private const string Question3 = "Should anything be added to the apprenticeship standard?";

        private const string Question3b = "Can you please explain this in more detail?";

        private const string Question4 = "Any other thoughts on your apprenticeship.";

        private const string QuestionsPleaseTypeYesOrNo = "Please type 'Yes' or 'No'";

        private const string ResponsesNegative01 = ":(";

        private const string ResponsesPositive01 = ":)";

        public InMemoryIfaDemoSurveyV1()
        {
            this.Id = "ifa-v1";
            this.Steps = new List<ISurveyStep>()
                {
                    this.CreateStartStep(),
                    this.CreateQuestion1(),
                    this.CreateQuestion2(),
                    this.CreateQuestion3(),
                    this.CreateQuestion3b(),
                };
        }

        public InMemoryIfaDemoSurveyV1(string id)
        {
            this.Id = id;
            this.Steps = new List<ISurveyStep>()
                {
                    this.CreateStartStep(),
                    this.CreateQuestion1(),
                    this.CreateQuestion2(),
                    this.CreateQuestion3(),
                    this.CreateQuestion3b(),
                };
        }

        public string Id { get; set; }

        public ICollection<ISurveyStep> Steps { get; set; }

        //public EndStep CreateEndStep()
        //{
        //    var id = "feedback-end";
        //    var responses = new List<IResponse>
        //        {
        //            new PredicateResponse
        //                {
        //                    Id = nameof(FinishKeepUpTheGoodWork),
        //                    Predicate = u => u.Score >= 300,
        //                    Prompt = FinishKeepUpTheGoodWork,
        //                },
        //            new PredicateResponse
        //                {
        //                    Id = nameof(FinishSpeakToYourEmployer),
        //                    Predicate = u => u.Score < 300 && u.Score >= 200,
        //                    Prompt = FinishWeWillBeInTouch,
        //                },
        //            new PredicateResponse
        //                {
        //                    Id = nameof(FinishSpeakToYourEmployer),
        //                    Predicate = u => u.Score < 200,
        //                    Prompt = FinishSpeakToYourEmployer,
        //                },
        //        };
        //    return new EndStep() { Id = id, Responses = responses };
        //}

        public QuestionStep CreateQuestion1()
        {
            var id = "feedback-q1";
            var responses = new List<IResponse>
                {
                    new PositiveResponse { Prompt = ResponsesPositive01 },
                    new NegativeResponse { Prompt = ResponsesNegative01 },
                };
            var prompt = $"{Question1} \n {QuestionsPleaseTypeYesOrNo}";
            var score = 100;

            return new BinaryQuestion { Id = id, Responses = responses, Prompt = prompt, Score = score };
        }

        public QuestionStep CreateQuestion2()
        {
            var id = "feedback-q2";
            var responses = new List<IResponse>
                {
                    new PositiveResponse { Prompt = ResponsesPositive01 },
                    new NegativeResponse { Prompt = ResponsesNegative01 },
                };
            var prompt = Question2;
            var score = 100;

            return new BinaryQuestion { Id = id, Responses = responses, Prompt = prompt, Score = score };
        }

        public QuestionStep CreateQuestion3()
        {
            var id = "feedback-q3";
            var responses = new List<IResponse>
                {
                    new PositiveResponse { Prompt = ResponsesPositive01 },
                    new NegativeResponse { Prompt = ResponsesNegative01 },
                };
            var prompt = Question3;
            var score = 100;

            return new BinaryQuestion { Id = id, Responses = responses, Prompt = prompt, Score = score };
        }

        public QuestionStep CreateQuestion3b()
        {
            var id = "feedback-q3b";
            var responses = new List<IResponse>
                {
                    new StaticResponse { Prompt = ResponsesPositive01 }
                };
            var prompt = Question3b;
            var score = 100;

            return new FreeTextQuestion { Id = id, Responses = responses, Prompt = prompt, Score = score };
        }

        public StartStep CreateStartStep()
        {
            var id = "feedback-start";
            var responses = new List<IResponse>
                {
                    new StaticResponse() { Id = nameof(Intro), Prompt = Intro, },
                    new StaticResponse() { Id = nameof(IntroOptOut), Prompt = IntroOptOut, }
                };
            return new StartStep() { Id = id, Responses = responses };
        }
    }
}