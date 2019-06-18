using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;
using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Builders;
using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Survey;
using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Surveys;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Feedback;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
using ESFA.DAS.ProvideFeedback.Apprentice.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;

namespace Apprentice.Bot.Dialogs.Test
{
    public class SurveyTests
    {
        const string SurveyCode = "afb-v6";
        private ComponentDialog _dialog;
        private Mock<IFeedbackBotStateRepository> _feedbackBotStateMock;
        private Mock<IFeedbackService> _feedbackServiceMock;
        private Features _features;

        public SurveyTests()
        {
            var userProfileAccessor = new Mock<IStatePropertyAccessor<UserProfile>>();
            var userProfile = new UserProfile();
            userProfileAccessor.Setup(x => x.GetAsync(It.IsAny<ITurnContext>(), It.IsAny<Func<UserProfile>>(), It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);
            var surveyState = new SurveyState();
            _features = new Features { CollateResponses = true, RealisticTypingDelay = false };
            var botSettings = new BotSettings();
            _feedbackServiceMock = new Mock<IFeedbackService>();
            _feedbackBotStateMock = new Mock<IFeedbackBotStateRepository>();
            _feedbackBotStateMock.SetupGet(x => x.UserProfile).Returns(userProfileAccessor.Object);

            var dialogFactory = new DialogFactory(new List<IComponentBuilder<ComponentDialog>>
            {
                new FreeTextDialogComponentBuilder(_feedbackBotStateMock.Object, Options.Create(_features), Options.Create(botSettings)),
                new MultipleChoiceDialogComponentBuilder(_feedbackBotStateMock.Object, Options.Create(_features), Options.Create(botSettings), _feedbackServiceMock.Object),
                new SurveyStartDialogComponentBuilder(_feedbackBotStateMock.Object, Options.Create(_features), Options.Create(botSettings)),
                new SurveyEndDialogComponentBuilder(_feedbackBotStateMock.Object, Options.Create(_features), Options.Create(botSettings))
            });

            _dialog = dialogFactory.Create<SurveyDialog>(new InMemoryApprenticeFeedbackSurveyV6());
        }

        [Fact]
        public Task WhenNotCollatingMessages_IntroShouldBe3Message()
        {
            // Arrange 
            _features.CollateResponses = false;

            //Act
            return GetTestFlow()
            .Send("Say something to start test")
            .AssertReply("Here’s your apprenticeship survey from the Department for Education.")
            .AssertReply("It's just 4 questions and it'll really help us improve apprenticeships.")
            .AssertReply("Normal SMS charges apply. To stop receiving these messages, please type ‘Stop’")
            .StartTestAsync();
        }

        [Fact]
        public Task WhenCollatingMessages_IntroShouldBeSingleMessage()
        {
            // Arrange
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Here’s your apprenticeship survey from the Department for Education.");
            sb.AppendLine("It's just 4 questions and it'll really help us improve apprenticeships.");
            sb.AppendLine("Normal SMS charges apply. To stop receiving these messages, please type ‘Stop’");

            // Act
            return GetTestFlow()
            .Send("Say something to start test")
            .AssertReply(sb.ToString())
            .StartTestAsync();
        }

        [Fact]
        public async Task WhenAUserCompletesSurvey_ShouldSaveCompletedFeedback()
        {
            // Arrange 

            // Act
            await GetTestFlow()
            .Send("Say something to start test")
            .Send("Yes")
            .Send("Yes")
            .Send("Yes")
            .Send("Yes")
            .StartTestAsync();

            // Assert
            _feedbackServiceMock.Verify(mock => mock.SaveFeedbackAsync(It.Is<ApprenticeFeedback>(x => x.Responses.Count == 4)), Times.Once);
        }

        [Fact]
        public async Task WhenAUserResponds_ShouldSavePartialFeedback()
        {
            // Arrange 
            
            // Act
            await GetTestFlow()
            .Send("Say something to start test")
            .Send("Yes")
            .StartTestAsync();

            // Assert
            _feedbackServiceMock.Verify(mock => mock.SaveFeedbackAsync(It.Is<ApprenticeFeedback>(x => x.Responses.Count == 1)), Times.Once);
        }

        private TestFlow GetTestFlow()
        {
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var dialogs = new DialogSet(dialogState);

            dialogs.Add(_dialog);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded)
                {
                    await dc.BeginDialogAsync(SurveyCode, null, cancellationToken);
                }
            });
        }
    }
}
