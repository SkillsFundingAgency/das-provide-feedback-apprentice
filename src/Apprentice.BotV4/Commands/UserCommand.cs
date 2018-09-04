﻿namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder.Dialogs;

    public abstract class UserCommand : IBotDialogCommand
    {
        protected UserCommand(string triggerWord)
            => this.Trigger = triggerWord ?? throw new ArgumentNullException(nameof(triggerWord));

        public string Trigger { get; }

        public abstract Task ExecuteAsync(DialogContext dc);

        public bool IsTriggered(DialogContext dc)
        {
            return dc.Context.Activity.Text.ToLowerInvariant().StartsWith(this.Trigger);
        }
    }
}