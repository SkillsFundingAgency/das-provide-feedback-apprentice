using System.Collections.Generic;

namespace ESFA.ProvideFeedback.Apprentice.Bot.Services
{
    public interface IDialogStep
    {
        /// <summary>
        /// The next dialog. Due to the lack of FormFlow with Bot Framework v4.0, this is a nasty string at the minute...
        /// TODO: refer to the next dialog by its FormFlow object
        /// </summary>
        string DialogTarget { get; set; }

        /// <summary>
        /// A list of responses to echo to the user once the step has been activated. Usually contains some feedback to a users actions.
        /// </summary>
        List<string> Responses { get; set; }
    }
}