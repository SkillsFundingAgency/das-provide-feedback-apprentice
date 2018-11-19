namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Surveys
{
    using System.Collections.Generic;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;

    public class InMemoryIfaDemoSurveyV1 : ISurveyDefinition
    {
        private const string Intro = "Please complete 4 quick questions to help the government improve apprenticeships in the digital sector.";

        private const string IntroOptOut = "Normal SMS charges apply. Text ‘Stop’ to opt out.";

        private const string Question1 = "Is your apprenticeship helping you to do your job?";

        private const string Question2 = "Thanks. What skills or knowledge could we add to your apprenticeship training to make it better?";

        private const string Question3 = "Thanks. What part of your apprenticeship is not relevant to your work or needs to be updated?";

        private const string Question4 = "Thanks for that. Do you think your apprenticeship will help you progress in your career?";

        private const string QuestionPleaseTypeYesOrNo = "Please type 'Yes' or 'No'";

        private const string QuestionFreeTextInstructions = "Tell us or type ‘skip’ to go to the next question.";

        private const string EndThanks = "Thanks very much for your answers. That's it for now.";

        private const string EndWeWontReply = "We won't get back to you directly, but your answers will help us improve digital sector apprenticeships.";

        public InMemoryIfaDemoSurveyV1(string id = "ifa-v1")
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
        public StartStepDefinition CreateStartStep()
        {
            var id = "feedback-start";
            var responses = new List<IBotResponse>
            {
                new StaticBotResponse() { Id = nameof(Intro), Prompt = Intro, },
                new StaticBotResponse() { Id = nameof(IntroOptOut), Prompt = IntroOptOut, }
            };
            return new StartStepDefinition() { Id = id, Responses = responses };
        }

        public QuestionStepDefinition CreateQuestion1()
        {
            var id = "feedback-q1";
            var responses = new List<IBotResponse> { };
            var prompt = $"{Question1}\n{QuestionPleaseTypeYesOrNo}";
            var score = 100;

            return new BinaryQuestion { Id = id, Responses = responses, Prompt = prompt, Score = score };
        }

        public QuestionStepDefinition CreateQuestion2()
        {
            var id = "feedback-q2";
            var responses = new List<IBotResponse> { };
            var prompt = $"{Question2}\n{QuestionFreeTextInstructions}";
            var score = 100;

            return new FreeTextQuestion() { Id = id, Responses = responses, Prompt = prompt, Score = score };
        }

        public QuestionStepDefinition CreateQuestion3()
        {
            var id = "feedback-q3";
            var responses = new List<IBotResponse> { };
            var prompt = $"{Question3}\n{QuestionFreeTextInstructions}";
            var score = 100;

            return new FreeTextQuestion { Id = id, Responses = responses, Prompt = prompt, Score = score };
        }

        public QuestionStepDefinition CreateQuestion4()
        {
            var id = "feedback-q4";
            var responses = new List<IBotResponse> { };
            var prompt = $"{Question4}\n{QuestionPleaseTypeYesOrNo}";
            var score = 100;

            return new BinaryQuestion { Id = id, Responses = responses, Prompt = prompt, Score = score };
        }

        public EndStepDefinition CreateEndStep()
        {
            var id = "feedback-end";
            var responses = new List<IBotResponse>
            {
                new StaticBotResponse() { Id = nameof(EndThanks), Prompt = EndThanks },
                new StaticBotResponse() { Id = nameof(EndWeWontReply), Prompt = EndWeWontReply }
            };
            return new EndStepDefinition() { Id = id, Responses = responses };
        }
    }
}