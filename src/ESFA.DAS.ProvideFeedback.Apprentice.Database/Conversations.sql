﻿CREATE TABLE [dbo].[Conversations]
(
	[Id] UNIQUEIDENTIFIER NOT NULL,
	[UserId] UNIQUEIDENTIFIER NOT NULL , 
    [ActivityId] VARCHAR(50) NOT NULL, 
	[TurnId] BIGINT NOT NULL
    CONSTRAINT [PK_Conversations] PRIMARY KEY ([Id]),
	CONSTRAINT UC_Conversation UNIQUE (Id, TurnId)
)
