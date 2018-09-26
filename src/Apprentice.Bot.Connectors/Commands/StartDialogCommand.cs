using System;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    public sealed class StartDialogCommand : AdminCommand, IBotDialogCommand
    {
        //private readonly ILogger logger;

        //public StartDialogCommand(ILogger logger)
        //    : base("start")
        //{
        //    this.logger = logger;
        //}

        public StartDialogCommand() : base("start")
        {
            
        }

        public override async Task ExecuteAsync(DialogContext dc)
        {
            try
            {
                UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);
                userInfo.SurveyState = new SurveyState();

                string message = dc.Context.Activity.Text.ToLowerInvariant();

                var strings = message.Split(new[] { " ", "|" }, StringSplitOptions.RemoveEmptyEntries);

                if (strings.Length > 1)
                {
                    string dialogId = strings[1];
                    
                    userInfo.SurveyState.SurveyId = dialogId;
                    userInfo.SurveyState.StartDate = DateTime.Now;
                    userInfo.SurveyState.Progress = ProgressState.InProgress;

                    // TODO: check dialog collection
                    await dc.Begin(dialogId);
                }
                else
                {
                    // this.logger.LogError($"could not find dialogId in command \"{ message }\"");
                }
            }
            catch (Exception e)
            {
                // this.logger.LogError(e.Message);
#if DEBUG
                await dc.Context.SendActivity($"DEBUG: {e}");
#endif
            }
        }
    }
}