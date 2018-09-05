using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs
{
    public sealed class ChoicePrompt : Microsoft.Bot.Builder.Dialogs.ChoicePrompt
    {
        public ChoicePrompt(string culture, PromptValidatorEx.PromptValidator<ChoiceResult> validator = null) : base(culture, validator)
        {
        }

        protected override async Task OnPrompt(DialogContext dc, PromptOptions options, bool isRetry)
        {
            var conversationInfo = ConversationState<ConversationInfo>.Get(dc.Context);

            if (isRetry)
            {
                var retryOptions = options as ChoicePromptOptions;
                conversationInfo.Attempts++;

                if (retryOptions != null && retryOptions.Attempts < conversationInfo.Attempts)
                {
                    options.RetryPromptString = retryOptions.TooManyAttemptsString;
                }
            }
            await base.OnPrompt(dc, options, isRetry);
        }
    }
}