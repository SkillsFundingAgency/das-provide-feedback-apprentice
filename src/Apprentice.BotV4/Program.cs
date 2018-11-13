namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4
{
    using System;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;
    using NLog.Web;

    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                BuildWebHost(args).Run();
            }
            catch (Exception ex)
            {
                // NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    // Add Azure Logging
                    logging.AddAzureWebAppDiagnostics();

                    // Logging Options.
                    // There are other logging options available:
                    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1
                    // logging.AddDebug();
                    // logging.AddConsole();
                })
                .UseApplicationInsights()
                .UseStartup<Startup>()
                .Build();
    }
}
