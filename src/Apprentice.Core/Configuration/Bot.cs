// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Bot.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   Defines the Bot type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The bot configuration class.
    /// </summary>
    public class Bot
    {
        public string AdminCommands { get; set; }
        public List<string> AdminCommandsSplit => AdminCommands.Replace(" ", string.Empty).Split(',').ToList();

        public int ConversationExpiryDays { get; set; } = 7;

        /// <summary>
        /// Gets or sets the typing configuration.
        /// </summary>
        public TypingConfig Typing { get; set; }

        /// <summary>
        /// The typing configuration class.
        /// </summary>
        public class TypingConfig
        {
            /// <summary>
            /// Gets or sets the bot typing speed in characters per minute.
            /// </summary>
            public int CharactersPerMinute { get; set; } = 15000;

            /// <summary>
            /// Gets or sets the bot response thinking time in milliseconds.
            /// </summary>
            public int ThinkingTimeDelay { get; set; } = 0;
        }
    }
}