namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models
{
    public interface IResponse
    {
        string Id { get; set; }

        string Prompt { get; set; }
    }
}