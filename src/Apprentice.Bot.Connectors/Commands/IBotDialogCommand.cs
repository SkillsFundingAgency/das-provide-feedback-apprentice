﻿using System.Threading;
using Microsoft.Bot.Builder;

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    public interface IBotDialogCommand
    {
        string Trigger { get; }

        Task<DialogTurnResult> ExecuteAsync(DialogContext dc, CancellationToken cancellationToken);

        bool IsTriggered(DialogContext dc);
    }
}