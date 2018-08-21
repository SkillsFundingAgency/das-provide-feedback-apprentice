// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Education & Skills Funding Agency">
//   Copyright © 2018 Education & Skills Funding Agency
// </copyright>
// <summary>
//   The bot entry point.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ESFA.ProvideFeedback.Apprentice.Bot
{
    using System;

    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;

    using NLog;
    using NLog.Web;

    /// <summary>
    /// The bot entry point.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Create a new host for the bot to run under
        /// </summary>
        /// <param name="args">Arguments to use on startup</param>
        /// <returns>The <see cref="IWebHostBuilder"/></returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).UseStartup<Startup>().UseNLog();

        /// <summary>
        /// The main method
        /// </summary>
        /// <param name="args">command line arguments</param>
        public static void Main(string[] args)
        {
            Logger logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Info("Starting up Apprentice Feedback Bot host");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped Apprentice Feedback Bot because of exception");
                throw;
            }
        }
    }
}