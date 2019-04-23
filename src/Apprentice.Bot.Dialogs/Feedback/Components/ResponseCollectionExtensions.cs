namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Helpers;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    using BotSettings = Core.Configuration.Bot;
    using FeatureToggles = Core.Configuration.Features;

    public static class ResponseCollectionExtensions
    {
        public static async Task Create(
            this IEnumerable<IBotResponse> responses,
            ITurnContext context,
            SurveyState surveyState,
            BotSettings botSettings,
            FeatureToggles features,
            CancellationToken cancellationToken)
        {
            var botResponses = responses as IBotResponse[] ?? responses.ToArray();
            if (botResponses.Any())
            {
                if (features.CollateResponses)
                {
                    await botResponses.RespondAsSingleMessageAsync(
                        context,
                        surveyState,
                        botSettings,
                        features,
                        cancellationToken);
                }
                else
                {
                    await botResponses.RespondAsMultipleMessagesAsync(
                        context,
                        surveyState,
                        botSettings,
                        features,
                        cancellationToken);
                }
            }
        }

        public static async Task RespondAsMultipleMessagesAsync(
            this IEnumerable<IBotResponse> responses,
            ITurnContext context,
            SurveyState surveyState,
            BotSettings botSettings,
            FeatureToggles features,
            CancellationToken cancellationToken)
        {
            foreach (IBotResponse r in responses)
            {
                if (r is ConditionalBotResponse conditionalResponse && !conditionalResponse.IsValid(surveyState))
                {
                    continue;
                }

                if (features != null && features.RealisticTypingDelay)
                {
                    await context.SendTypingActivityAsync(
                        r.Prompt,
                        botSettings.Typing.CharactersPerMinute,
                        botSettings.Typing.ThinkingTimeDelay);
                }

                await context.SendActivityAsync(r.Prompt, InputHints.IgnoringInput, cancellationToken: cancellationToken);
            }
        }

        public static async Task RespondAsSingleMessageAsync(
            this IEnumerable<IBotResponse> responses,
            ITurnContext context,
            SurveyState surveyState,
            BotSettings botSettings,
            FeatureToggles features,
            CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();
            foreach (var r in responses)
            {
                if (r is ConditionalBotResponse conditionalResponse && !conditionalResponse.IsValid(surveyState))
                {
                    continue;
                }

                sb.AppendLine(r.Prompt);
            }

            var response = sb.ToString();

            if (features != null && features.RealisticTypingDelay)
            {
                await context.SendTypingActivityAsync(
                    response,
                    botSettings.Typing.CharactersPerMinute,
                    botSettings.Typing.ThinkingTimeDelay);
            }

            var activity = new Activity() {
                Id = context.Activity.Id,
                Type = ActivityTypes.Message,
                InputHint = InputHints.IgnoringInput,
                Text = response
            };

            await context.SendActivityAsync(activity, cancellationToken: cancellationToken);
        }
    }
}