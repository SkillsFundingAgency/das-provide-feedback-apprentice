namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4
{
    using System;
    using System.Globalization;
    using System.IO;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Commands;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Configuration;
    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.State;
    using ESFA.DAS.ProvideFeedback.Apprentice.Infrastructure.Configuration;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Bot.Builder.BotFramework;
    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Builder.TraceExtensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    using AzureConfiguration = Infrastructure.Configuration.Azure;
    using BotConfiguration = Infrastructure.Configuration.Bot;
    using DataConfiguration = Infrastructure.Configuration.Data;
    using NotifyConfiguration = Infrastructure.Configuration.Notify;

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true).AddEnvironmentVariables();

            this.Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles().UseStaticFiles().UseBotFramework();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () =>
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                    return settings;
                };

            // add config options
            services.AddOptions()
                .BindConfiguration<AzureConfiguration>(this.Configuration)
                .BindConfiguration<BotConfiguration>(this.Configuration)
                .BindConfiguration<ConnectionStrings>(this.Configuration)
                .BindConfiguration<DataConfiguration>(this.Configuration)
                .BindConfiguration<NotifyConfiguration>(this.Configuration);

            // add & configure localization
            services.AddLocalization(options => options.ResourcesPath = "Resources")
                .Configure<RequestLocalizationOptions>(
                    options =>
                        {
                            var supportedCultures = new[] { new CultureInfo("en-GB") };
                            options.DefaultRequestCulture = new RequestCulture("en-GB");
                            options.SupportedCultures = supportedCultures;
                            options.SupportedUICultures = supportedCultures;
                        });

            // TODO: invoke a list resolver for this
            services.AddSingleton<IBotDialogCommand, ExpireCommand>();
            services.AddSingleton<IBotDialogCommand, AdminHelpCommand>();
            services.AddSingleton<IBotDialogCommand, OptOutCommand>();
            services.AddSingleton<IBotDialogCommand, ResetDialogCommand>();
            services.AddSingleton<IBotDialogCommand, StartDialogCommand>();
            services.AddSingleton<IBotDialogCommand, StatusCommand>();

            services.AddBot<FeedbackBot>(
                options =>
                    {
                        options.CredentialProvider = new ConfigurationCredentialProvider(this.Configuration);
                        options.Middleware.Add(
                            new CatchExceptionMiddleware<Exception>(
                                async (context, exception) =>
                                    {
                                        await context.TraceActivity($"{nameof(FeedbackBot)} Exception", exception);
#if DEBUG
                                        await context.SendActivity($"Sorry, it looks like something went wrong! {exception.Message}");
#endif
                                    }));

                        // Add state management middleware for conversation and user state.
                        string path = Path.Combine(Path.GetTempPath(), nameof(FeedbackBot));
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        IStorage storage = new FileStorage(path);

                        options.Middleware.Add(new ConversationState<ConversationInfo>(storage));
                        options.Middleware.Add(new UserState<UserInfo>(storage));
                    });
        }
    }
}