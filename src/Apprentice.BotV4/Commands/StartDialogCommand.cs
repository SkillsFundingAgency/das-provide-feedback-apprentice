namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands
{
    using System;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.State;

    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Extensions.Logging;

    public sealed class StartDialogCommand : AdminCommand
    {
        private readonly ILogger<FeedbackBot> logger;

        public StartDialogCommand(ILogger<FeedbackBot> logger)
            : base("start")
        {
            this.logger = logger;
        }

        public override async Task ExecuteAsync(DialogContext dc)
        {
            try
            {
                UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);
                userInfo.SurveyState = new SurveyState();

                string message = dc.Context.Activity.Text.ToLowerInvariant();

                var strings = message.Split(" ", StringSplitOptions.RemoveEmptyEntries);

                if (strings.Length > 1)
                {
                    string dialogId = strings[1];
                    
                    userInfo.SurveyState.SurveyId = dialogId;
                    userInfo.SurveyState.StartDate = DateTime.Now;
                    userInfo.SurveyState.Progress = ProgressState.Enagaged;

                    await dc.Begin(dialogId);
                }
                else
                {
                    this.logger.LogError($"could not find dialogId in command \"{ message }\"");
                }
            }
            catch (Exception e)
            {
                this.logger.LogError(e.Message);
#if DEBUG
                await dc.Context.SendActivity($"DEBUG: {e}");
#endif
            }
        }
    }
}