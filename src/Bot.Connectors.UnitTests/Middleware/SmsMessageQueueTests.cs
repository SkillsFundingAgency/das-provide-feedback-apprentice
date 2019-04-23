using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Middleware;
using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;
using ESFA.DAS.ProvideFeedback.Apprentice.Domain.Dto;
using ESFA.DAS.ProvideFeedback.Apprentice.Services;
using Microsoft.Azure.ServiceBus;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.UnitTests.Middleware
{
    public class SmsMessageQueueTests
    {
        private SmsMessageQueue _sut;
        private readonly Mock<ISmsQueueProvider> _mockSmsQueueProvider;
        private readonly Mock<IOptions<Core.Configuration.Notify>> _mockNotifyOptions;

        private FeedbackBotStateRepository _feedbackBotStateRepository;
        private readonly ConversationState _conversationState;
        private readonly UserState _userState;
        private IStorage _testStorage;
        private Activity _testActivity;
        private string _testOutgoingQueueName;
        private readonly Mock<UserState> _mockUserState;
        private Mock<ITurnContext> _mockTurnContext;
        private List<Activity> _testActivityList;
        private Mock<IStatePropertyAccessor<Microsoft.Bot.Builder.Dialogs.DialogState>> _mockConversationDialogState;

        private Mock<NextDelegate> _mockNext;

        public SmsMessageQueueTests()
        {
            _testStorage = new TestStorage();
            _mockSmsQueueProvider = new Mock<ISmsQueueProvider>();
            _mockNotifyOptions = new Mock<IOptions<Core.Configuration.Notify>>();

            _testOutgoingQueueName = Guid.NewGuid().ToString();
            var testNotify = new Core.Configuration.Notify { OutgoingMessageQueueName = _testOutgoingQueueName };            
            _mockNotifyOptions.Setup(m => m.Value).Returns(testNotify);

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
                Id = Guid.NewGuid().ToString(),
                Type = ActivityTypes.Message,
                Text = Guid.NewGuid().ToString(),
                ChannelId = "Test",
                Conversation = new ConversationAccount() { Id = "TestConversation" },
                From = new ChannelAccount { Id = "TestFromAccount" },
                Recipient = new ChannelAccount { Id = "TestRecipientAccount" },
                ChannelData = new object()
            };

            _mockTurnContext
                .Setup(m => m.Activity)
                .Returns(_testActivity);

            _mockTurnContext
                .Setup(m => m.OnSendActivities(It.IsAny<SendActivitiesHandler>()))
                .Callback<SendActivitiesHandler>(async (handler) =>
                {
                    await handler.Invoke(_mockTurnContext.Object, _testActivityList, () => Task.FromResult(new List<ResourceResponse>().ToArray()));
                });

            _sut = new SmsMessageQueue(_mockSmsQueueProvider.Object, _mockNotifyOptions.Object, _feedbackBotStateRepository);
        }

        public class EnqueueMessageAsyncTests : SmsMessageQueueTests
        {
            [Fact]
            public async Task WhenCalled_ThenTheMessageIsSentToTheQueueProvider()
            {
                // arrange                
                await InitialiseTurnIdInTestStorage(1);

                // act
                await _sut.EnqueueMessageAsync(_mockTurnContext.Object, _testActivity);

                //assert
                _mockSmsQueueProvider.Verify(m => m.SendAsync(_testActivity.Conversation.Id, It.IsAny<Message>(), _testOutgoingQueueName), times: Times.Once);
            }

            [Fact]
            public async Task WhenCalledAndtheTurnIdIsNotSet_ThenTheTurnIdIsSetToMinusOne()
            {
                // arrange
                OutgoingSms sentSms = null;
                _mockSmsQueueProvider
                    .Setup(m => m.SendAsync(_testActivity.Conversation.Id, It.IsAny<Message>(), _testOutgoingQueueName))
                    .Callback<string, Message, string>((conversationId, message, queueName) =>
                   {
                       sentSms = JsonConvert.DeserializeObject<OutgoingSms>(Encoding.UTF8.GetString(message.Body));
                   })
                   .Returns(Task.CompletedTask);

                // act
                await _sut.EnqueueMessageAsync(_mockTurnContext.Object, _testActivity);

                //assert
                Assert.Equal(-1, sentSms.Conversation.TurnId);
            }
        }

        private Task InitialiseTurnIdInTestStorage(long value)
        {
            return _testStorage.WriteAsync(new Dictionary<string, object>
            {
                { $"{_testActivity.ChannelId}/conversations/{_testActivity.Conversation.Id}",
                    new Dictionary<string, object>
                    {
                        { "turnId", value }
                    }
                }
            });
        }
    }
}
