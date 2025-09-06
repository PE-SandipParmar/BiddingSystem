-- Create EMD/SD Management Tables
-- Run this script in your SQL Server database

USE [AzureDevOps_BiddingSystem]
GO

-- Create EMDSDDeposits Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='EMDSDDeposits' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[EMDSDDeposits](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [DepositId] [nvarchar](50) NOT NULL,
        [TenderId] [int] NOT NULL,
        [Amount] [decimal](18, 2) NOT NULL,
        [BidderName] [nvarchar](100) NOT NULL,
        [CompanyName] [nvarchar](200) NOT NULL,
        [TransactionDate] [datetime2](7) NOT NULL,
        [BankName] [nvarchar](100) NOT NULL,
        [FSSAIBranchName] [nvarchar](100) NOT NULL,
        [TransactionId] [nvarchar](50) NOT NULL,
        [Status] [nvarchar](20) NOT NULL DEFAULT('Pending'),
        [Type] [nvarchar](10) NOT NULL DEFAULT('EMD'),
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        [UpdatedAt] [datetime2](7) NULL,
        [Remarks] [nvarchar](500) NULL,
        
        CONSTRAINT [PK_EMDSDDeposits] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_EMDSDDeposits_Tenders] FOREIGN KEY ([TenderId]) REFERENCES [dbo].[Tenders] ([Id])
    )
    
    CREATE UNIQUE NONCLUSTERED INDEX [IX_EMDSDDeposits_DepositId] ON [dbo].[EMDSDDeposits] ([DepositId])
    CREATE NONCLUSTERED INDEX [IX_EMDSDDeposits_TenderId] ON [dbo].[EMDSDDeposits] ([TenderId])
    CREATE NONCLUSTERED INDEX [IX_EMDSDDeposits_Status] ON [dbo].[EMDSDDeposits] ([Status])
    CREATE NONCLUSTERED INDEX [IX_EMDSDDeposits_Type] ON [dbo].[EMDSDDeposits] ([Type])
    CREATE NONCLUSTERED INDEX [IX_EMDSDDeposits_TransactionDate] ON [dbo].[EMDSDDeposits] ([TransactionDate])
    
    PRINT 'EMDSDDeposits table created successfully'
END
ELSE
BEGIN
    PRINT 'EMDSDDeposits table already exists'
END
GO

-- Create EMDSDTransactions Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='EMDSDTransactions' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[EMDSDTransactions](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [EMDSDDepositId] [int] NOT NULL,
        [TransactionReference] [nvarchar](50) NOT NULL,
        [Amount] [decimal](18, 2) NOT NULL,
        [TransactionType] [nvarchar](20) NOT NULL,
        [Status] [nvarchar](20) NOT NULL DEFAULT('Pending'),
        [TransactionDate] [datetime2](7) NOT NULL,
        [BankReference] [nvarchar](100) NULL,
        [Remarks] [nvarchar](500) NULL,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        [UpdatedAt] [datetime2](7) NULL,
        [CreatedBy] [nvarchar](100) NULL,
        
        CONSTRAINT [PK_EMDSDTransactions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_EMDSDTransactions_EMDSDDeposits] FOREIGN KEY ([EMDSDDepositId]) REFERENCES [dbo].[EMDSDDeposits] ([Id]) ON DELETE CASCADE
    )
    
    CREATE NONCLUSTERED INDEX [IX_EMDSDTransactions_EMDSDDepositId] ON [dbo].[EMDSDTransactions] ([EMDSDDepositId])
    CREATE NONCLUSTERED INDEX [IX_EMDSDTransactions_TransactionReference] ON [dbo].[EMDSDTransactions] ([TransactionReference])
    CREATE NONCLUSTERED INDEX [IX_EMDSDTransactions_Status] ON [dbo].[EMDSDTransactions] ([Status])
    CREATE NONCLUSTERED INDEX [IX_EMDSDTransactions_TransactionType] ON [dbo].[EMDSDTransactions] ([TransactionType])
    CREATE NONCLUSTERED INDEX [IX_EMDSDTransactions_TransactionDate] ON [dbo].[EMDSDTransactions] ([TransactionDate])
    
    PRINT 'EMDSDTransactions table created successfully'
END
ELSE
BEGIN
    PRINT 'EMDSDTransactions table already exists'
END
GO

-- Insert sample data for testing
IF NOT EXISTS (SELECT * FROM [dbo].[EMDSDDeposits] WHERE DepositId = 'EMD20250105001')
BEGIN
    -- Get the first tender ID for sample data
    DECLARE @FirstTenderId INT
    SELECT TOP 1 @FirstTenderId = Id FROM [dbo].[Tenders] WHERE IsActive = 1
    
    IF @FirstTenderId IS NOT NULL
    BEGIN
        INSERT INTO [dbo].[EMDSDDeposits] 
        ([DepositId], [TenderId], [Amount], [BidderName], [CompanyName], [TransactionDate], [BankName], [FSSAIBranchName], [TransactionId], [Status], [Type], [Remarks])
        VALUES 
        ('EMD20250105001', @FirstTenderId, 250000.00, 'Rahul Sharma', 'Techno Solutions Ltd.', '2025-01-05', 'State Bank of India', 'Delhi HQ', '9876543210', 'Paid', 'EMD', 'Sample EMD deposit'),
        ('EMD20250105002', @FirstTenderId, 1200000.00, 'Priya Verma', 'Innovative Builders', '2025-01-04', 'HDFC Bank', 'Mumbai Branch', '1234567890', 'Paid', 'EMD', 'Sample EMD deposit'),
        ('EMD20250105003', @FirstTenderId, 500000.00, 'Amit Singh', 'Nexus Technologies', '2025-01-03', 'ICICI Bank', 'Bangalore Branch', '1122334455', 'Failed', 'EMD', 'Sample EMD deposit'),
        ('EMD20250105004', @FirstTenderId, 180000.00, 'Sneha Kapoor', 'Global Safety Equipment', '2025-01-02', 'Axis Bank', 'Chennai Branch', '5566778899', 'Refunded', 'EMD', 'Sample EMD deposit'),
        ('EMD20250105005', @FirstTenderId, 320000.00, 'Vikas Mehra', 'Secure Solutions Pvt. Ltd.', '2025-01-06', 'Punjab National Bank', 'Kolkata Branch', '9988776655', 'Paid', 'EMD', 'Sample EMD deposit')
        
        PRINT 'Sample EMD/SD deposit data inserted successfully'
    END
    ELSE
    BEGIN
        PRINT 'No active tenders found. Please create a tender first before inserting sample EMD/SD data.'
    END
END
ELSE
BEGIN
    PRINT 'Sample EMD/SD deposit data already exists'
END
GO

PRINT 'All EMD/SD management tables created successfully!'
