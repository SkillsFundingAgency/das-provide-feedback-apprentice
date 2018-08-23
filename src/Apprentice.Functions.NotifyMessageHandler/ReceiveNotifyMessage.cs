namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandler
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;

    using Newtonsoft.Json;

    public static class ReceiveNotifyMessage
    {
        [FunctionName("ReceiveNotifyMessage")]
        [return: Queue("sms-received-messages")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequestMessage req,
            TraceWriter log,
            ExecutionContext context)
        {
            try
            {
                var config = new SettingsProvider(context);

                log.Info("ReceiveNotifyMessage trigger function processed a request.");

                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                dynamic json = JsonConvert.DeserializeObject(data);

                return json == null
                           ? req.CreateResponse(
                               HttpStatusCode.BadRequest,
                               "Expecting a text message receipt payload. Ensure that the payload has an ID, reference, recipient, status and notification type")
                           : req.CreateResponse(HttpStatusCode.OK, $"{json}");
            }
            catch (Exception e)
            {
                var message = $"Exception: {e.Message}";
                log.Info(message);
                return req.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }
    }
}