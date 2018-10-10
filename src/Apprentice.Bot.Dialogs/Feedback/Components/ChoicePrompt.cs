namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Choices;

    /// <summary>
    /// TODO: add 3 max tries feature again!
    /// </summary>
    public class ChoicePrompt : Microsoft.Bot.Builder.Dialogs.ChoicePrompt
    {
        public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            return base.BeginDialogAsync(dc, options, cancellationToken);
        }

        public override Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            return base.ContinueDialogAsync(dc, cancellationToken);
        }

        public override Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            return base.EndDialogAsync(turnContext, instance, reason, cancellationToken);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override Task RepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            return base.RepromptDialogAsync(turnContext, instance, cancellationToken);
        }

        public override Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return base.ResumeDialogAsync(dc, reason, result, cancellationToken);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        protected override Task OnPromptAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, bool isRetry, CancellationToken cancellationToken = default(CancellationToken))
        {
            return base.OnPromptAsync(turnContext, state, options, isRetry, cancellationToken);
        }

        protected override Task<PromptRecognizerResult<FoundChoice>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            return base.OnRecognizeAsync(turnContext, state, options, cancellationToken);
        }

        public ChoicePrompt(string dialogId, PromptValidator<FoundChoice> validator = null, string defaultLocale = null)
            : base(dialogId, validator, defaultLocale)
        {
        }
    }

    //public sealed class ChoicePrompt : Microsoft.Bot.Builder.Dialogs.ChoicePrompt
    //{
    //    public ChoicePrompt(string culture, PromptValidatorEx.PromptValidator<ChoiceResult> validator = null)
    //        : base(culture, validator)
    //    {
    //    }

    //    // <summary>
    //    // What happens when the dialog prompts the user for input?
    //    // </summary>
    //    // <param name = "dc" ></ param >
    //    // < param name="options"></param>
    //    // <param name = "isRetry" ></ param >
    //    // < returns ></ returns >
    //    protected override async Task OnPromptAsync(DialogContext dc, PromptOptions options, bool isRetry)
    //    {
    //        UserProfile userInfo = UserState<UserProfile>.Get(dc.Context);

    //        if (isRetry)
    //        {
    //            try
    //            {
    //                if (options is ChoicePromptOptions retryOptions)
    //                {
    //                    long retries = 1;
    //                    var hasAttempts = dc.ActiveDialog.State.ContainsKey("Retries");

    //                    if (hasAttempts)
    //                    {
    //                        retries = (long)dc.ActiveDialog.State["Retries"];
    //                        retries++;

    //                        dc.ActiveDialog.State["Retries"] = retries;
    //                    }
    //                    else
    //                    {
    //                        dc.ActiveDialog.State.Add("Retries", retries);
    //                    }

    //                    Too many retries - replace the retry prompt and terminate the dialog
    //                    if (retryOptions.Attempts <= retries)
    //                    {
    //                        options.RetryPromptString = retryOptions.TooManyAttemptsString;
    //                        userInfo.SurveyState.Progress = ProgressState.BlackListed;

    //                        TODO: Terminate the conversation
    //                    }

    //                    Check for dynamic retry prompts
    //                    else if (retryOptions.RetryPromptsCollection.Any())
    //                        {
    //                            options.RetryPromptString =
    //                                retryOptions.RetryPromptsCollection.Single(s => s.Key == retries).Value
    //                                ?? retryOptions.RetryPromptString;
    //                        }
    //                }
    //            }
    //            catch (Exception e)
    //            {
    //                await dc.Context.TraceActivity(e.Message);
    //            }
    //        }

    //        await base.OnPrompt(dc, options, isRetry);
    //    }


    //    protected override async Task<ChoiceResult> OnRecognize(DialogContext dc, PromptOptions options)
    //    {
    //        ConversationInfo conversationInfo = ConversationState<ConversationInfo>.Get(dc.Context);
    //        conversationInfo.Attempts = 0;

    //        return await base.OnRecognize(dc, options);
    //    }
    //}

}