using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.Dto;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands.SendSms;
using ESFA.DAS.ProvideFeedback.Apprentice.Services.UnitTests.Builders;
using Microsoft.Azure.ServiceBus;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.UnitTests.FeedbackService.SendSms
{
    public class SendSmsCommandHandlerTests
    {
        private SendSmsCommandHandler _sut;
        private Mock<IConversationRepository> _mockConversationRepository;
        private Mock<INotificationClient> _mockNotificationClient;
        private Mock<ISettingService> _mockSettingService;
        private string _testNotifyTemplateId;
        private string _testNotifySmsSenderId;
        private Message _testQueueMessage;
        private OutgoingSms _testOutgoingSms;

        public SendSmsCommandHandlerTests()
        {
            _mockConversationRepository = new Mock<IConversationRepository>();
            _mockNotificationClient = new Mock<INotificationClient>();
            _mockSettingService = new Mock<ISettingService>();

            _testQueueMessage = new Message();
            _testOutgoingSms = new OutgoingSms();

            _testNotifyTemplateId = Guid.NewGuid().ToString();
            _testNotifySmsSenderId = Guid.NewGuid().ToString();

            _mockSettingService
                .Setup(m => m.Get("NotifyTemplateId"))
                .Returns(_testNotifyTemplateId);

            _mockSettingService
                .Setup(m => m.Get("NotifySmsSenderId"))
                .Returns(_testNotifySmsSenderId);

            _sut = new SendSmsCommandHandler(_mockConversationRepository.Object, _mockNotificationClient.Object, _mockSettingService.Object);
        }

        public class HandleAsync : SendSmsCommandHandlerTests
        {
            [Fact]
            public async Task WhenCalled_ThenTheSmsIsSentUsingToTheNotificationClient()
            {
                // arrange
                var mobileNumber = Guid.NewGuid().ToString();
                var messageText = Guid.NewGuid().ToString();
                var reference = Guid.NewGuid().ToString();

                var message = new OutgoingSmsBuilder()
                    .WithConversation(new BotConversationBuilder().WithConversationId(reference))
                    .WithMessageText(messageText)
                    .WithParticipant(new ParticipantBuilder().WithUserId(mobileNumber));

                var command = new SendSmsCommand(message, _testQueueMessage);

                // act
                await _sut.HandleAsync(command);

                //assert
                _mockNotificationClient.Verify(m =>
                m.SendSms(
                    mobileNumber,
                    _testNotifyTemplateId,
                    It.Is<Dictionary<string, dynamic>>(i => i.ContainsKey("message") && i.ContainsValue(messageText)),
                    reference,
                    _testNotifySmsSenderId)
                    , Times.Once);
            }

            [Fact]
            public async Task WhenCalled_ThenTheConversationIsSavedToTheConversationRepository()
            {
                // arrange
                Conversation testConversation = null;
                OutgoingSms message = new OutgoingSmsBuilder();

                _mockConversationRepository.Setup(m =>
                m.Save(It.IsAny<Conversation>()))
                .Callback<Conversation>(c =>
                {

                    c.Id.Should().Be(message.Conversation.ConversationId);
                    c.UserId.Should().Be(message.Conversation.UserId);
                    c.ActivityId.Should().Be(message.Conversation.ActivityId);
                    c.TurnId.Should().Be(message.Conversation.TurnId);

                    testConversation = c;
                })
                .Returns(Task.CompletedTask);
                
                var command = new SendSmsCommand(message, _testQueueMessage);

                // act
                await _sut.HandleAsync(command);

                //assert
                _mockConversationRepository.Verify(m => m.Save(testConversation), Times.Once);
            }

            [Fact]
            public void WhenTheNotificationClientThrowsAnException_ThenTheConversationIsNotSavedTotheConverssationRepository()
            {
                // arrange
                _mockNotificationClient.Setup(m =>
                m.SendSms(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception());

                var command = new SendSmsCommand(new OutgoingSmsBuilder(), _testQueueMessage);

                // act
                Func<Task> action = async () => await _sut.HandleAsync(command);

                //assert
                action.Should().ThrowExactly<Exception>();
                _mockConversationRepository.Verify(m => m.Save(It.IsAny<Conversation>()), Times.Never);
            }
        }
    }
}
