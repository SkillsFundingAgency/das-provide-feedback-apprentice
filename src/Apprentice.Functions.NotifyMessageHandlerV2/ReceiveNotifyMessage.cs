namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Web.Http;
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
            ICollector<string> queue,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("ReceiveNotifyMessage trigger function processed a request.");

            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                dynamic receivedSms = JsonConvert.DeserializeObject(requestBody);

                log.LogInformation($"Message received from {receivedSms?.source_number}");

                if (receivedSms == null)
                {
                    return new BadRequestObjectResult(
                        "Expecting a text message payload. Please see the Notify callback documentation for details: https://www.notifications.service.gov.uk/callbacks");
                }

                queue.Add(JsonConvert.SerializeObject(receivedSms));
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