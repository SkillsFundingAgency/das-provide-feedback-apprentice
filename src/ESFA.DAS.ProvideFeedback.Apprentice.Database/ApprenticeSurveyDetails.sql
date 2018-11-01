CREATE TABLE [dbo].[ApprenticeSurveyDetails]
(
	[UniqueLearnerNumber] VARCHAR(50) NOT NULL PRIMARY KEY, 
    [MobileNumber] INT NOT NULL, 
	[StandardCode] INT NOT NULL,
    [Ukprn] INT NOT NULL, 
    [SurveyCode] VARCHAR(10) NOT NULL, 
    [SentDate] DATETIME NULL
)
