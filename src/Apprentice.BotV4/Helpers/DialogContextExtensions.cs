using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;

namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Choices;
    using Microsoft.Bot.Schema;

    public static class DialogContextExtensions
    {
        public static async Task<T> BeginState<T>(this DialogContext dc, string keyName)
            where T : new()
        {
            return await Task.Run(() => (T)(dc.ActiveDialog.State[keyName] = Activator.CreateInstance<T>()));
        }

        public static async Task<T> GetDialogState<T>(this DialogContext dc, string keyName)
        {
            return await Task.Run(() => (T)dc.ActiveDialog.State[keyName]);
        }

        public static async Task AskPolarQuestion(this DialogContext dc, string questionText)
        {
            var prompOptions = FormHelper.PolarQuestionOptions;
            prompOptions.Prompt = new Activity
                                      {
                                          Type = ActivityTypes.Message,
                                          Text = questionText
                                      };
            await dc.PromptAsync("multiChoicePrompt", prompOptions);
        }

        public static async Task<BinaryQuestionResponse> GetPolarQuestionResponse(this DialogContext dc, IDictionary<string, object> args, string questionText, int score = 0)
        {
            return await Task.Run(
                () =>
                    {
                        string utterance = dc.Context.Activity.Text;                // What did they say?
                        string intent = (args["Value"] as FoundChoice)?.Value;      // What did they mean?
                        bool positive = intent == "yes";                            // Was it positive?

                        BinaryQuestionResponse feedbackResponse =
                            new BinaryQuestionResponse
                            {
                                    Question = questionText,
                                    Answer = utterance,
                                    Intent = intent,
                                    Score = positive ? score : -score
                                };

                        return feedbackResponse;
                    });
        }
    }
}