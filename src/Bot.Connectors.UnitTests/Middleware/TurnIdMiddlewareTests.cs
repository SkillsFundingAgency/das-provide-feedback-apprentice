using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware;
using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Moq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.UnitTests.Middleware
{
    public class TurnIdMiddlewareTests
    {
        private TurnIdMiddleware _sut;
        private FeedbackBotStateRepository _feedbackBotStateRepository;
        private readonly ConversationState _conversationState;
        private readonly UserState _userState;
        private IStorage _testStorage;
        private Activity _testActivity;
        private readonly Mock<UserState> _mockUserState;
        private Mock<ITurnContext> _mockTurnContext;
        private List<Activity> _testActivityList;
        private Mock<IStatePropertyAccessor<Microsoft.Bot.Builder.Dialogs.DialogState>> _mockConversationDialogState;

        private Mock<NextDelegate> _mockNext;

        public TurnIdMiddlewareTests()
        {
            _testStorage = new TestStorage();
            _mockNext = new Mock<NextDelegate>();
            _conversationState = new ConversationState(_testStorage);
            _userState = new UserState(_testStorage);
            _mockUserState = new Mock<UserState>();

            _mockConversationDialogState = new Mock<IStatePropertyAccessor<Microsoft.Bot.Builder.Dialogs.DialogState>>();
            _feedbackBotStateRepository = new FeedbackBotStateRepository(_conversationState, _userState);
            _feedbackBotStateRepository.ConversationDialogState = _mockConversationDialogState.Object;

            _mockTurnContext = new Mock<ITurnContext>();
            _mockTurnContext
                .Setup(m => m.TurnState)
                .Returns(new TurnContextStateCollection());

            _testActivityList = new List<Activity>();

            // set up the test activity with some random text and known ChannelId and ConversationId
            _testActivity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = Guid.NewGuid().ToString(),
                ChannelId = "Test",
                Conversation = new ConversationAccount() { Id = "TestConversation" }
            };

            _testActivityList.Add(_testActivity);

            _mockTurnContext
                .Setup(m => m.Activity)
                .Returns(_testActivity);

            _mockTurnContext
                .Setup(m => m.OnSendActivities(It.IsAny<SendActivitiesHandler>()))
                .Callback<SendActivitiesHandler>(async (handler) =>
                {
                    await handler.Invoke(_mockTurnContext.Object, _testActivityList, () => Task.FromResult(new List<ResourceResponse>().ToArray()));
                });

            _sut = new TurnIdMiddleware(_feedbackBotStateRepository);
        }

        public class OnTurnAsync : TurnIdMiddlewareTests
        {
            [Fact]
            public async Task WhenCalledWithAMessageActivityWithoutATurnId_ThenTheTurnIdIsInitialised()
            {
                // arrange  

                // act
                await _sut.OnTurnAsync(_mockTurnContext.Object, _mockNext.Object);

                //assert
                await AssertTurnIdInTestStorage(1);
            }

            [Fact]
            public async Task WhenCalledWithAMessageActivityWithATurnId_ThenTheTurnIdIsIncremented()
            {
                // arrange  
                long turnId = 3;

                await InitialiseTurnIdInTestStorage(turnId);

                // act
                await _sut.OnTurnAsync(_mockTurnContext.Object, _mockNext.Object);

                //assert
                await AssertTurnIdInTestStorage(++turnId);
            }

            [Theory]
            [InlineData(ActivityTypes.ContactRelationUpdate)]
            [InlineData(ActivityTypes.ConversationUpdate)]
            [InlineData(ActivityTypes.Typing)]
            [InlineData(ActivityTypes.EndOfConversation)]
            [InlineData(ActivityTypes.Event)]
            [InlineData(ActivityTypes.Invoke)]
            [InlineData(ActivityTypes.DeleteUserData)]
            [InlineData(ActivityTypes.MessageUpdate)]
            [InlineData(ActivityTypes.MessageDelete)]
            [InlineData(ActivityTypes.InstallationUpdate)]
            [InlineData(ActivityTypes.MessageReaction)]
            [InlineData(ActivityTypes.Suggestion)]
            [InlineData(ActivityTypes.Trace)]
            [InlineData(ActivityTypes.Handoff)]
            public async Task WhenCalledWithAMessageActivityThatIsNotAMessageType_ThenTheTurnIdIsNotIncreased(string activityType)
            {
                // arrange
                long turnId = 4;
                _testActivity.Type = activityType;

                await InitialiseTurnIdInTestStorage(turnId);

                // act
                await _sut.OnTurnAsync(_mockTurnContext.Object, _mockNext.Object);

                //assert
                await AssertTurnIdInTestStorage(turnId);
            }

            [Fact]
            public async Task WhenCalledWithAMessageActivityWithNoContent_ThenTheTurnIdIsNotIncremented()
            {
                // arrange
                long turnId = 6;
                _testActivity.Text = string.Empty;

                await InitialiseTurnIdInTestStorage(turnId);

                // act
                await _sut.OnTurnAsync(_mockTurnContext.Object, _mockNext.Object);

                //assert
                await AssertTurnIdInTestStorage(turnId);
            }
        }

        private async Task InitialiseTurnIdInTestStorage(long value)
        {
            await _testStorage.WriteAsync(new Dictionary<string, object>
            {
                { $"{_testActivity.ChannelId}/conversations/{_testActivity.Conversation.Id}",
                    new Dictionary<string, object>
                    {
                        { "turnId", value }
                    }
                }
            });
        }

        private async Task AssertTurnIdInTestStorage(long expected)
        {
            var storedData = await _testStorage.ReadAsync(new[] { $"{_testActivity.ChannelId}/conversations/{_testActivity.Conversation.Id}" });
            Assert.Equal(1, storedData.Count);
            Assert.Equal(1, storedData.Values.Count);
            var properties = storedData.Values.First() as IDictionary<string, object>;
            Assert.Equal(1, properties.Count);
            Assert.Equal(expected, properties.Where(i => i.Key.Equals("turnId")).First().Value);
        }
    }
}
