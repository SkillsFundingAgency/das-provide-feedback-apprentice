CREATE TABLE [dbo].[ApprenticeSurveyInvites]
(
	[Id] UNIQUEIDENTIFIER NOT NULL,
	[UniqueLearnerNumber] VARCHAR(50) NOT NULL , 
    [MobileNumber] VARCHAR(15) NOT NULL, 
	[StandardCode] INT NOT NULL,
    [Ukprn] BIGINT NOT NULL, 
    [SurveyCode] VARCHAR(10) NOT NULL, 
    [SentDate] DATETIME NULL, 
    [ExpiryDate] DATETIME NULL, 
    CONSTRAINT [PK_ApprenticeSurveyInvites] PRIMARY KEY ([Id])
)
