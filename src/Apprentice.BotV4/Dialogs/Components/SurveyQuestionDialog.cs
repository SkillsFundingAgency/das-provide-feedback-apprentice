namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs.Components
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Helpers;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.State;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Prompts;
    using Microsoft.Bot.Builder.Prompts.Choices;
    using Microsoft.Bot.Schema;
    using Microsoft.Recognizers.Text;

    using ChoicePrompt = Microsoft.Bot.Builder.Dialogs.ChoicePrompt;

    public sealed class SurveyQuestionDialog : DialogContainer
    {
        public SurveyQuestionDialog(string id)
            : base(id)
        {
        }

        public string Prompt { get; private set; }

        public ICollection<IResponse> Responses { get; private set; } = new List<IResponse>();

        public int Score { get; private set; } = 1;

        public SurveyQuestionDialog AddResponse(IResponse response)
        {
            this.Responses.Add(response);
            return this;
        }

        public SurveyQuestionDialog Build()
        {
            var steps = new WaterfallStep[]
                            {
                                async (dc, args, next) => { await this.Question(dc, args, next); },
                                async (dc, args, next) => { await this.Response(dc, args, next); },
                                async (dc, args, next) => { await dc.End(); }
                            };

            this.Dialogs.Add(this.DialogId, steps);

            // Define the prompts used in this conversation flow.
            this.Dialogs.Add($"{this.DialogId}-prompt", new ChoicePrompt(Culture.English) { Style = ListStyle.None });

            return this;
        }

        public SurveyQuestionDialog WithPrompt(string prompt)
        {
            this.Prompt = prompt;
            return this;
        }

        public SurveyQuestionDialog WithResponses(ICollection<IResponse> responses)
        {
            this.Responses = responses;
            return this;
        }

        public SurveyQuestionDialog WithScore(int score)
        {
            this.Score = score;
            return this;
        }

        private async Task AskPolarQuestion(DialogContext dc, string questionText)
        {
            await dc.Prompt($"{this.DialogId}-prompt", questionText, this.BuildPolarQuestionOptions(dc));
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

        private async Task Question(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            // Ask question A
            await dc.Context.SendTypingActivity(this.Prompt);
            await this.AskPolarQuestion(dc, this.Prompt);
        }

        private async Task Response(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);
            ConversationInfo conversationInfo = ConversationState<ConversationInfo>.Get(dc.Context);

            userInfo.SurveyState.Progress = ProgressState.Enagaged;

            BinaryQuestionResponse response = await dc.GetPolarQuestionResponse(args, this.Prompt, this.Score);

            userInfo.SurveyState.Responses.Add(response);

            foreach (IResponse r in this.Responses)
            {
                if (r is ConditionalResponse<BinaryQuestionResponse> conditionalResponse)
                {
                    if (!conditionalResponse.IsValid(response))
                    {
                        continue;
                    }
                }

                await dc.Context.SendTypingActivity(r.Prompt);
                await dc.Context.SendActivity(r.Prompt);
            }

            // Ask next question
            await next();
        }
    }
}