namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4
{
    using System.Runtime.Serialization;

    /// <summary>
    /// The channels supported by this bot.
    /// </summary>
    public enum BotChannel
    {
        /// <summary>
        /// Conversations over slack.
        /// </summary>
        [EnumMember(Value = "slack")]
        Slack,

        /// <summary>
        /// Conversations using the Direct Line API.
        /// </summary>
        [EnumMember(Value = "directline")]
        DirectLine,

        /// <summary>
        /// Conversations over SMS (typically using Twilio integration).
        /// </summary>
        [EnumMember(Value = "sms")]
        Sms,

        /// <summary>
        /// Conversations over the bot framework Emulator.
        /// </summary>
        [EnumMember(Value = "emulator")]
        Emulator,

        /// <summary>
        /// Channel is unsupported.
        /// </summary>
        [EnumMember(Value = "unsupported")]
        Unsupported
    }
}