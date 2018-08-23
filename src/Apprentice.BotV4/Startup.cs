namespace ESFA.DAS.ProvideFeedback.Apprentice.BotV4
{
    using System;
    using System.IO;

    using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.State;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Bot.Builder.BotFramework;
    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Builder.TraceExtensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

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
                        var path = Path.Combine(Path.GetTempPath(), nameof(FeedbackBot));
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