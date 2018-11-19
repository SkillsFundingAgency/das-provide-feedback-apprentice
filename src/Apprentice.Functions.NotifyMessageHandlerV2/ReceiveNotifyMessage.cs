namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    using System;
    using System.IO;
    using System.Web.Http;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public static class ReceiveNotifyMessage
    {
        // TODO: [security] hash the incoming phone number
        [FunctionName("ReceiveNotifyMessage")]
        [return: ServiceBus("sms-incoming-messages", Connection = "ServiceBusConnection")]
        public static ActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("ReceiveNotifyMessage trigger function processed a request.");

            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                dynamic receivedSms = data?.Value;

                log.LogInformation($"Message received from {receivedSms?.source_number}");

                return receivedSms != null
                    ? (ActionResult)new OkObjectResult(receivedSms)
                    : new BadRequestObjectResult(
                        "Expecting a text message payload. Please see the Notify callback documentation for details: https://www.notifications.service.gov.uk/callbacks");
            }
            catch (Exception e)
            {
                log.LogInformation($"Exception: {e.Message}");
                return new ExceptionResult(e, true);
            }
        }
    }
}