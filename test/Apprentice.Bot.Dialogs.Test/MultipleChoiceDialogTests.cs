using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs;
using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Feedback.Components;
using ESFA.DAS.ProvideFeedback.Apprentice.Bot.Dialogs.Models;
using ESFA.DAS.ProvideFeedback.Apprentice.BotV4.Surveys;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.Models.Feedback;
using ESFA.DAS.ProvideFeedback.Apprentice.Core.State;
using ESFA.DAS.ProvideFeedback.Apprentice.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Moq;
using Xunit;

using BotSettings = ESFA.DAS.ProvideFeedback.Apprentice.Core.Configuration.Bot;

namespace Apprentice.Bot.Dialogs.Test
{
    public class ComponentDialogTests
    {
        private ComponentDialog _dialog;
        private Mock<IFeedbackBotStateRepository> _feedbackBotStateMock;
        private Mock<IFeedbackService> _feedbackServiceMock;

        public ComponentDialogTests()
        {
            var userProfileAccessor = new Mock<IStatePropertyAccessor<UserProfile>>();
            var userProfile = new UserProfile();
            userProfileAccessor.Setup(x => x.GetAsync(It.IsAny<ITurnContext>(), It.IsAny<Func<UserProfile>>(), It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);
            var surveyState = new SurveyState();
            var features = new Features { CollateResponses = true, RealisticTypingDelay = false };
            _feedbackServiceMock = new Mock<IFeedbackService>();
            _feedbackBotStateMock = new Mock<IFeedbackBotStateRepository>();
            _feedbackBotStateMock.SetupGet(x => x.UserProfile).Returns(userProfileAccessor.Object);
            _dialog = new MultipleChoiceDialog("feedback_q1", _feedbackBotStateMock.Object, new BotSettings(), features, _feedbackServiceMock.Object)
                .WithPrompt("Question one?")
                        .WithResponses(new List<IBotResponse> { new PositiveBotResponse { Prompt = "Thanks" }, new NegativeBotResponse { Prompt = "thansk :(" } })
                        .WithScore(100)
                        .Build(); ;
        }

        [Fact]
        public async Task WhenAQuestionGetsAResponse_ShouldSaveFeedback()
        {
            // Arrange 
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var dialogs = new DialogSet(dialogState);

            dialogs.Add(_dialog);
            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded)
                {
                    await dc.BeginDialogAsync("feedback_q1", null, cancellationToken);
                }
            })
            .Send("Say something to start test")
            .AssertReply("Question one?")
            .Send("Yes")
            .StartTestAsync();

            _feedbackServiceMock.Verify(mock => mock.SaveFeedbackAsync(It.Is<ApprenticeFeedback>(x => x.Responses.Count == 1)), Times.Once);
        }
    }
}
