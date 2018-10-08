namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Models
{
    public class ApprenticeResponse
    {
        public string Question { get; set; }

        public string Answer { get; set; }

        public string Intent { get; set; }

        public int Score { get; set; }
    }
}