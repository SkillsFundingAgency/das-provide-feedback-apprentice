namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models
{
    public class StaticBotResponse : IBotResponse
    {
        public string Id { get; set; }

        public string Prompt { get; set; }
    }
}