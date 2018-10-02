namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Commands
{
    using System;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;

    using Microsoft.Azure.ServiceBus;
    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Dialogs;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class SmsConversationTrigger
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("source_number")]
        public string SourceNumber { get; set; }

        [JsonProperty("destination_number")]
        public string DestinationNumber { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("date_received")]
        public DateTime TimeStamp { get; set; }
    }

    public sealed class ApprenticeFeedbackSecretTrigger : AdminCommand, IBotDialogCommand
    {
        public ApprenticeFeedbackSecretTrigger() : base("I like avocado")
        {

        }

        public override async Task ExecuteAsync(DialogContext dc)
        {
            var dialogId = "afb-v3";

            UserInfo userInfo = UserState<UserInfo>.Get(dc.Context);
            userInfo.SurveyState = new SurveyState
                                       {
                                           SurveyId = dialogId, StartDate = DateTime.Now, Progress = ProgressState.InProgress
                                       };


            await dc.Begin(dialogId);
        }
    }
}