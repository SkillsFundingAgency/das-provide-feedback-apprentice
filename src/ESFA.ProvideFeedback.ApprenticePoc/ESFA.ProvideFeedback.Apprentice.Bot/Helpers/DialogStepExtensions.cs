using ESFA.ProvideFeedback.Apprentice.Bot.Services;

namespace ESFA.ProvideFeedback.Apprentice.Bot.Helpers
{
    public static class DialogStepExtensions
    {
        public static IDialogStep WithResponse(this IDialogStep option, string response)
        {
            option.Responses.Add(response);
            return option;
        }
    }
}