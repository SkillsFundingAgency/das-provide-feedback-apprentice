using System;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Exceptions;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Apprentice.Domain.Dto;
using ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands.SendSms;
using ESFA.DAS.ProvideFeedback.Apprentice.Services.UnitTests.Builders;
using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Moq;
using Xunit;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.UnitTests.FeedbackService.SendSms
{
    public class SendSmsCommandHandlerWithOrderCheckTests
    {
        private SendSmsCommandHandlerWithOrderCheck _sut;
        private Mock<ICommandHandlerAsync<SendSmsCommand>> _mockHandler;
        private Mock<IConversationRepository> _mockConversationRepository;
        
        private Message _testQueueMessage;
        private OutgoingSms _testOutgoingSms;

        public SendSmsCommandHandlerWithOrderCheckTests()
        {
            _mockHandler = new Mock<ICommandHandlerAsync<SendSmsCommand>>();
            _mockConversationRepository = new Mock<IConversationRepository>();
            
            _testQueueMessage = new Message();
            _testOutgoingSms = new OutgoingSmsBuilder();
            
            _sut = new SendSmsCommandHandlerWithOrderCheck(
                _mockHandler.Object,
                _mockConversationRepository.Object);
        }

        public class HandleAsync : SendSmsCommandHandlerWithOrderCheckTests
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

            [Fact]
            public async Task WhenCalled_ThenLatestConversationIsRetrievedFromtherepository()
            {
                // arrange
                var command = new SendSmsCommand(_testOutgoingSms, _testQueueMessage);

                // act
                await _sut.HandleAsync(command);

                //assert
                _mockConversationRepository.Verify(m => m.Get(_testOutgoingSms.Conversation.ConversationId), Times.Once);
            }

            [Fact]
            public async Task WhenTheConversationHasAlreadyBeenStartedAndTheCommandIsInOrder_ThenTheInnerHandlerIsCalled()
            {
                // arrange
                Conversation lastConversation = new ConversationBuilder()
                    .WithId(_testOutgoingSms.Conversation.ConversationId)
                    .WithTurnId(2);

                _testOutgoingSms.Conversation.TurnId = lastConversation.TurnId + 1;

                var command = new SendSmsCommand(_testOutgoingSms, _testQueueMessage);                

                _mockConversationRepository
                    .Setup(m => m.Get(_testOutgoingSms.Conversation.ConversationId))
                    .ReturnsAsync(lastConversation);

                // act
                await _sut.HandleAsync(command);

                //assert
                _mockHandler.Verify(m => m.HandleAsync(command, It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task WhenTheTurnIdHasAlreadyBeenProcessed_ThenTheInnerHandlerIsNotCalled()
            {
                // arrange
                Conversation lastConversation = new ConversationBuilder()
                    .WithId(_testOutgoingSms.Conversation.ConversationId)
                    .WithTurnId(2);

                _testOutgoingSms.Conversation.TurnId = lastConversation.TurnId - 1;

                var command = new SendSmsCommand(_testOutgoingSms, _testQueueMessage);

                _mockConversationRepository
                    .Setup(m => m.Get(_testOutgoingSms.Conversation.ConversationId))
                    .ReturnsAsync(lastConversation);

                // act
                await _sut.HandleAsync(command);

                //assert
                _mockHandler.Verify(m => m.HandleAsync(command, It.IsAny<CancellationToken>()), Times.Never);
            }

            [Fact]
            public async Task WhenTheTurnIdIsThesameAsTheCurentOne_ThenTheInnerHandlerIsNotCalled()
            {
                // arrange
                Conversation lastConversation = new ConversationBuilder()
                    .WithId(_testOutgoingSms.Conversation.ConversationId)
                    .WithTurnId(2);

                _testOutgoingSms.Conversation.TurnId = lastConversation.TurnId;

                var command = new SendSmsCommand(_testOutgoingSms, _testQueueMessage);

                _mockConversationRepository
                    .Setup(m => m.Get(_testOutgoingSms.Conversation.ConversationId))
                    .ReturnsAsync(lastConversation);

                // act
                await _sut.HandleAsync(command);

                //assert
                _mockHandler.Verify(m => m.HandleAsync(command, It.IsAny<CancellationToken>()), Times.Never);
            }

            [Fact]
            public void WhenTheTurnIdIsOutOfOrder_ThenAnOutOfOrderExceptionIscalled()
            {
                // arrange
                Conversation lastConversation = new ConversationBuilder()
                    .WithId(_testOutgoingSms.Conversation.ConversationId)
                    .WithTurnId(3);

                _testOutgoingSms.Conversation.TurnId = lastConversation.TurnId + 2;

                var command = new SendSmsCommand(_testOutgoingSms, _testQueueMessage);

                _mockConversationRepository
                    .Setup(m => m.Get(_testOutgoingSms.Conversation.ConversationId))
                    .ReturnsAsync(lastConversation);

                // act
                Func<Task> action = async () => await _sut.HandleAsync(command);

                //assert
                action.Should().ThrowExactly<OutOfOrderException>().WithMessage($"Message for conversation {_testOutgoingSms.Conversation.ConversationId} processed out of order.  Expected turnId {lastConversation.TurnId + 1} but received turnId {command.Message.Conversation.TurnId} with activityId {command.Message.Conversation.ActivityId}");
            }

            [Fact]
            public void WhenTheTurnIdIsOutOfOrder_ThenTheInnerHandlerIsNotCalled()
            {
                // arrange
                Conversation lastConversation = new ConversationBuilder()
                    .WithId(_testOutgoingSms.Conversation.ConversationId)
                    .WithTurnId(3);

                _testOutgoingSms.Conversation.TurnId = lastConversation.TurnId + 2;

                var command = new SendSmsCommand(_testOutgoingSms, _testQueueMessage);

                _mockConversationRepository
                    .Setup(m => m.Get(_testOutgoingSms.Conversation.ConversationId))
                    .ReturnsAsync(lastConversation);

                // act
                Func<Task> action = async () => await _sut.HandleAsync(command);
                action.Invoke();

                //assert
                _mockHandler.Verify(m => m.HandleAsync(It.IsAny< SendSmsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }
    }
}
