CREATE TABLE [dbo].[ApprenticeSurveyInvites]
(
	[Id] UNIQUEIDENTIFIER NOT NULL,
	[UniqueLearnerNumber] VARCHAR(50) NOT NULL , 
    [MobileNumber] INT NOT NULL, 
	[StandardCode] INT NOT NULL,
    [Ukprn] INT NOT NULL, 
    [SurveyCode] VARCHAR(10) NOT NULL, 
    [SentDate] DATETIME NULL, 
    [ExpiryDate] DATETIME NOT NULL, 
    CONSTRAINT [PK_ApprenticeSurveyInvites] PRIMARY KEY ([Id])
)
