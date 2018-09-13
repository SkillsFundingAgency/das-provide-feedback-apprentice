namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Prompts;
    using Microsoft.Bot.Builder.TraceExtensions;

    public sealed class ChoicePrompt : Microsoft.Bot.Builder.Dialogs.ChoicePrompt
    {
        public ChoicePrompt(string culture, PromptValidatorEx.PromptValidator<ChoiceResult> validator = null)
            : base(culture, validator)
        {
        }

        /// <summary>
        /// What happens when the dialog prompts the user for input?
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="options"></param>
        /// <param name="isRetry"></param>
        /// <returns></returns>
        protected override async Task OnPrompt(DialogContext dc, PromptOptions options, bool isRetry)
        {
            ConversationInfo conversationInfo = ConversationState<ConversationInfo>.Get(dc.Context);
            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);

            if (isRetry)
            {
                try
                {
                    ChoicePromptOptions retryOptions = options as ChoicePromptOptions;
                    conversationInfo.Attempts++;

                    if (retryOptions != null)
                    {
                        // Too many retries - replace the retry prompt and terminate the dialog
                        if (retryOptions.Attempts <= conversationInfo.Attempts)
                        {
                            options.RetryPromptString = retryOptions.TooManyAttemptsString;
                            userInfo.SurveyState.Progress = ProgressState.BlackListed;

                            // TODO: Terminate the conversation
                        }

                        // Check for dynamic retry prompts
                        else if (retryOptions.RetryPromptsCollection.Any())
                        {
                            options.RetryPromptString =
                                retryOptions.RetryPromptsCollection.Single(s => s.Key == conversationInfo.Attempts).Value
                                ?? retryOptions.RetryPromptString;
                        }

                    }
                }
                catch (Exception e)
                {
                    await dc.Context.TraceActivity(e.Message);
                }


            }

            await base.OnPrompt(dc, options, isRetry);
        }
    }
}