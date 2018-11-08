namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components
{
    public interface ICustomComponent<out T>
    {
        T Build();
    }
}