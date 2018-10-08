namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Models
{
    public class Framework : Apprenticeship
    {
        public Framework()
            : base(nameof(Framework))
        {
        }

        public string FrameworkId { get; set; }

        public string Pathway { get; set; }

        public string ProgrammeType { get; set; }
    }
}