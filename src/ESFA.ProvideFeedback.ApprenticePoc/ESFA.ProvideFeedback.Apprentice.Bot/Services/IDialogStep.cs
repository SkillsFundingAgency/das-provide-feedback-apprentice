using System.Collections.Generic;

namespace ESFA.ProvideFeedback.Apprentice.Bot.Services
{
    /// <summary>
    ///     Represents a single conversation step.
    /// </summary>
    public interface IDialogStep
    {
        /// <summary>
        ///     Gets or sets the next dialog. Due to the lack of FormFlow with Bot Framework v4.0, this is a nasty string at the minute...
        ///     TODO: refer to the next dialog by its FormFlow object
        /// </summary>
        string DialogTarget { get; set; }

        /// <summary>
        ///     Gets or sets list of responses to echo to the user once the step has been activated. Usually contains some feedback to a users actions.
        /// </summary>
        List<string> Responses { get; set; }
    }
}