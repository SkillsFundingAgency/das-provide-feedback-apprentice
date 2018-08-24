namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models
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
        Enagaged,

        /// <summary>
        /// Feedback has been completed
        /// </summary>
        [EnumMember(Value = "complete")]
        Complete,

        /// <summary>
        /// Feedback cycle has expired before user completed the survey
        /// </summary>
        [EnumMember(Value = "expired")]
        Expired
    }
}