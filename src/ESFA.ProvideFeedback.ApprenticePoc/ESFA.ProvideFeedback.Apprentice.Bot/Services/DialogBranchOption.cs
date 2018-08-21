namespace ESFA.ProvideFeedback.Apprentice.Bot.Services
{
    using System.Collections.Generic;

    /// <inheritdoc />
    /// <summary>
    ///     Represents a single step of branching dialog.
    /// </summary>
    public class DialogBranchOption : IDialogStep
    {
        /// <inheritdoc />
        public string DialogTarget { get; set; }

        /// <inheritdoc />
        public List<string> Responses { get; set; }
    }
}