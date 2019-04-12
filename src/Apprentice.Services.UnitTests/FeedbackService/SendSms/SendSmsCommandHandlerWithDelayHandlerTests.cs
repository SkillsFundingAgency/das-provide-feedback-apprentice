using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto;
using ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands.SendSms;
using ESFA.DAS.ProvideFeedback.Apprentice.Services.UnitTests.Builders;
using Microsoft.Azure.ServiceBus;
using Moq;
using System.Threading.Tasks;
using Xunit;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading;
using System;
using FluentAssertions;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.UnitTests.FeedbackService.SendSms
{
    public class SendSmsCommandHandlerWithDelayHandlerTests
    {
        private SendSmsCommandHandlerWithDelayHandler _sut;
        private Mock<ICommandHandlerAsync<SendSmsCommand>> _mockHandler;
        private Mock<IQueueClient> _mockQueueClient;
        private Mock<ILoggerFactory> _mockLoggerFactory;
        private Mock<ISettingService> _mockSettingService;
        private Mock<ILogger> _mockLogger;

        private Message _testQueueMessage;
        private OutgoingSms _testOutgoingSms;
        private int _maxRetryAttempts;
        private int _retryDelayMs;

        public SendSmsCommandHandlerWithDelayHandlerTests()
        {
            _mockHandler = new Mock<ICommandHandlerAsync<SendSmsCommand>>();
            _mockQueueClient = new Mock<IQueueClient>();
            _mockLogger = new Mock<ILogger>();
            _mockLoggerFactory = new Mock<ILoggerFactory>();
            _mockSettingService = new Mock<ISettingService>();

            _testQueueMessage = new Message();
            _testOutgoingSms = new OutgoingSmsBuilder();
            _maxRetryAttempts = 3;
            _retryDelayMs = 1000;
            
            _mockSettingService
                .Setup(m => m.GetInt("maxRetryAttempts"))
                .Returns(_maxRetryAttempts);

            _mockSettingService
                .Setup(m => m.GetInt("retryDelayMs"))
                .Returns(_retryDelayMs);

            _mockLoggerFactory
                .Setup(m => m.CreateLogger(It.IsAny<string>()))                
                .Returns(_mockLogger.Object);

            _sut = new SendSmsCommandHandlerWithDelayHandler(
                _mockHandler.Object,
                _mockQueueClient.Object,
                _mockLoggerFactory.Object,
                _mockSettingService.Object);
        }

        public class HandleAsync : SendSmsCommandHandlerWithDelayHandlerTests
        {
            [Fact]
            public async Task WhenCalled_ThenInnerHandlerIsCalled()
            {
                // arrange
                var command = new SendSmsCommand(_testOutgoingSms, _testQueueMessage);

                // act
                await _sut.HandleAsync(command);

                //assert
                _mockHandler.Verify(m => m.HandleAsync(command, It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public void WhenTheInnerHandlerThrowsAnUnhandledException_ThenExceptionIsPropogated()
            {
                // arrange
                var errorMessage = Guid.NewGuid().ToString();
                var testException = new Exception(errorMessage);

                var command = new SendSmsCommand(_testOutgoingSms, _testQueueMessage);

                _mockHandler
                    .Setup(m => m.HandleAsync(command, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(testException);

                // act
                Func<Task> action = async () => await _sut.HandleAsync(command);

                //assert
                action.Should().ThrowExactly<Exception>().WithMessage(errorMessage);
            }

            [Theory]
            [InlineData(typeof(ConversationLockedException), "ConversationLockedRetryCount" )]
            [InlineData(typeof(OutOfOrderException), "OutOfOrderRetryCount")]
            [InlineData(typeof(PreviousMessageNotSentException), "PreviousMessageNotSentCount")]
            public async Task WhenTheInnerHandlerThrowsAHandledException_ThenADelayedMessageIsSentToTheQueue(Type exceptionType, string retryKey)
            {
                // arrange
                var errorMessage = Guid.NewGuid().ToString();
                var testException = (Exception)Activator.CreateInstance(exceptionType, errorMessage);

                var command = new SendSmsCommand(_testOutgoingSms, _testQueueMessage);

                _mockHandler
                    .Setup(m => m.HandleAsync(command, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(testException);

                // act
                await _sut.HandleAsync(command);

                //assert
                _mockQueueClient.Verify(m => m.SendAsync(It.Is<Message>((qm) => ((int)qm.UserProperties[retryKey]) == 1)), Times.Once);
            }

            [Theory]
            [InlineData(typeof(ConversationLockedException), "ConversationLockedRetryCount", 0, 1)]
            [InlineData(typeof(ConversationLockedException), "ConversationLockedRetryCount", 1, 2)]
            [InlineData(typeof(ConversationLockedException), "ConversationLockedRetryCount", 2, 3)]
            [InlineData(typeof(OutOfOrderException), "OutOfOrderRetryCount", 0, 1)]
            [InlineData(typeof(OutOfOrderException), "OutOfOrderRetryCount", 1, 2)]
            [InlineData(typeof(OutOfOrderException), "OutOfOrderRetryCount", 2, 3)]
            [InlineData(typeof(PreviousMessageNotSentException), "PreviousMessageNotSentCount", 0, 1)]
            [InlineData(typeof(PreviousMessageNotSentException), "PreviousMessageNotSentCount", 1, 2)]
            [InlineData(typeof(PreviousMessageNotSentException), "PreviousMessageNotSentCount", 2, 3)]
            public async Task WhenTheInnerHandlerThrowsAHandledException_ThenADelayedMessageHasItsRetryCountIncremented(Type exceptionType, string retryKey, int initialValue, int finalValue)
            {
                // arrange
                // arrange
                var errorMessage = Guid.NewGuid().ToString();
                var testException = (Exception)Activator.CreateInstance(exceptionType, errorMessage);

                _testQueueMessage.UserProperties[retryKey] = initialValue;

                var command = new SendSmsCommand(_testOutgoingSms, _testQueueMessage);

                _mockHandler
                    .Setup(m => m.HandleAsync(command, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(testException);

                // act
                await _sut.HandleAsync(command);

                //assert
                _mockQueueClient.Verify(m => m.SendAsync(It.Is<Message>((qm) => ((int)qm.UserProperties[retryKey]) == finalValue)), Times.Once);
            }

            [Fact]
            public async Task WhenTheInnerHandlerThrowsAHandledException_ThenADelayedMessageHasItsExistingRetryPropertiesClearedBeforeSettingTheRetryCount()
            {
                // arrange
                var command = new SendSmsCommand(_testOutgoingSms, _testQueueMessage);

                // act
                await _sut.HandleAsync(command);

                //assert

            }

            [Fact]
            public async Task WhenTheInnerHandlerThrowsAHandledException_ThenTheDelayIsLogged()
            {
                // arrange
                var command = new SendSmsCommand(_testOutgoingSms, _testQueueMessage);

                // act
                await _sut.HandleAsync(command);

                //assert

            }

            [Fact]
            public async Task WhenTheInnerHandlerThrowsAHandledExceptionAndTheNumberOfretriesIsExceeded_ThenTheOriginalExceptionIsRethrown()
            {
                // arrange
                var command = new SendSmsCommand(_testOutgoingSms, _testQueueMessage);

                // act
                await _sut.HandleAsync(command);

                //assert

            }

        }
    }
}
