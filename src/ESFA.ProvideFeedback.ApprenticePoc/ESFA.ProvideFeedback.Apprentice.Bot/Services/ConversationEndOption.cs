namespace ESFA.ProvideFeedback.Apprentice.Bot.Services
{
    using System.Collections.Generic;

    /// <inheritdoc />
    /// <summary>
    ///     Represents a single conversation end option.
    /// </summary>
    public class ConversationEndOption : IDialogStep
    {
        /// <inheritdoc />
        public string DialogTarget { get; set; }

        /// <inheritdoc />
        public List<string> Responses { get; set; }
    }
}