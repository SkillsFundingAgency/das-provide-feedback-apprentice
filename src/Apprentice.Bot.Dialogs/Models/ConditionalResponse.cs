namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models
{
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    public interface IConditionalResponse : IResponse
    {
        bool IsValid(SurveyState state);
    }

    public abstract class ConditionalResponse : IConditionalResponse
    {
        public string Id { get; set; }

        public string Prompt { get; set; }

        public abstract bool IsValid(SurveyState state);
    }

}