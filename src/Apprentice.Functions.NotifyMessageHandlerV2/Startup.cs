using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2;

using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(Startup))]

namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;

    using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
    using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.DependecyInjection.Config;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    internal class Startup : IWebJobsStartup
    {
        private readonly IConfigurationRoot configuration;

        public Startup()
        {
            this.configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public void Configure(IWebJobsBuilder builder) => builder.AddDependencyInjection(this.ConfigureServices);

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDbConnection>(c => new SqlConnection(this.configuration.GetConnectionStringOrSetting("SqlConnectionString")));
            services.AddScoped<IStoreApprenticeSurveyDetails, ApprenticeSurveyInvitesRepository>();
        }
    }
}