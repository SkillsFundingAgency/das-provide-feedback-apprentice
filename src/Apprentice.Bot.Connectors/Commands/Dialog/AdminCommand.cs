namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands.Dialog
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder.Dialogs;

    public abstract class AdminCommand
    {
        protected AdminCommand(string triggerWord)
            => this.Trigger = triggerWord ?? throw new ArgumentNullException(nameof(triggerWord));

        public string Trigger { get; }

        public abstract Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken);

        public virtual bool IsTriggered(DialogContext dc)
        {
            // TODO: check auth
            return dc.Context.Activity.Text.ToLowerInvariant().StartsWith(this.Trigger, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}