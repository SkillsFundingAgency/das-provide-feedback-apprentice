using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bogus;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Dto;
using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands.TriggerSurveyInvites;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.UnitTests.FeedbackService.TriggerSurveyInvites
{
    public class TriggerSurveyInvitesHandlerTests
    {
        const int BatchSize = 5;
        private TriggerSurveyInvitesCommandHandler _sut;
        private Mock<ISettingService> _mockSettingService;
        private Mock<IStoreApprenticeSurveyDetails> _mockSurveyDetailsRepo;
        private Mock<IQueueClientFactory> _mockQueueClientFactory;
        private Mock<IQueueClient> _mockQueueClient;

        public TriggerSurveyInvitesHandlerTests()
        {
            _mockSurveyDetailsRepo = new Mock<IStoreApprenticeSurveyDetails>();
            _mockSettingService = new Mock<ISettingService>();
            _mockQueueClientFactory = new Mock<IQueueClientFactory>();
            _mockQueueClient = new Mock<IQueueClient>();

            _mockQueueClientFactory.Setup(mock => mock.CreateIncomingSmsQueueClient()).Returns(_mockQueueClient.Object);
            _mockSettingService.Setup(mock => mock.GetInt("ApprenticeBatchSize")).Returns(5);

            _sut = new TriggerSurveyInvitesCommandHandler(_mockSurveyDetailsRepo.Object, _mockSettingService.Object, _mockQueueClientFactory.Object, Mock.Of<ILogger<TriggerSurveyInvitesCommandHandler>>());
        }

        [Fact]
        public async Task WhenNoInvitesToSend_ThenShouldNotUpdateSentDates()
        {
            // Arrange


            // Act
            await _sut.HandleAsync(new TriggerSurveyInvitesCommand());

            // Assert
            _mockSurveyDetailsRepo.Verify(mock => mock.SetApprenticeSurveySentAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task WhenNoInvitesToSend_ThenShouldNoSendTriggerMessages()
        {
            // Arrange


            // Act
            await _sut.HandleAsync(new TriggerSurveyInvitesCommand());

            // Assert
            _mockQueueClient.Verify(mock => mock.SendAsync(It.IsAny<Message>()), Times.Never);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public async Task WhenInvitesToSendExist_ThenShouldUpdateSentDates(int numberOfInvitesToSend)
        {
            // Arrange
            var invites = CreateInvitesToSend(numberOfInvitesToSend);
            _mockSurveyDetailsRepo.Setup(mock => mock.GetApprenticeSurveyInvitesAsync(5)).ReturnsAsync(invites);

            // Act
            await _sut.HandleAsync(new TriggerSurveyInvitesCommand());

            // Assert
            _mockSurveyDetailsRepo.Verify(mock => mock.SetApprenticeSurveySentAsync(It.IsAny<Guid>()), Times.Exactly(numberOfInvitesToSend));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public async Task WhenInvitesToSendExist_ThenShouldSendTriggerMessages(int numberOfInvitesToSend)
        {
            // Arrange
            var invites = CreateInvitesToSend(numberOfInvitesToSend);
            _mockSurveyDetailsRepo.Setup(mock => mock.GetApprenticeSurveyInvitesAsync(5)).ReturnsAsync(invites);

            // Act
            await _sut.HandleAsync(new TriggerSurveyInvitesCommand());

            // Assert
            _mockQueueClient.Verify(mock => mock.SendAsync(It.IsAny<Message>()), Times.Exactly(numberOfInvitesToSend));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public async Task WhenInviteMessageFailsToSend_ThenMarkInviteAsUnsent(int numberOfInvitesToSend)
        {
            // Arrange
            var invites = CreateInvitesToSend(numberOfInvitesToSend);
            _mockSurveyDetailsRepo.Setup(mock => mock.GetApprenticeSurveyInvitesAsync(5)).ReturnsAsync(invites);
            _mockQueueClient.Setup(mock => mock.SendAsync(It.IsAny<Message>())).ThrowsAsync(new Exception());

            // Act
            await _sut.HandleAsync(new TriggerSurveyInvitesCommand());

            // Assert
            _mockSurveyDetailsRepo.Verify(mock => mock.SetApprenticeSurveyNotSentAsync(It.IsAny<Guid>()), Times.Exactly(numberOfInvitesToSend));
        }

        [Fact]
        public async Task WhenInviteMessageFailsToSend_ShouldStillAttemptToSendOtherMessages()
        {
            // Arrange
            var numberOfInvitesToSend = 3;
            var invites = CreateInvitesToSend(numberOfInvitesToSend);
            _mockSurveyDetailsRepo.Setup(mock => mock.GetApprenticeSurveyInvitesAsync(5)).ReturnsAsync(invites);
            _mockQueueClient.Setup(mock => mock.SendAsync(It.IsAny<Message>()))
                .ThrowsAsync(new Exception())
                .Callback(() =>
                {
                    _mockQueueClient.Setup(mock => mock.SendAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);
                });

            // Act
            await _sut.HandleAsync(new TriggerSurveyInvitesCommand());

            // Assert
            _mockQueueClient.Verify(mock => mock.SendAsync(It.IsAny<Message>()), Times.Exactly(numberOfInvitesToSend));
            _mockSurveyDetailsRepo.Verify(mock => mock.SetApprenticeSurveyNotSentAsync(It.IsAny<Guid>()), Times.Once);
        }

        private List<ApprenticeSurveyInvite> CreateInvitesToSend(int numberOfInvitesToSend)
        {
            return new Faker<ApprenticeSurveyInvite>().Generate(numberOfInvitesToSend);
        }
    }
}
