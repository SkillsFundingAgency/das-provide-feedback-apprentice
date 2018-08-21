using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Queue;

namespace ESFA.ProvideFeedback.Apprentice.NotifyMessageHandler.Helpers
{
    public static class FunctionsHelper
    {
        public static async Task AddQueueMessageAsync(CloudQueue queue, string payload, TelemetryClient telemetry)
        {
            var success = false;
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var message = new CloudQueueMessage(payload);
                var ttl = TimeSpan.FromMinutes(30);
                await queue.AddMessageAsync(message, ttl, null, null, null);
                success = true;
            }
            finally
            {
                timer.Stop();
                var dependencyName = queue.Uri.Host + queue.Uri.AbsolutePath;
                telemetry.TrackDependency(dependencyName, "AddQueueMessageAsync", startTime, timer.Elapsed, success);
            }
        }

        public static TelemetryClient CreateApplicationInsightsClient(ISettingService settingsProvider, ExecutionContext functionContext)
        {
            var instrumentationKey = settingsProvider.Get("APPINSIGHTS_INSTRUMENTATIONKEY");
            var config = new TelemetryConfiguration(instrumentationKey);

            return new TelemetryClient(config)
            {
                Context =
                {
                    Component = { Version = "1.0.0" },
                    Cloud = { RoleName = functionContext.FunctionName, RoleInstance = functionContext.InvocationId.ToString() }
                }
            };
        }
    }
}