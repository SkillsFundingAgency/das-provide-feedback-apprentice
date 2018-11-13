namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models
{
    /// <summary>
    /// A response from the bot, based on user interaction
    /// </summary>
    public interface IBotResponse
    {
        string Id { get; set; }

        string Prompt { get; set; }
    }
}