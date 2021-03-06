﻿namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;
    using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    /// <inheritdoc />
    public class ConversationLogMiddleware : IMiddleware
    {        
        private readonly IConversationLogRepository conversationRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationLogMiddleware"/> class. 
        /// </summary>
        /// <param name="conversationRepository">Conversation storage</param>
        public ConversationLogMiddleware(IConversationLogRepository conversationRepository)
        {
            this.conversationRepository = conversationRepository;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ConversationLogMiddleware"/> class. 
        /// </summary>
        ~ConversationLogMiddleware()
        {
            // cleanup
        }

        /// <inheritdoc />
        public async Task OnTurnAsync(
            ITurnContext context,
            NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            string botReply = string.Empty;

            if (context.Activity.Type == ActivityTypes.Message)
            {
                // Create a send activity handler to grab all response activities 
                // from the activity list.
                context.OnSendActivities(
                    async (activityContext, activityList, activityNext) =>
                    {
                        if (activityList.Any())
                        {
                            botReply = string.Join("\n\n", activityList.Select(a => a.Text));
                        }
                        return await activityNext();
                    });
            }

            // Pass execution on to the next layer in the pipeline.
            await next(cancellationToken);

            // Save logs for each conversational exchange only.
            if (context.Activity.Type == ActivityTypes.Message)
            {
                // Build a log object to write to the database.
                var logData = new ConversationLog
                {
                    From = context.Activity.From,
                    Recipient = context.Activity.Recipient,
                    Conversation = context.Activity.Conversation,
                    ChannelData = context.Activity.ChannelData,
                    ChannelId = context.Activity.ChannelId,
                    Time = DateTime.Now.ToString(
                            CultureInfo.InvariantCulture),
                    Message = context.Activity.Text,
                    Reply = botReply
                };

                // Write our log to the database.
                try
                {
                    await this.conversationRepository.Save(logData);
                }
                catch (Exception ex)
                {
                    // More logic for what to do on a failed write can be added here
                    throw ex;
                }
            }
        }
    }
}