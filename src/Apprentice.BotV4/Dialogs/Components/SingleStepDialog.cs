namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs.Components
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;

    using Microsoft.Bot.Builder.Dialogs;

    public abstract class SingleStepDialog : DialogContainer
    {
        protected SingleStepDialog(string id)
            : base(id)
        {
        }

        public ICollection<IResponse> Responses { get; protected set; } = new List<IResponse>();

        public SingleStepDialog AddResponse(IResponse response)
        {
            this.Responses.Add(response);
            return this;
        }

        public SingleStepDialog Build()
        {
            var steps = new WaterfallStep[]
                            {
                                async (dc, args, next) => { await this.Step(dc, args, next); },
                                async (dc, args, next) => { await dc.End(); }
                            };

            this.Dialogs.Add(this.DialogId, steps);

            return this;
        }

        public SingleStepDialog WithResponses(ICollection<IResponse> responses)
        {
            this.Responses = responses;
            return this;
        }

        protected abstract Task Step(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next);
    }
}