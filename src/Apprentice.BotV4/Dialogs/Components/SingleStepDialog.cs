using System.Linq;
using System.Text;
using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Helpers;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
using Microsoft.Bot.Schema;

namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Dialogs.Components
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Models;

    using Microsoft.Bot.Builder.Dialogs;

    public class DialogConfiguration
    {
        public bool CollateResponses { get; set; } = true;
        public bool RealisticTypingDelay { get; set; } = true;
        public int CharactersPerMinute { get; set; } = 600;
        public int ThinkingTimeDelayMs { get; set; } = 1000;
    }

    public abstract class SingleStepDialog : DialogContainer
    {
        private readonly DialogConfiguration _configuration;

        protected SingleStepDialog(string id)
            : base(id)
        {
            _configuration = new DialogConfiguration();
        }

        public DialogConfiguration Configuration => _configuration;

        public ICollection<IResponse> Responses { get; protected set; } = new List<IResponse>();

        public SingleStepDialog AddResponse(IResponse response)
        {
            this.Responses.Add(response);
            return this;
        }

        public SingleStepDialog Build()
        {
            var steps = new WaterfallStep[]
                            {
                                async (dc, args, next) => { await this.Step(dc, args, next); },
                                async (dc, args, next) => { await dc.End(); }
                            };

            this.Dialogs.Add(this.DialogId, steps);

            return this;
        }

        public SingleStepDialog WithResponses(ICollection<IResponse> responses)
        {
            this.Responses = responses;
            return this;
        }

        protected abstract Task Step(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next);

        protected async Task RespondAsMultipleMessages(IEnumerable<IResponse> responses, DialogContext dc, UserInfo userInfo)
        {
            foreach (IResponse r in responses)
            {
                if (r is PredicateResponse predicatedResponse && !predicatedResponse.IsValid(userInfo))
                {
                    continue;
                }

                if (Configuration.RealisticTypingDelay)
                {
                    await Task.Delay(Configuration.ThinkingTimeDelayMs + (r.Prompt.Length / Configuration.CharactersPerMinute));
                    await dc.Context.SendTypingActivity(r.Prompt);
                }

                await dc.Context.SendActivity(r.Prompt, InputHints.IgnoringInput);
            }
        }

        protected async Task RespondAsSingleMessage(IEnumerable<IResponse> responses, DialogContext dc, UserInfo userInfo)
        {
            StringBuilder sb = new StringBuilder();
            foreach (IResponse r in responses)
            {
                if (r is PredicateResponse predicatedResponse && !predicatedResponse.IsValid(userInfo))
                {
                    continue;
                }

                sb.AppendLine(r.Prompt);
            }
            
            var response = sb.ToString();

            if (Configuration.RealisticTypingDelay)
            {
                await Task.Delay(Configuration.ThinkingTimeDelayMs + (response.Length / Configuration.CharactersPerMinute));
                await dc.Context.SendTypingActivity(response);
            }

            await dc.Context.SendActivity(response, InputHints.IgnoringInput);
        }
    }
}