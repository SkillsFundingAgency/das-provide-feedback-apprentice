using System.Threading;
using Microsoft.Bot.Builder;

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    public abstract class AdminCommand
    {
        protected AdminCommand(string triggerWord)
            => this.Trigger = triggerWord ?? throw new ArgumentNullException(nameof(triggerWord));

        public string Trigger { get; }

        public abstract Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken);

        public bool IsTriggered(DialogContext dc)
        {
            // TODO: check auth
            return dc.Context.Activity.Text.ToLowerInvariant().StartsWith(this.Trigger, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}