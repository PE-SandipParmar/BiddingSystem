-- Create SecurityEvents table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SecurityEvents' AND xtype='U')
BEGIN
    CREATE TABLE SecurityEvents (
        Id int IDENTITY(1,1) PRIMARY KEY,
        EventType nvarchar(50) NOT NULL,
        Description nvarchar(500) NOT NULL,
        UserId int NULL,
        Username nvarchar(100) NULL,
        IpAddress nvarchar(45) NOT NULL,
        UserAgent nvarchar(500) NULL,
        AdditionalData nvarchar(MAX) NULL,
        Severity int NOT NULL DEFAULT 2, -- 1=Low, 2=Medium, 3=High, 4=Critical
        Timestamp datetime2 NOT NULL DEFAULT GETUTCDATE(),
        IsResolved bit NOT NULL DEFAULT 0,
        Resolution nvarchar(500) NULL,

        CONSTRAINT FK_SecurityEvents_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
    );

    CREATE INDEX IX_SecurityEvents_EventType ON SecurityEvents(EventType);
    CREATE INDEX IX_SecurityEvents_UserId ON SecurityEvents(UserId);
    CREATE INDEX IX_SecurityEvents_Timestamp ON SecurityEvents(Timestamp);
    CREATE INDEX IX_SecurityEvents_Severity ON SecurityEvents(Severity);
    CREATE INDEX IX_SecurityEvents_IpAddress ON SecurityEvents(IpAddress);
END
GO

-- Create UserActions table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserActions' AND xtype='U')
BEGIN
    CREATE TABLE UserActions (
        Id int IDENTITY(1,1) PRIMARY KEY,
        UserId int NOT NULL,
        Action nvarchar(100) NOT NULL,
        EntityType nvarchar(50) NOT NULL,
        EntityId int NULL,
        OldValues nvarchar(MAX) NULL,
        NewValues nvarchar(MAX) NULL,
        IpAddress nvarchar(45) NOT NULL,
        UserAgent nvarchar(500) NULL,
        Timestamp datetime2 NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT FK_UserActions_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
    );

    CREATE INDEX IX_UserActions_UserId ON UserActions(UserId);
    CREATE INDEX IX_UserActions_Action ON UserActions(Action);
    CREATE INDEX IX_UserActions_EntityType ON UserActions(EntityType);
    CREATE INDEX IX_UserActions_Timestamp ON UserActions(Timestamp);
    CREATE INDEX IX_UserActions_EntityId ON UserActions(EntityId);
END
GO

-- Create FailedLoginAttempts table for additional security tracking
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FailedLoginAttempts' AND xtype='U')
BEGIN
    CREATE TABLE FailedLoginAttempts (
        Id int IDENTITY(1,1) PRIMARY KEY,
        Username nvarchar(100) NOT NULL,
        IpAddress nvarchar(45) NOT NULL,
        AttemptTime datetime2 NOT NULL DEFAULT GETUTCDATE(),
        UserAgent nvarchar(500) NULL,
        Reason nvarchar(200) NULL,
        IsBlocked bit NOT NULL DEFAULT 0
    );

    CREATE INDEX IX_FailedLoginAttempts_Username ON FailedLoginAttempts(Username);
    CREATE INDEX IX_FailedLoginAttempts_IpAddress ON FailedLoginAttempts(IpAddress);
    CREATE INDEX IX_FailedLoginAttempts_AttemptTime ON FailedLoginAttempts(AttemptTime);
END
GO

-- Create UserSessions table for session tracking
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserSessions' AND xtype='U')
BEGIN
    CREATE TABLE UserSessions (
        Id int IDENTITY(1,1) PRIMARY KEY,
        UserId int NOT NULL,
        SessionId nvarchar(100) NOT NULL,
        IpAddress nvarchar(45) NOT NULL,
        UserAgent nvarchar(500) NULL,
        LoginTime datetime2 NOT NULL DEFAULT GETUTCDATE(),
        LastActivity datetime2 NOT NULL DEFAULT GETUTCDATE(),
        LogoutTime datetime2 NULL,
        IsActive bit NOT NULL DEFAULT 1,

        CONSTRAINT FK_UserSessions_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
    );

    CREATE INDEX IX_UserSessions_UserId ON UserSessions(UserId);
    CREATE INDEX IX_UserSessions_SessionId ON UserSessions(SessionId);
    CREATE INDEX IX_UserSessions_IsActive ON UserSessions(IsActive);
    CREATE INDEX IX_UserSessions_LastActivity ON UserSessions(LastActivity);
END
GO

PRINT 'Security audit tables created successfully!';
