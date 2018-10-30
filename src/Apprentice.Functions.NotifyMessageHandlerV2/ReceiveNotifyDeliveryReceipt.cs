namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    using System;
    using System.IO;
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
        [FunctionName("ReceiveNotifyDeliveryReceipt")]
        // [return: ServiceBus("sms-delivery-log", Connection = "ServiceBusConnection")]
        public static ActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            var config = new SettingsProvider(context);

            log.LogInformation("ReceiveNotifyDeliveryReceipt trigger function processed a request.");

            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                log.LogInformation($"result: {data}");

                return data != null
                           ? (ActionResult)new OkObjectResult(data)
                           : new BadRequestObjectResult(
                               "Expecting a text message receipt payload. Ensure that the payload has an ID, reference, recipient, status and notification type");
            }
            catch (Exception e)
            {
                log.LogInformation($"Exception: {e.Message}");
                return new ExceptionResult(e, true);
            }
        }
    }
}