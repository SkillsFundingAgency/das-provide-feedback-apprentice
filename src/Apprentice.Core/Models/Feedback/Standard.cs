namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Models
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