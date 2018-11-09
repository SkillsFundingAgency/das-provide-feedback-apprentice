using System.Data;
using System.Data.SqlClient;
using System.IO;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2;
using ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2.DependecyInjection.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(Startup))]
namespace ESFA.DAS.ProvideFeedback.Apprentice.Functions.NotifyMessageHandlerV2
{
    internal class Startup : IWebJobsStartup
    {
        private readonly IConfigurationRoot _configuration;
        public Startup()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public void Configure(IWebJobsBuilder builder) =>
            builder.AddDependencyInjection(ConfigureServices);

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDbConnection>(c => new SqlConnection(_configuration.GetConnectionStringOrSetting("SqlConnectionString")));
            services.AddScoped<IStoreApprenticeSurveyInvites, ApprenticeSurveyInvitesRepository>();
        }
    }
}
