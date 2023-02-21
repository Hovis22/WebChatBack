 IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'Chat')
  BEGIN
    CREATE DATABASE [Chat]


    END
	  GO
       USE [Chat]
    GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' and xtype='U')
BEGIN
    CREATE TABLE Users (
        Id INT PRIMARY KEY IDENTITY (1, 1),
        Name VARCHAR(100)  NOT NULL,
        Email VARCHAR(100)  NOT NULL,
        UserPassword VARCHAR(100) NOT NULL

    )
END




IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ChatBlock' and xtype='U')
BEGIN
    CREATE TABLE ChatBlock (
        Id INT PRIMARY KEY IDENTITY (1, 1),
        OwnerId INT  NOT NULL,
        OtherId INT  NOT NULL,
        CONSTRAINT [FK_ChatBlock_OwnerId_UserId] FOREIGN KEY ([OwnerId]) REFERENCES [dbo].[Users] ([Id]),
        CONSTRAINT [FK_ChatBlock_OtherId_UserId] FOREIGN KEY ([OtherId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
    )
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Messag' and xtype='U')
BEGIN
    CREATE TABLE Messag (
        Id INT PRIMARY KEY IDENTITY (1, 1),
        ChatId INT  NOT NULL,
        UserId INT NOT NULL,
        Mess_Text varchar(200) NOT NULL,
        CONSTRAINT [FK_Messag_ChatId_Chat] FOREIGN KEY ([ChatId]) REFERENCES [dbo].[ChatBlock] ([Id]) ON DELETE CASCADE,
       CONSTRAINT [FK_Messag_UserId_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])

    )
END


INSERT INTO Users (Name,Email,UserPassword) VALUES('Anton','admin@i.ua','123');
