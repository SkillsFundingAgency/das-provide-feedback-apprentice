namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.Helpers
{
    using System;
    using System.Threading.Tasks;

    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;

    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.WindowsAzure.Storage.Queue;

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
                string dependencyName = queue.Uri.Host + queue.Uri.AbsolutePath;
                telemetry.TrackDependency("Azure Queue", dependencyName, payload, startTime, timer.Elapsed, success);
            }
        }

        /// <summary>
        ///     Creates a new Application Insights <see cref="TelemetryClient"/>
        /// </summary>
        /// <param name="instrumentationKey"> the instrumentation key for the Application Insights instance </param>
        /// <param name="functionName"> the name of the function </param>
        /// <param name="functionInstance"> the unique instance ID of the function </param>
        /// <param name="functionVersion"> the version number of the function </param>
        /// <returns> A <see cref="TelemetryClient"/> </returns>
        public static TelemetryClient CreateApplicationInsightsClient(
            string instrumentationKey,
            string functionName,
            string functionInstance,
            string functionVersion)
        {
            try
            {
                TelemetryConfiguration config = new TelemetryConfiguration(instrumentationKey);

                return new TelemetryClient(config)
                           {
                               Context =
                                   {
                                       Component = { Version = functionVersion },
                                       Cloud =
                                           {
                                               RoleName = functionName,
                                               RoleInstance = functionInstance
                                           }
                                   }
                           };
            }
            catch (System.ArgumentNullException ex)
            {
                throw new ApplicationInsightsClientException("An error occurred while trying to create a client.", ex);
            }
            catch (System.ArgumentException ex)
            {
                throw new ApplicationInsightsClientException("An error occurred while trying to create a client.", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationInsightsClientException("An error occurred while trying to create a client.", ex);
            }
        }
    }
}