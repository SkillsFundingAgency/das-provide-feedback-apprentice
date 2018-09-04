namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Prompts.Choices;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// A set of extensions that can be used on dialog turns.
    /// </summary>
    public static class TurnContextExtensions
    {
        /// <summary>
        /// Adds a realistic typing delay to make the bot appear more natural.
        /// </summary>
        /// <param name="ctx">the turn context</param>
        /// <param name="textToType">the text response that the bot should type. Used to determine the length of the delay</param>
        /// <param name="charactersPerMinute">typing speed in characters per minute</param>
        /// <param name="thinkingTimeDelay">millisecond delay to wait before starting a response</param>
        /// <param name="inputHint">the input hint to use. Possible values include acceptingInput, ignoringInput, expectingInput</param>
        /// <returns>The <see cref="Task"/></returns>
        public static async Task SendActivityWithRealisticDelay(this ITurnContext ctx, string textToType, int charactersPerMinute, int thinkingTimeDelay, string inputHint)
        {
            Activity typing = new Activity() { Type = ActivityTypes.Typing, InputHint = InputHints.IgnoringInput };
            await ctx.SendActivity(typing);
            await Task.Delay(FormHelper.CalculateTypingTime(textToType, charactersPerMinute, thinkingTimeDelay));
            await ctx.SendActivity(textToType, inputHint);
        }

        public static async Task SendTypingActivity(
            this ITurnContext ctx,
            string textToType,
            int charactersPerMinute = 500,
            int thinkingTimeDelay = 1000)
        {
            Activity typing = new Activity() { Type = ActivityTypes.Typing, InputHint = InputHints.IgnoringInput };
            await ctx.SendActivity(typing);
            await Task.Delay(FormHelper.CalculateTypingTime(textToType, charactersPerMinute, thinkingTimeDelay));
        }
    }

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
            await dc.Prompt("multiChoicePrompt", questionText, FormHelper.PolarQuestionOptions);
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