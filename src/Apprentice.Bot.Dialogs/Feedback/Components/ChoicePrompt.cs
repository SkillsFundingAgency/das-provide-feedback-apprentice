namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Choices;

    /// <summary>
    /// TODO: add 3 max tries feature again!
    /// </summary>
    public class ChoicePrompt : Microsoft.Bot.Builder.Dialogs.ChoicePrompt
    {
        private readonly IFeedbackBotStateRepository stateRepository;

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
            UserProfile userInfo = stateRepository.UserProfile.GetAsync(turnContext).Result;

            if (isRetry)
            {
                try
                {
                    if (options is RetryPromptOptions retryOptions)
                    {
                        long retries = 1;
                        var hasAttempts = state.ContainsKey("Retries");

                        if (hasAttempts)
                        {
                            retries = (long)state["Retries"];
                            retries++;

                            state["Retries"] = retries;
                        }
                        else
                        {
                            state.Add("Retries", retries);
                        }

                        // Too many retries - replace the retry prompt and terminate the dialog
                        if (retryOptions.Attempts <= retries)
                        {
                            var context = turnContext.TurnState;
                            options.RetryPrompt.Text = retryOptions.TooManyAttemptsString;
                            userInfo.SurveyState.Progress = ProgressState.BlackListed;

                            // TODO: Terminate the conversation
                        }

                        // Check for dynamic retry prompts
                        else if (retryOptions.RetryPromptsCollection.Any())
                        {
                            options.RetryPrompt.Text =
                                retryOptions.RetryPromptsCollection.Single(s => s.Key == retries).Value
                                ?? retryOptions.RetryPrompt.Text;
                        }
                    }
                }
                catch (Exception e)
                {
                    // TODO: Do something here
                }
            }

            return base.OnPromptAsync(turnContext, state, options, isRetry, cancellationToken);
        }

        protected override Task<PromptRecognizerResult<FoundChoice>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            return base.OnRecognizeAsync(turnContext, state, options, cancellationToken);
        }

        public ChoicePrompt(string dialogId, IFeedbackBotStateRepository stateRepository, PromptValidator<FoundChoice> validator = null, string defaultLocale = null)
            : base(dialogId, validator, defaultLocale)
        {
            this.stateRepository = stateRepository;
        }
    }
}