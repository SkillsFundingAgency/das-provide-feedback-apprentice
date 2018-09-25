//using System.Threading;
//using Microsoft.Bot.Builder;

//namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs.Components
//{
//    using System.Collections.Generic;
//    using System.Threading.Tasks;

//    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;

//    using Microsoft.Bot.Builder.Dialogs;

//    public abstract class SingleStepDialog : ComponentDialog
//    {
//        protected SingleStepDialog(string id)
//            : base(id)
//        {
//        }

//        public ICollection<IResponse> Responses { get; protected set; } = new List<IResponse>();

//        public SingleStepDialog AddResponse(IResponse response)
//        {
//            this.Responses.Add(response);
//            return this;
//        }

//        public SingleStepDialog Build()
//        {
//            WaterfallDialog dialog = new WaterfallDialog("step", new WaterfallStep[] {StepDialogAsync, EndDialogAsync});
//            this.AddDialog(dialog);

//            return this;
//        }

//        private async Task<DialogTurnResult> EndDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {
//            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
//        }

//        private async Task<DialogTurnResult> StepDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {
//            return await this.Step(stepContext, cancellationToken);
//        }

//        public SingleStepDialog WithResponses(ICollection<IResponse> responses)
//        {
//            this.Responses = responses;
//            return this;
//        }

//        protected abstract Task<DialogTurnResult> Step(WaterfallStepContext turnContext, CancellationToken cancellationToken = default(CancellationToken));
//    }
//}