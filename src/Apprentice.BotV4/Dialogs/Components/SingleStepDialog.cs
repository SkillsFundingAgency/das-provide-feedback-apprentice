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
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;

    public abstract class SingleStepDialog<TState, TDialog>
    {
        protected SingleStepDialog(TState state, TDialog dialog)
            : base()
        {
            this.State = Activator.CreateInstance<TState>();
            this.Configuration = new DialogConfiguration(); // TODO: Inject from services (first pass)
        }

        public DialogConfiguration Configuration { get; }

        public ICollection<IResponse> Responses { get; protected set; } = new List<IResponse>();

        public TState State { get; }

        public SingleStepDialog<TState, TDialog> AddResponse(IResponse response)
        {
            this.Responses.Add(response);
            return this;
        }

        public TDialog Build(string id)
        {
            ComponentDialog dialog = new ComponentDialog(id);

            var steps = new WaterfallStep[] { this.StepAsync, this.WrapUpAsync };

            var waterfall = new WaterfallDialog(dialog.Id, steps);

            dialog.AddDialog(waterfall);

            return dialog;
        }

        public SingleStepDialog<TState, TDialog> WithResponses(ICollection<IResponse> responses)
        {
            this.Responses = responses;
            return this;
        }

        protected async Task RespondAsMultipleMessages(
            IEnumerable<IResponse> responses,
            DialogContext dc,
            UserInfo userInfo)
        {
            foreach (IResponse r in responses)
            {
                if (r is PredicateResponse predicatedResponse && !predicatedResponse.IsValid(userInfo))
                {
                    continue;
                }

                if (this.Configuration.RealisticTypingDelay)
                {
                    await dc.Context.SendTypingActivity(
                        r.Prompt,
                        this.Configuration.CharactersPerMinute,
                        this.Configuration.ThinkingTimeDelayMs);
                }

                await dc.Context.SendActivityAsync(r.Prompt, InputHints.IgnoringInput);
            }
        }

        protected async Task RespondAsSingleMessage(
            IEnumerable<IResponse> responses,
            DialogContext dc,
            UserInfo userInfo)
        {
            StringBuilder sb = new StringBuilder();
            foreach (IResponse r in responses)
            {
                if (r is PredicateResponse predicatedResponse && !predicatedResponse.IsValid(userInfo))
                {
                    continue;
                }

                sb.AppendLine(r.Prompt);
            }

            var response = sb.ToString();

            if (this.Configuration.RealisticTypingDelay)
            {
                await dc.Context.SendTypingActivity(
                    response,
                    this.Configuration.CharactersPerMinute,
                    this.Configuration.ThinkingTimeDelayMs);
            }

            await dc.Context.SendActivityAsync(response, InputHints.IgnoringInput);
        }

        protected abstract Task<DialogTurnResult> StepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken);

        private Task<DialogTurnResult> WrapUpAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}