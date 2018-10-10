namespace ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Feedback
{
    public class Apprenticeship
    {
        protected Apprenticeship(string type)
        {
            this.Type = type;
        }

        public string Type { get; set; }

        public string Provider { get; set; }
    }
}