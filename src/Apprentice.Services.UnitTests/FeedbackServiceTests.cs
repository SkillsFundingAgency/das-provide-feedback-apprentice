namespace ESFA.DAS.ProvideFeedback.Apprentice.Services.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Connectors.UnitTests.Stubs;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Interfaces;
    using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models;
    using ESFA.DAS.ProvideFeedback.Apprentice.Data.Repositories;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService;
    using ESFA.DAS.ProvideFeedback.Apprentice.Services.FeedbackService.Commands;

    using Microsoft.Extensions.Options;

    using NSubstitute;

    using Xunit;

    using ApprenticeFeedbackDto = Data.Dto.ApprenticeFeedback;

    using AzureOptions = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Azure;
    using DataOptions = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Data;

    public class FeedbackServiceTests : IDisposable
    {
        private readonly IFeedbackRepository feedbackRepo;
        private readonly ICommandHandlerAsync<SaveFeedbackCommand> saveFeedbackCommandHandler;
        private readonly IFeedbackService sut;

        private readonly IOptions<DataOptions> dataConfig;

        private readonly IOptions<AzureOptions> azureConfig;

        public FeedbackServiceTests()
        {
            this.dataConfig = Options.Create(new DataOptions());
            this.azureConfig = Options.Create(new AzureOptions());

            this.feedbackRepo = Substitute.For<IFeedbackRepository>();
            this.saveFeedbackCommandHandler = Substitute.For<ICommandHandlerAsync<SaveFeedbackCommand>>();

            this.sut = new FeedbackService(this.saveFeedbackCommandHandler);
        }

        public class SaveFeedbackTest : FeedbackServiceTests
        {
            const string UNIQUE_LEARNER_NUMBER = "abc-123";
            const string STANDARD_ID = "STN-001";
            const string UKPRN = "100001";

            private readonly ApprenticeFeedback feedback;

            public SaveFeedbackTest()
            {
                this.feedback = new ApprenticeFeedback()
                    {
                        StartTime = DateTime.UtcNow.AddMinutes(-5),
                        FinishTime = DateTime.UtcNow,
                        Apprentice = FakeFeedbackRepository.CreateApprentice(UNIQUE_LEARNER_NUMBER),
                        Apprenticeship = FakeFeedbackRepository.CreateStandard(UKPRN, STANDARD_ID),
                        Responses = new List<ApprenticeResponse>() { FakeFeedbackRepository.CreateResponse(100, "question1", "yeah", "yes"), FakeFeedbackRepository.CreateResponse(100, "question2", "nah", "no") }
                    };

                this.feedbackRepo.When(x => x.SaveFeedback(Arg.Any<ApprenticeFeedbackDto>()))
                    .Do(
                        async x =>
                            {
                                string key = Guid.NewGuid().ToString();
                                var dto = x.Arg<ApprenticeFeedbackDto>();
                                var success = FakeFeedbackRepository.Feedback.TryAdd(key, dto);
                            });

                this.saveFeedbackCommandHandler.When(x => x.HandleAsync(Arg.Any<SaveFeedbackCommand>(), Arg.Any<CancellationToken>()))
                    .Do(
                        async x =>
                            {
                                var command = this.BuildCommand(this.feedback);
                                await command
                                    .Using(this.feedbackRepo)
                                    .ExecuteAsync(x.Arg<CancellationToken>());
                            });

            }

            [Fact]
            public void ShouldAddFeedbackToCollectionOnSave()
            {
                // arrange
                var expected = this.feedback;

                // act
                this.sut.SaveFeedbackAsync(expected);
                var actual = FakeFeedbackRepository.Feedback.FirstOrDefault(
                    f => f.Value.Apprentice.UniqueLearnerNumber == UNIQUE_LEARNER_NUMBER);

                // assert
                Assert.NotNull(actual.Value);
                Assert.Equal(expected.StartTime, actual.Value.StartTime);
                Assert.Equal(expected.FinishTime, actual.Value.FinishTime);
                Assert.Equal(expected.Apprentice, actual.Value.Apprentice);
                Assert.Equal(expected.Apprenticeship, actual.Value.Apprenticeship);
                Assert.Equal(expected.Responses, actual.Value.Responses);
            }
        }

        private SaveFeedbackCommand BuildCommand(ApprenticeFeedback feedback)
        {
            SaveFeedbackCommand command = new SaveFeedbackCommand()
                .StartedOn(feedback.StartTime)
                .CompletedOn(feedback.FinishTime)
                .SubmittedBy(feedback.Apprentice)
                .WithResponses(feedback.Responses)
                .ForApprenticeship(feedback.Apprenticeship);

            return command;
        }

        public void Dispose()
        {
        }
    }
}
