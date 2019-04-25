using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware;
using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Moq;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Bot.Builder.Dialogs;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.UnitTests.Middleware
{
    public class ActivityIdMiddlewareTests
    {
        private ActivityIdMiddleware _sut;
        private FeedbackBotStateRepository _feedbackBotStateRepository;
        private readonly ConversationState _conversationState;
        private readonly UserState _testUserState;
        private DialogInstance _testCurrentActivityDialog;
        private IStorage _testStorage;
        private Activity _testActivity;
        private readonly Mock<UserState> _mockUserState;
        private Mock<ITurnContext> _mockTurnContext;
        private List<Activity> _testActivityList;
        private Mock<IStatePropertyAccessor<Microsoft.Bot.Builder.Dialogs.DialogState>> _mockConversationDialogState;

        private Mock<NextDelegate> _mockNext;

        public ActivityIdMiddlewareTests()
        {
            _testStorage = new TestStorage();
            _mockNext = new Mock<NextDelegate>();
            _conversationState = new ConversationState(_testStorage);
            _testUserState = new UserState(_testStorage);
            _mockUserState = new Mock<UserState>();
            _testCurrentActivityDialog = new DialogInstance { Id = "TestactivityId" };

            _mockConversationDialogState = new Mock<IStatePropertyAccessor<Microsoft.Bot.Builder.Dialogs.DialogState>>();
            _feedbackBotStateRepository = new FeedbackBotStateRepository(_conversationState, _testUserState);
            _feedbackBotStateRepository.ConversationDialogState =  _mockConversationDialogState.Object;

            _mockTurnContext = new Mock<ITurnContext>();
            _mockTurnContext
                .Setup(m => m.TurnState)
                .Returns(new TurnContextStateCollection());

            DialogState testState = new DialogState(new List<DialogInstance> { new DialogInstance { Id = "TestactivityId" } });

            var testDialogState = new DialogState(new List<DialogInstance>
                {
                    {
                        new DialogInstance
                        {
                            Id = Guid.NewGuid().ToString(),
                            State = new Dictionary<string, object>{
                                { "DialogState",
                                    new DialogState(new List<DialogInstance> { _testCurrentActivityDialog } )
                                }
                            }
                        }
                }
            });

            _mockConversationDialogState
                .Setup(m => m.GetAsync(_mockTurnContext.Object, It.IsAny<Func<DialogState>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(testDialogState);

            _testActivityList = new List<Activity>();

            // set up the test activity with some random text and known ChannelId and ConversationId
            _testActivity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
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


            _sut = new ActivityIdMiddleware(_feedbackBotStateRepository);
        }

        public class OnTurnAsync : ActivityIdMiddlewareTests
        {

            [Fact]
            public async Task WhenCalledWithAMessageActivityWithoutAnActivityId_ThenTheActivityIdIsUpdatedFromTheDialogState()
            {
                // arrange  
                _testActivity.Id = null;
                var newId = Guid.NewGuid().ToString();
                _testCurrentActivityDialog.Id = newId;

                // act
                await _sut.OnTurnAsync(_mockTurnContext.Object, _mockNext.Object);

                //assert
                Assert.Equal(newId, _testActivity.Id);
            }

            [Fact]
            public async Task WhenCalledWithAMessageActivityWithAnActivityId_ThenTheActivityIdIsNotUpdated()
            {
                // arrange  
                _testActivity.Id = "DoNotChange";

                // act
                await _sut.OnTurnAsync(_mockTurnContext.Object, _mockNext.Object);

                //assert
                Assert.Equal("DoNotChange", _testActivity.Id);
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
            public async Task WhenCalledWithAMessageActivityThatIsNotAMessageType_ThenTheActivityIdIsNotUpdated(string activityType)
            {
                // arrange
                _testActivity.Id = null;
                _testActivity.Type = activityType;

                // act
                await _sut.OnTurnAsync(_mockTurnContext.Object, _mockNext.Object);

                //assert
                Assert.Null(_testActivity.Id);
            }

            [Fact]
            public async Task WhenCalledWithAMessageActivityWithNoContent_ThenTheActivityIdIsNotUpdated()
            {
                // arrange
                _testActivity.Id = null;
                _testActivity.Text = string.Empty; // no content

                // act
                await _sut.OnTurnAsync(_mockTurnContext.Object, _mockNext.Object);

                //assert
                Assert.Null(_testActivity.Id);
            }
        }
    }
}
