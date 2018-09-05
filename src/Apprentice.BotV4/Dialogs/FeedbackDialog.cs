using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Helpers;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Prompts;
    using Microsoft.Bot.Builder.Prompts.Choices;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;
    using Microsoft.Recognizers.Text;

    using ChoicePrompt = Microsoft.Bot.Builder.Dialogs.ChoicePrompt;
    using TextPrompt = Microsoft.Bot.Builder.Dialogs.TextPrompt;

    public class FeedbackDialog : DialogContainer
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

        private readonly IStringLocalizer<FeedbackDialog> localizer;

        public FeedbackDialog(IStringLocalizer<FeedbackDialog> localizer)
            : base(new Guid().ToString())
        {
            this.localizer = localizer;
        }

        public string Id { get; }

        public DialogContainer Build(string id)
        {
            var steps = new WaterfallStep[]
                            {
                                async (dc, args, next) => { await this.StepA(dc, args, next); },
                                async (dc, args, next) => { await this.StepB(dc, args, next); },
                                async (dc, args, next) => { await this.StepC(dc, args, next); },
                                async (dc, args, next) => { await this.SurveyEnd(dc, args, next); }
                            };

            this.Dialogs.Add(this.Id, steps);

            // Define the prompts used in this conversation flow.
            this.Dialogs.Add("textPrompt", new TextPrompt());
            this.Dialogs.Add("multiChoicePrompt", new ChoicePrompt(Culture.English) { Style = ListStyle.None });

            return this;
        }

        private async Task AskPolarQuestion(DialogContext dc, string questionText)
        {
            await dc.Prompt("multiChoicePrompt", questionText, this.BuildPolarQuestionOptions(dc));
        }

        private List<Choice> BuildConfirmationChoices()
        {
            return new List<Choice>()
                       {
                           new Choice
                               {
                                   Action = new CardAction(
                                       text: "yes",
                                       title: "yes",
                                       value: "yes"),
                                   Value = "yes",
                                   Synonyms = new List<string>()
                                                  {
                                                      "true",
                                                      "yep",
                                                      "yeah",
                                                      "ok",
                                                      "y"
                                                  }
                               },
                           new Choice
                               {
                                   Action = new CardAction(text: "no", title: "no", value: "no"),
                                   Value = "no",
                                   Synonyms = new List<string>()
                                                  {
                                                      "false",
                                                      "nope",
                                                      "nah",
                                                      "negative",
                                                      "n"
                                                  }
                               },
                       };
        }

        private ChoicePromptOptions BuildPolarQuestionOptions(DialogContext dc)
        {
            return new ChoicePromptOptions()
                       {
                           Choices = this.BuildConfirmationChoices(),
                           RetryPromptActivity = this.GenerateRetryPrompt(dc)
                       };
        }

        private Activity GenerateRetryPrompt(DialogContext dc)
        {
            return MessageFactory.Text(
                $"Sorry, I'm just a simple bot. Please type ‘Yes’ or ‘No’",
                inputHint: InputHints.ExpectingInput);
        }

        private async Task StepA(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);

            userInfo.SurveyState.StartDate = DateTime.Now;
            userInfo.SurveyState.Progress = ProgressState.NotStarted;

            userInfo.SurveyState.SurveyId = this.Id;

            await dc.Context.SendTypingActivity(IntroWelcome);
            await dc.Context.SendActivity(IntroWelcome, inputHint: InputHints.IgnoringInput);

            await dc.Context.SendTypingActivity(IntroOptOut);
            await dc.Context.SendActivity(IntroOptOut, inputHint: InputHints.IgnoringInput);

            // Ask question A
            await dc.Context.SendTypingActivity(QuestionsDaysOfTraining);
            await this.AskPolarQuestion(dc, QuestionsDaysOfTraining);
        }

        private async Task StepB(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);
            ConversationInfo conversationInfo = ConversationState<ConversationInfo>.Get(dc.Context);

            userInfo.SurveyState.Progress = ProgressState.InProgress;

            BinaryQuestionResponse response = await dc.GetPolarQuestionResponse(args, QuestionsDaysOfTraining);

            userInfo.SurveyState.Responses.Add(response);

            if (response != null && response.IsPositive)
            {
                await dc.Context.SendTypingActivity(ResponsesPositive01);
                await dc.Context.SendActivity(ResponsesPositive01);
            }
            else
            {
                await dc.Context.SendTypingActivity(ResponsesNegative01);
                await dc.Context.SendActivity(ResponsesNegative01);
            }

            // Ask question B
            await dc.Context.SendTypingActivity(QuestionsTrainerKnowledge);
            await this.AskPolarQuestion(dc, QuestionsTrainerKnowledge);
        }

        private async Task StepC(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);
            ConversationInfo conversationInfo = ConversationState<ConversationInfo>.Get(dc.Context);

            BinaryQuestionResponse response = await dc.GetPolarQuestionResponse(args, QuestionsTrainerKnowledge);

            userInfo.SurveyState.Responses.Add(response);

            if (response != null && response.IsPositive)
            {
                await dc.Context.SendTypingActivity(ResponsesPositive02);
                await dc.Context.SendActivity(ResponsesPositive02);
            }
            else
            {
                await dc.Context.SendTypingActivity(ResponsesNegative02);
                await dc.Context.SendActivity(ResponsesNegative02);
            }

            // Ask question C
            await dc.Context.SendTypingActivity(QuestionsOverallSatisfaction);
            await this.AskPolarQuestion(dc, QuestionsOverallSatisfaction);
        }

        private async Task SurveyEnd(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);
            ConversationInfo conversationInfo = ConversationState<ConversationInfo>.Get(dc.Context);

            BinaryQuestionResponse response = await dc.GetPolarQuestionResponse(args, QuestionsOverallSatisfaction);

            userInfo.SurveyState.Responses.Add(response);

            if (response != null && response.IsPositive)
            {
                await dc.Context.SendTypingActivity(ResponsesItsReallyHelpful);
                await dc.Context.SendActivity(ResponsesItsReallyHelpful);
            }
            else
            {
                await dc.Context.SendTypingActivity(ResponsesSorryToHearThat);
                await dc.Context.SendActivity(ResponsesSorryToHearThat);
            }

            int maxScore = userInfo.SurveyState.Responses.Count;

            if (userInfo.SurveyState.Score >= maxScore)
            {
                await dc.Context.SendTypingActivity(FinishKeepUpTheGoodWork);
                await dc.Context.SendActivity(FinishKeepUpTheGoodWork);
            }
            else
            {
                await dc.Context.SendTypingActivity(FinishSpeakToYourEmployer);
                await dc.Context.SendActivity(FinishSpeakToYourEmployer);
            }

            if (userInfo.SurveyState.Score < 0)
            {
                await dc.Context.SendTypingActivity(FinishFormalComplaint);
                await dc.Context.SendActivity(FinishFormalComplaint);
            }

            userInfo.SurveyState.Progress = ProgressState.Complete;
            userInfo.SurveyState.EndDate = DateTime.Now;

            await dc.End();
        }
    }
}