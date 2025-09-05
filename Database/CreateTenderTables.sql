-- Create Tender Management Tables
-- Run this script in your SQL Server database

USE [BiddingDb]
GO

-- Create Tenders Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Tenders' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Tenders](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [TenderId] [nvarchar](50) NOT NULL,
        [TenderTitle] [nvarchar](200) NOT NULL,
        [Description] [nvarchar](1000) NOT NULL,
        [Department] [nvarchar](100) NOT NULL,
        [PublishDate] [datetime2](7) NOT NULL,
        [EmdAmount] [decimal](18, 2) NOT NULL,
        [SdAmount] [decimal](18, 2) NOT NULL,
        [ProcessingFee] [decimal](18, 2) NOT NULL,
        [EstimatedValue] [decimal](18, 2) NOT NULL,
        [LastDateEmd] [datetime2](7) NOT NULL,
        [TenderClosingDate] [datetime2](7) NULL,
        [TenderOpeningDate] [datetime2](7) NULL,
        [Status] [int] NOT NULL DEFAULT(1),
        [CreatedBy] [int] NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        [UpdatedAt] [datetime2](7) NULL,
        [PublishedAt] [datetime2](7) NULL,
        [IsActive] [bit] NOT NULL DEFAULT(1),
        
        CONSTRAINT [PK_Tenders] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Tenders_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([Id])
    )
    
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Tenders_TenderId] ON [dbo].[Tenders] ([TenderId]) WHERE [IsActive] = 1
    CREATE NONCLUSTERED INDEX [IX_Tenders_Status] ON [dbo].[Tenders] ([Status]) WHERE [IsActive] = 1
    CREATE NONCLUSTERED INDEX [IX_Tenders_Department] ON [dbo].[Tenders] ([Department]) WHERE [IsActive] = 1
    
    PRINT 'Tenders table created successfully'
END
ELSE
BEGIN
    PRINT 'Tenders table already exists'
END
GO

-- Create TenderDocuments Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TenderDocuments' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[TenderDocuments](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [TenderId] [int] NOT NULL,
        [DocumentName] [nvarchar](255) NOT NULL,
        [FileName] [nvarchar](255) NOT NULL,
        [FilePath] [nvarchar](500) NOT NULL,
        [FileSize] [bigint] NOT NULL,
        [ContentType] [nvarchar](100) NOT NULL,
        [DocumentType] [int] NOT NULL DEFAULT(1),
        [IsRequired] [bit] NOT NULL DEFAULT(0),
        [UploadedBy] [int] NOT NULL,
        [UploadedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        [IsActive] [bit] NOT NULL DEFAULT(1),
        
        CONSTRAINT [PK_TenderDocuments] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_TenderDocuments_Tenders] FOREIGN KEY ([TenderId]) REFERENCES [dbo].[Tenders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TenderDocuments_Users] FOREIGN KEY ([UploadedBy]) REFERENCES [dbo].[Users] ([Id])
    )
    
    CREATE NONCLUSTERED INDEX [IX_TenderDocuments_TenderId] ON [dbo].[TenderDocuments] ([TenderId]) WHERE [IsActive] = 1
    
    PRINT 'TenderDocuments table created successfully'
END
ELSE
BEGIN
    PRINT 'TenderDocuments table already exists'
END
GO

-- Create TenderBids Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TenderBids' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[TenderBids](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [TenderId] [int] NOT NULL,
        [BidderName] [nvarchar](200) NOT NULL,
        [BidderEmail] [nvarchar](100) NOT NULL,
        [BidderPhone] [nvarchar](20) NOT NULL,
        [CompanyName] [nvarchar](200) NULL,
        [CompanyAddress] [nvarchar](500) NULL,
        [BidAmount] [decimal](18, 2) NOT NULL,
        [EmdAmount] [decimal](18, 2) NOT NULL,
        [ProcessingFee] [decimal](18, 2) NOT NULL,
        [TotalAmount] [decimal](18, 2) NOT NULL,
        [Status] [int] NOT NULL DEFAULT(1),
        [PaymentStatus] [int] NOT NULL DEFAULT(1),
        [PaymentReference] [nvarchar](100) NULL,
        [PaymentDate] [datetime2](7) NULL,
        [SubmittedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        [IsActive] [bit] NOT NULL DEFAULT(1),
        
        CONSTRAINT [PK_TenderBids] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_TenderBids_Tenders] FOREIGN KEY ([TenderId]) REFERENCES [dbo].[Tenders] ([Id]) ON DELETE CASCADE
    )
    
    CREATE NONCLUSTERED INDEX [IX_TenderBids_TenderId] ON [dbo].[TenderBids] ([TenderId]) WHERE [IsActive] = 1
    CREATE NONCLUSTERED INDEX [IX_TenderBids_Status] ON [dbo].[TenderBids] ([Status]) WHERE [IsActive] = 1
    
    PRINT 'TenderBids table created successfully'
END
ELSE
BEGIN
    PRINT 'TenderBids table already exists'
END
GO

-- Create TenderBidDocuments Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TenderBidDocuments' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[TenderBidDocuments](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [TenderBidId] [int] NOT NULL,
        [DocumentName] [nvarchar](255) NOT NULL,
        [FileName] [nvarchar](255) NOT NULL,
        [FilePath] [nvarchar](500) NOT NULL,
        [FileSize] [bigint] NOT NULL,
        [ContentType] [nvarchar](100) NOT NULL,
        [DocumentType] [int] NOT NULL DEFAULT(1),
        [IsRequired] [bit] NOT NULL DEFAULT(0),
        [UploadedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        [IsActive] [bit] NOT NULL DEFAULT(1),
        
        CONSTRAINT [PK_TenderBidDocuments] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_TenderBidDocuments_TenderBids] FOREIGN KEY ([TenderBidId]) REFERENCES [dbo].[TenderBids] ([Id]) ON DELETE CASCADE
    )
    
    CREATE NONCLUSTERED INDEX [IX_TenderBidDocuments_TenderBidId] ON [dbo].[TenderBidDocuments] ([TenderBidId]) WHERE [IsActive] = 1
    
    PRINT 'TenderBidDocuments table created successfully'
END
ELSE
BEGIN
    PRINT 'TenderBidDocuments table already exists'
END
GO

PRINT 'All tender management tables created successfully!'
