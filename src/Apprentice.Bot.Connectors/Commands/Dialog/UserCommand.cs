namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands.Dialog
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder.Dialogs;

    public abstract class UserCommand
    {
        protected UserCommand(string triggerWord)
            => this.Trigger = triggerWord ?? throw new ArgumentNullException(nameof(triggerWord));

        public string Trigger { get; }

        public abstract Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken);

        public bool IsTriggered(DialogContext dc)
        {
            return dc.Context.Activity.Text.ToLowerInvariant().StartsWith(this.Trigger);
        }
    }
}