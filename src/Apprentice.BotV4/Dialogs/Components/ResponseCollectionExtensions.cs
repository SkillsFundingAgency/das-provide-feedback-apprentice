namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs.Components
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Helpers;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;

    static class ResponseCollectionExtensions
    {
        public static async Task RespondAsMultipleMessagesAsync(
            this IEnumerable<IResponse> responses,
            ITurnContext context,
            SurveyState surveyState,
            DialogConfiguration configuration,
            CancellationToken cancellationToken)
        {
            foreach (var r in responses)
            {
                if (r is ConditionalResponse conditionalResponse && !conditionalResponse.IsValid(surveyState))
                {
                    continue;
                }

                if (configuration != null && configuration.RealisticTypingDelay)
                {
                    await context.SendTypingActivityAsync(
                        r.Prompt,
                        configuration.CharactersPerMinute,
                        configuration.ThinkingTimeDelayMs);
                }

                await context.SendActivityAsync(r.Prompt, InputHints.IgnoringInput, cancellationToken: cancellationToken);
            }
        }

        public static async Task RespondAsSingleMessageAsync(
            this IEnumerable<IResponse> responses,
            ITurnContext context,
            SurveyState surveyState,
            DialogConfiguration configuration,
            CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();
            foreach (var r in responses)
            {
                if (r is PredicateResponse predicatedResponse && !predicatedResponse.IsValid(surveyState))
                {
                    continue;
                }

                sb.AppendLine(r.Prompt);
            }

            var response = sb.ToString();

            if (configuration != null && configuration.RealisticTypingDelay)
            {
                await context.SendTypingActivityAsync(
                    response,
                    configuration.CharactersPerMinute,
                    configuration.ThinkingTimeDelayMs);
            }

            await context.SendActivityAsync(response, InputHints.IgnoringInput, cancellationToken: cancellationToken);
        }
    }
}