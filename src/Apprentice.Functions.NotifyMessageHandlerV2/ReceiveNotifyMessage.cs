namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Web.Http;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public static class ReceiveNotifyMessage
    {
        // TODO: [security] hash the incoming phone number
        [FunctionName("ReceiveNotifyMessage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            [ServiceBus("sms-incoming-messages", Connection = "ServiceBusConnection", EntityType = Microsoft.Azure.WebJobs.ServiceBus.EntityType.Queue)]
            IAsyncCollector<IncomingSms> queue,
            ILogger log,
            ExecutionContext context)
        {
            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                IncomingSms receivedSms = JsonConvert.DeserializeObject<IncomingSms>(requestBody);
                receivedSms.Type = SmsType.NotifySms;

                log.LogInformation($"Message received from {receivedSms.SourceNumber}");

                if (receivedSms.Message == null || receivedSms.DestinationNumber == null || receivedSms.SourceNumber == null)
                {
                    return new BadRequestObjectResult(
                        "Expecting a text message payload. Please see the Notify callback documentation for details: https://www.notifications.service.gov.uk/callbacks");
                }

                await queue.AddAsync(receivedSms);
                return new OkObjectResult(receivedSms);
            }
            catch (MessageLockLostException e)
            {
                log.LogError($"ReceiveNotifyMessage MessageLockLostException [{context.FunctionName}|{context.InvocationId}]", e, e.Message);
                return new ExceptionResult(e, true);
            }
            catch (Exception e)
            {
                log.LogInformation($"Exception: {e.Message}");
                return new ExceptionResult(e, true);
            }
        }
    }
}