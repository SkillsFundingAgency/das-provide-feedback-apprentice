using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.ProvideFeedback.ApprenticeBot.Models;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace ESFA.ProvideFeedback.ApprenticeBot
{
    public class ApprenticeBot : IBot
    {
        private readonly DialogSet _dialogs;

        public ApprenticeBot(IApprenticeFeedbackSurvey feedbackDialogSet)
        {
            _dialogs = feedbackDialogSet.Current();
        }

        /// <inheritdoc />
        public async Task OnTurn(ITurnContext context)
        {
            try
            {
                switch (context.Activity.Type)
                {
                    case ActivityTypes.Message:
                        var state = ConversationState<Dictionary<string, object>>.Get(context);
                        var dc = _dialogs.CreateContext(context, state);

                        if (context.Activity.Text.ToLowerInvariant().Contains("stop"))
                        {
                            await dc.Context.SendActivity($"Feedback cancelled");
                            dc.EndAll();
                        }
                        else if (context.Activity.Text.ToLowerInvariant().Contains("feedback"))
                        {
                            dc.EndAll();
                            await dc.Begin("start");
                        }
                        else
                        {
                            await dc.Continue();

                            if (!context.Responded)
                            {
                                await dc.Begin("start");
                            }
                        }

                        break;

                    case ActivityTypes.ConversationUpdate:
                        foreach (var newMember in context.Activity.MembersAdded)
                        {
                            if (newMember.Id != context.Activity.Recipient.Id)
                            {
                                await context.SendActivity($"Hello! I'm Bertie the Apprentice Feedback Bot. Please reply with 'help' if you would like to see a list of my capabilities");
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                await context.SendActivity($"Exception: {e.Message}");
            }
        }
    }
}
