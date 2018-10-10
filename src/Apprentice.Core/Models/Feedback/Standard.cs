namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Feedback
{
    public class Standard : Apprenticeship
    {
        public Standard()
            : base(nameof(Standard))
        {
        }

        public string StandardId { get; set; }
    }
}