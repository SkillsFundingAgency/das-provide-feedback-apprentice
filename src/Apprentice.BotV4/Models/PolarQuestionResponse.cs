namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models
{
    /// <summary>
    /// State information associated with the check-in dialog.
    /// </summary>
    public class PolarQuestionResponse : IQuestionResponse
    {
        public string Question { get; set; }

        public string Answer { get; set; }

        public string Intent { get; set; }

        public int Score { get; set; }

        public bool IsPositive => this.Score > 0;
    }
}