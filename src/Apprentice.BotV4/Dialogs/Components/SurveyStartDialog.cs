namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs.Components
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Helpers;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Options;

    using AzureSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Azure;
    using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;
    using DataSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Data;
    using NotifySettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Notify;
    using FeatureToggles = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Features;

    public sealed class SurveyStartDialog : ComponentDialog
    {
        private readonly BotSettings botSettings;

        private readonly FeatureToggles features;

        private FeedbackBotState state;

        private DialogConfiguration configuration;

        /// <inheritdoc />
        public SurveyStartDialog(FeedbackBotState state, BotSettings botSettings, FeatureToggles features)
            : base("survey-start")
        {
            this.botSettings = botSettings;
            this.features = features;
            this.state = state;
            this.configuration = new DialogConfiguration(); // TODO: Inject from IOptions
        }

        public ICollection<IResponse> Responses { get; protected set; } = new List<IResponse>();

        public SurveyStartDialog AddResponse(IResponse response)
        {
            this.Responses.Add(response);
            return this;
        }

        public SurveyStartDialog Build(string id)
        {
            var steps = new WaterfallStep[]
                {
                    this.StepAsync,
                    this.WrapUpAsync,
                };

            var waterfall = new WaterfallDialog(id, steps);

            this.AddDialog(waterfall);

            return this;
        }

        public SurveyStartDialog WithResponses(ICollection<IResponse> responses)
        {
            this.Responses = responses;
            return this;
        }

        private async Task<DialogTurnResult> StepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userInfo = await this.state.UserInfo.GetAsync(
                                    stepContext.Context,
                                    () => new UserInfo(),
                                    cancellationToken);

            if (this.configuration.CollateResponses)
            {
                await this.Responses.RespondAsSingleMessageAsync(stepContext, this.configuration, cancellationToken);
            }
            else
            {
                await this.Responses.RespondAsMultipleMessagesAsync(stepContext, this.configuration, cancellationToken);
            }

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> WrapUpAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }

    static class ResponseCollectionExtensions
    {
        public static async Task RespondAsMultipleMessagesAsync(
            this IEnumerable<IResponse> responses,
            DialogContext dc,
            DialogConfiguration configuration,
            CancellationToken cancellationToken)
        {
            foreach (var r in responses)
            {
                if (r is ConditionalResponse conditionalResponse && !conditionalResponse.IsValid(dc))
                {
                    continue;
                }

                if (configuration != null && configuration.RealisticTypingDelay)
                {
                    await dc.Context.SendTypingActivityAsync(
                        r.Prompt,
                        configuration.CharactersPerMinute,
                        configuration.ThinkingTimeDelayMs);
                }

                await dc.Context.SendActivityAsync(r.Prompt, InputHints.IgnoringInput, cancellationToken: cancellationToken);
            }
        }

        public static async Task RespondAsSingleMessageAsync(
            this IEnumerable<IResponse> responses,
            DialogContext dc,
            DialogConfiguration configuration,
            CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();
            foreach (var r in responses)
            {
                if (r is PredicateResponse predicatedResponse && !predicatedResponse.IsValid(dc))
                {
                    continue;
                }

                sb.AppendLine(r.Prompt);
            }

            var response = sb.ToString();

            if (configuration != null && configuration.RealisticTypingDelay)
            {
                await dc.Context.SendTypingActivityAsync(
                    response,
                    configuration.CharactersPerMinute,
                    configuration.ThinkingTimeDelayMs);
            }

            await dc.Context.SendActivityAsync(response, InputHints.IgnoringInput, cancellationToken: cancellationToken);
        }
    }
}