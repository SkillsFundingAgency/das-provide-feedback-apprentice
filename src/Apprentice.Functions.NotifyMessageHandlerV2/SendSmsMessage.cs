namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.DependecyInjection;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands.SendSms;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public static class SendSmsMessage
    {        
        [FunctionName("SendSmsMessage")]
        public static async Task Run(
        [ServiceBusTrigger("sms-outgoing-messages", Connection = "ServiceBusConnection")]
        Message queueMessage,
        ILogger log,
        [Inject]ICommandHandlerAsync<SendSmsCommand> commandHandler)
        {
            try
            {
                var outgoingSms = JsonConvert.DeserializeObject<OutgoingSms>(Encoding.UTF8.GetString(queueMessage.Body));

                await commandHandler.HandleAsync(new SendSmsCommand(outgoingSms, queueMessage));
            }           
            catch (Exception e)
            {
                log.LogError($"SendSmsMessage ERROR", e, e.Message);
                throw new BotConnectorException("Something went wrong when relaying the message to the Notify client", e);
            }
        }
    }
}