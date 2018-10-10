namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation
{
    using System.Runtime.Serialization;

    public enum ProgressState
    {
        /// <summary>
        /// Feedback request has been sent but not started
        /// </summary>
        [EnumMember(Value = "notStarted")]
        NotStarted,

        /// <summary>
        /// Feedback is in progress.
        /// </summary>
        [EnumMember(Value = "inProgress")]
        InProgress,

        /// <summary>
        /// Feedback has been completed
        /// </summary>
        [EnumMember(Value = "complete")]
        Complete,

        /// <summary>
        /// Feedback cycle has expired before user completed the survey
        /// </summary>
        [EnumMember(Value = "expired")]
        Expired,

        /// <summary>
        /// User has opted out of the survey
        /// </summary>
        [EnumMember(Value = "optedOut")]
        OptedOut,

        /// <summary>
        /// User has entered too many bad responses, so we've banned them from this survey
        /// </summary>
        [EnumMember(Value = "blackListed")]
        BlackListed
    }
}