namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Web.Http;
    using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public static class ReceiveNotifyDeliveryReceipt
    {
        // TODO: [security] hash the incoming phone number
        [FunctionName("ReceiveNotifyDeliveryReceipt")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            [ServiceBus("sms-delivery-log", Connection = "ServiceBusConnection", EntityType = Microsoft.Azure.WebJobs.ServiceBus.EntityType.Queue)]
            ICollector<string> queue,
            ILogger log,
            ExecutionContext context)
        {
            var config = new SettingsProvider(context);

            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                dynamic deliveryReceipt = JsonConvert.DeserializeObject(requestBody);

                log.LogInformation($"Message to {deliveryReceipt?.to} has an updated status: {deliveryReceipt?.status}");

                if (deliveryReceipt == null)
                {
                    return new BadRequestObjectResult(
                        "Expecting a text message receipt payload. Ensure that the payload has an ID, reference, recipient, status and notification type");
                }

                queue.Add(JsonConvert.SerializeObject(deliveryReceipt));
                return new OkObjectResult(deliveryReceipt);
            }
            catch (Exception e)
            {
                log.LogError($"ReceiveNotifyDeliveryReceipt ERROR", e, e.Message);
                return new ExceptionResult(e, true);
            }
        }
    }
}