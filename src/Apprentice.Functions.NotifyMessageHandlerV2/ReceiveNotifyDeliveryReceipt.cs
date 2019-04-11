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

    public static class ReceiveNotifyDeliveryReceipt
    {
        // TODO: [security] hash the incoming phone number
        [FunctionName("ReceiveNotifyDeliveryReceipt")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            [ServiceBus("sms-delivery-log", Connection = "ServiceBusConnection", EntityType = Microsoft.Azure.WebJobs.ServiceBus.EntityType.Queue)]
            IAsyncCollector<SmsDeliveryReceipt> queue,
            ILogger log,
            ExecutionContext context)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                SmsDeliveryReceipt deliveryReceipt = JsonConvert.DeserializeObject<SmsDeliveryReceipt>(requestBody);

                log.LogInformation($"{deliveryReceipt.Status}: (to: {deliveryReceipt.To} uuid: {deliveryReceipt.Reference}");

                if (deliveryReceipt.Id == null)
                {
                    return new BadRequestObjectResult(
                        "Expecting a text message receipt payload. Ensure that the payload has an ID, reference, recipient, status and notification type");
                }

                await queue.AddAsync(deliveryReceipt);
                return new OkObjectResult(deliveryReceipt);
            }
            catch (MessageLockLostException e)
            {
                log.LogError($"ReceiveNotifyDeliveryReceipt MessageLockLostException [{context.FunctionName}|{context.InvocationId}]", e, e.Message);
                return new ExceptionResult(e, true);
            }
            catch (Exception e)
            {
                log.LogError($"ReceiveNotifyDeliveryReceipt ERROR: {e.Message}", e, e.Message);
                return new ExceptionResult(e, true);
            }
        }
    }
}