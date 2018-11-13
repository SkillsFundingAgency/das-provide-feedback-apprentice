namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models
{
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    public interface IConditionalBotResponse : IBotResponse
    {
        bool IsValid(SurveyState state);
    }

    public abstract class ConditionalBotResponse : IConditionalBotResponse
    {
        public string Id { get; set; }

        public string Prompt { get; set; }

        public abstract bool IsValid(SurveyState state);
    }

}