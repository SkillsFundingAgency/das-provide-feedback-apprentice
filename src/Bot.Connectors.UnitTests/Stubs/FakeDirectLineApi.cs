namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.UnitTests.Stubs
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Conversation;
    using ESFA.DAS.ProvideFeedback.Apprentice.Domain.Dto;

    internal static class FakeDirectLineApi
    {
        internal static HttpResponseMessage FakeStartConversationAsync()
        {
            HttpResponseMessage response =
                new HttpResponseMessage(HttpStatusCode.Accepted)
                    {
                        Content = new StringContent($"{{ \"conversationId\": \"aaa-bbb-ccc-ddd\" }}")
                    };
            return response;
        }

        internal static HttpResponseMessage FakePostToConversationAsync(string conversationId, BotMessage message)
        {
            var conversation = Conversations.FirstOrDefault(c => c.Value.ConversationId == conversationId);
            if (conversation.Value == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent($"{{ \"error\": {{ \"code\": \"BadArgument\", \"message\": \"Unknown conversation\" }} }}")
                    };
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.Accepted)
                    {
                        Content = new StringContent($"{{ \"id\": \"{conversation.Value.ConversationId}|{ conversation.Value.TurnId + 1:D7}\" }}")
                    };
            }
        }

        internal static HttpResponseMessage FakeWonkyResponseAsync()
        {
            return new HttpResponseMessage(HttpStatusCode.Accepted)
                {
                    Content = new StringContent($"{{ \"guff\": {{ \"wibble\": \"yes\", \"wobble\": \"no\" }} }}")
                };
        }

        internal static Dictionary<string, BotConversation> Conversations
        {
            get
            {
                var d = new Dictionary<string, BotConversation>
                    {
                        { "01234567890", new BotConversation() { UserId = "01234567890", ConversationId = "aaa-bbb-ccc-ddd", TurnId = 3 } },
                        { "07798765432", new BotConversation() { UserId = "01234567890", ConversationId = "bbb-ccc-ddd-aaa", TurnId = 1 } },
                        { "09876543210", new BotConversation() { UserId = "01234567890", ConversationId = "ccc-ddd-aaa-bbb", TurnId = 99 } },
                    };

                return d;
            }
        }

        internal static BotMessage CreateMessage(string phoneNumber, string message)
        {
            var sms = new IncomingSms()
                {
                    DateReceived =
                        DateTime.UtcNow,
                    DestinationNumber = "00000000000",
                    SourceNumber = phoneNumber,
                    Message = message
                };

            var msg = new BotMessage();

            dynamic channelData = new ExpandoObject();
            channelData.NotifyMessage = sms;

            dynamic from = new ExpandoObject();
            from.Id = sms.SourceNumber;
                
            msg.ChannelData = channelData;
            msg.From = from;

            return msg;
        }

    }
}