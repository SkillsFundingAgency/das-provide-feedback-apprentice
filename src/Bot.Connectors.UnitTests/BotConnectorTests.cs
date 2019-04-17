namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.UnitTests
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.UnitTests.Stubs;
    using ESFA.DAS.ProvideFeedback.Apprentice.Domain.Dto;
    using Microsoft.Extensions.Options;

    using NSubstitute;

    using Xunit;

    using DirectLineSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.DirectLine;

    public class BotConnectorTests : IDisposable
    {
        private readonly BotConnector connector;

        private readonly IOptions<DirectLineSettings> configuration;

        private readonly IDirectLineClient botClient;

        public BotConnectorTests()
        {
            this.configuration = Options.Create(
                new DirectLineSettings { BotClientAuthToken = "aa-bb-cc-dd", BotClientBaseAddress = "http://localhost" });

            this.botClient = Substitute.For<IDirectLineClient>();
            this.botClient.StartConversationAsync().ReturnsForAnyArgs(FakeDirectLineApi.FakeStartConversationAsync());

            this.connector = new BotConnector(this.botClient, this.configuration);
        }

        public class StartConversationTests : BotConnectorTests
        {
            public StartConversationTests()
            {
                this.botClient.StartConversationAsync()
                    .Returns(Task.Run(() => FakeDirectLineApi.FakeStartConversationAsync()));
            }

            [Fact]
            public async Task ShouldCreateNewConversationId()
            {
                // arrange
                var expected = "aaa-bbb-ccc-ddd";

                // act
                var sut = await this.connector.StartConversationAsync();
                var actual = sut.ConversationId;

                // assert
                Assert.Equal(expected, actual);
            }

            [Fact]
            public Task ShouldThrowExceptionIfApiResponseIsWonky()
            {
                // arrange
                this.botClient.StartConversationAsync()
                    .Returns(FakeDirectLineApi.FakeWonkyResponseAsync());

                // assert
                return Assert.ThrowsAsync<Exception>(() => this.connector.StartConversationAsync());
            }
        }

        public class PostToBotTests : BotConnectorTests
        {
            public PostToBotTests()
            {
            }

            [Theory]
            [InlineData("01234567890", "aaa-bbb-ccc-ddd", 0000004)]
            [InlineData("07798765432", "bbb-ccc-ddd-aaa", 0000002)]
            [InlineData("09876543210", "ccc-ddd-aaa-bbb", 0000100)]
            public async Task ShouldReturnCorrectConversationAndTurnId(string mobile, string expectedId, int expectedTurn)
            {
                // arrange
                var conversation = FakeDirectLineApi.Conversations.FirstOrDefault(c => c.Key == mobile);

                var conversationId = conversation.Value.ConversationId;
                var msg = FakeDirectLineApi.CreateMessage(mobile, "Hello Bertie!");

                this.botClient.PostToConversationAsync(conversationId, msg)
                    .Returns(FakeDirectLineApi.FakePostToConversationAsync(conversationId, msg));

                // act
                var sut = await this.connector.PostToBotAsync(conversation.Value.ConversationId, msg);
                var actual = sut;

                // assert
                Assert.Equal(expectedId, actual.ConversationId);
                Assert.Equal(expectedTurn, actual.Turn);
            }

            [Fact]
            public Task ShouldThrowExceptionIfConversationIdNotFound()
            {
                // arrange
                const string INVALID_MOBILE = "07709393939";
                const string INVALID_CONVERSATION_ID = "ddd-aaa-bbb-ccc";

                var conversationId = INVALID_CONVERSATION_ID;
                var msg = FakeDirectLineApi.CreateMessage(INVALID_MOBILE, "Hello Bertie!");

                this.botClient.PostToConversationAsync(conversationId, msg)
                    .Returns(FakeDirectLineApi.FakePostToConversationAsync(conversationId, msg));

                // assert
                return Assert.ThrowsAsync<HttpRequestException>(() => this.connector.PostToBotAsync(INVALID_CONVERSATION_ID, msg));
            }

            [Fact]
            public Task ShouldThrowExceptionIfApiResponseIsWonky()
            {
                // arrange
                const string VALID_MOBILE = "01234567890";
                const string VALID_CONVERSATION_ID = "aaa-bbb-ccc-ddd";

                var msg = FakeDirectLineApi.CreateMessage(VALID_MOBILE, "Hello Bertie!");

                this.botClient.PostToConversationAsync(Arg.Any<string>(), Arg.Any<BotMessage>())
                    .Returns(FakeDirectLineApi.FakeWonkyResponseAsync());

                // assert
                return Assert.ThrowsAsync<Exception>(() => this.connector.PostToBotAsync(VALID_CONVERSATION_ID, msg));
            }

        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.connector.Dispose();
        }
    }
}
