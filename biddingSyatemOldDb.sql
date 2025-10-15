USE [AzureDevOps_BiddingSystems]
GO
/****** Object:  Table [dbo].[PaymentTransactions]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentTransactions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PaymentLinkId] [nvarchar](50) NOT NULL,
	[TenderId] [int] NOT NULL,
	[TenderBidId] [int] NULL,
	[RazorpayPaymentId] [nvarchar](100) NULL,
	[RazorpayOrderId] [nvarchar](100) NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[PaymentMethod] [nvarchar](50) NULL,
	[BankName] [nvarchar](100) NULL,
	[CardLast4] [nvarchar](4) NULL,
	[UPIId] [nvarchar](100) NULL,
	[WalletName] [nvarchar](50) NULL,
	[ErrorCode] [nvarchar](100) NULL,
	[ErrorDescription] [nvarchar](500) NULL,
	[CustomerEmail] [nvarchar](100) NULL,
	[CustomerPhone] [nvarchar](20) NULL,
	[TransactionDate] [datetime2](7) NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_PaymentTransactions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RefundPayments]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RefundPayments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TenderBidId] [int] NOT NULL,
	[TenderId] [int] NOT NULL,
	[RefundAmount] [decimal](18, 2) NOT NULL,
	[ReasonForRefund] [nvarchar](500) NOT NULL,
	[RefundStatus] [nvarchar](20) NOT NULL,
	[InitiatedBy] [int] NOT NULL,
	[InitiatedAt] [datetime2](7) NOT NULL,
	[ApprovedBy] [int] NULL,
	[ApprovedAt] [datetime2](7) NULL,
	[CheckerRemarks] [nvarchar](500) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[RazorpayRefundId] [nvarchar](100) NULL,
	[RefundErrorMessage] [nvarchar](500) NULL,
	[RefundProcessedAt] [datetime2](7) NULL,
	[PaymentLinkId] [int] NULL,
	[PaymentType] [varchar](50) NULL,
 CONSTRAINT [PK_RefundPayments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tenders]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
	[Status] [int] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[PublishedAt] [datetime2](7) NULL,
	[IsActive] [bit] NOT NULL,
	[AllocatedBidId] [int] NULL,
	[AllocatedAt] [datetime2](7) NULL,
	[AllocatedBy] [int] NULL,
	[AllocationRemarks] [nvarchar](500) NULL,
 CONSTRAINT [PK_Tenders] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TenderBids]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
	[Status] [nvarchar](100) NULL,
	[PaymentStatus] [nvarchar](100) NULL,
	[PaymentReference] [nvarchar](100) NULL,
	[PaymentDate] [datetime2](7) NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[SubmittedAt] [datetime2](7) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime] NULL,
	[Remarks] [nvarchar](max) NULL,
	[IsAllocated] [bit] NOT NULL,
	[AllocationDate] [datetime2](7) NULL,
	[AllocationRemarks] [nvarchar](500) NULL,
 CONSTRAINT [PK_TenderBids] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[Username] [nvarchar](50) NOT NULL,
	[PasswordHash] [nvarchar](500) NOT NULL,
	[Salt] [nvarchar](500) NOT NULL,
	[Role] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[ResetPasswordToken] [nvarchar](500) NULL,
	[ResetPasswordExpires] [datetime2](7) NULL,
	[PhoneNumber] [nvarchar](20) NULL,
	[Department] [nvarchar](100) NULL,
	[CreatedBy] [int] NULL,
	[LastLoginAt] [datetime2](7) NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UK_Users_Email] UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UK_Users_Username] UNIQUE NONCLUSTERED 
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_RefundMonitoring]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_RefundMonitoring]
AS
SELECT 
    rp.Id AS RefundPaymentId,
    t.TenderId AS TenderIdString,
    t.TenderTitle,
    tb.BidderName,
    tb.BidderEmail,
    tb.CompanyName,
    rp.RefundAmount,
    rp.RefundStatus,
    rp.RazorpayRefundId,
    rp.InitiatedAt,
    rp.ApprovedAt,
    rp.RefundErrorMessage,
    pt.RazorpayPaymentId AS OriginalPaymentId,
    pt.Amount AS OriginalAmount,
    u1.Username AS InitiatedByUser,
    u2.Username AS ApprovedByUser
FROM RefundPayments rp
INNER JOIN TenderBids tb ON rp.TenderBidId = tb.Id
INNER JOIN Tenders t ON rp.TenderId = t.Id
LEFT JOIN PaymentTransactions pt ON 
    pt.TenderBidId = rp.TenderBidId 
    AND pt.Status = 'Success'
    AND pt.PaymentMethod != 'REFUND'
LEFT JOIN Users u1 ON rp.InitiatedBy = u1.Id
LEFT JOIN Users u2 ON rp.ApprovedBy = u2.Id;
GO
/****** Object:  View [dbo].[vw_EligibleBiddersForRefund]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- View to get eligible bidders for refund (Maker view)
CREATE   VIEW [dbo].[vw_EligibleBiddersForRefund] AS
SELECT 
    tb.Id as TenderBidId,
    t.Id as TenderId,
    t.TenderId as TenderIdString,
    t.TenderTitle,
    tb.BidderName,
    tb.BidderEmail,
    tb.BidderPhone,
    tb.CompanyName,
    tb.BidAmount,
    tb.EmdAmount,
    tb.ProcessingFee,
    tb.TotalAmount,
    tb.PaymentDate,
    tb.PaymentReference,
    tb.Status as BidStatus,
    tb.PaymentStatus,
    t.Status as TenderStatus,
    rp.Id as RefundPaymentId,
    rp.RefundStatus,
    CASE 
        WHEN rp.Id IS NOT NULL THEN 1 
        ELSE 0 
    END as HasPendingRefund
FROM TenderBids tb
INNER JOIN Tenders t ON tb.TenderId = t.Id
LEFT JOIN RefundPayments rp ON tb.Id = rp.TenderBidId 
    AND rp.RefundStatus IN ('Pending', 'Approved')
WHERE tb.Status = 'Rejected'
    AND tb.PaymentStatus = 'Paid'
    AND t.Status = 3
    AND tb.IsActive = 1;
GO
/****** Object:  View [dbo].[vw_PendingRefundApprovals]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- View to get pending refunds for approval (Checker view)
CREATE   VIEW [dbo].[vw_PendingRefundApprovals] AS
SELECT 
    rp.Id as RefundPaymentId,
    rp.TenderBidId,
    rp.TenderId,
    rp.RefundAmount,
    rp.ReasonForRefund,
    rp.RefundStatus,
    rp.InitiatedAt,
    rp.InitiatedBy,
    t.TenderId as TenderIdString,
    t.TenderTitle,
    tb.BidderName,
    tb.BidderEmail,
    tb.CompanyName,
    tb.TotalAmount,
    u.Username as InitiatedByName
FROM RefundPayments rp
INNER JOIN TenderBids tb ON rp.TenderBidId = tb.Id
INNER JOIN Tenders t ON rp.TenderId = t.Id
LEFT JOIN Users u ON rp.InitiatedBy = u.Id
WHERE rp.RefundStatus = 'Pending';
GO
/****** Object:  View [dbo].[vw_ActiveUsers]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Views for easier querying
CREATE VIEW [dbo].[vw_ActiveUsers] AS
SELECT 
    u.Id,
    u.FirstName,
    u.LastName,
    u.FirstName + ' ' + u.LastName AS FullName,
    u.Email,
    u.Username,
    u.Role,
    CASE u.Role 
        WHEN 1 THEN 'Employee'
        WHEN 2 THEN 'Vendor'
        WHEN 3 THEN 'Admin'
        ELSE 'Unknown'
    END AS RoleName,
    u.PhoneNumber,
    u.Department,
    u.IsActive,
    u.CreatedAt,
    u.UpdatedAt,
    u.LastLoginAt,
    creator.FirstName + ' ' + creator.LastName AS CreatedByName
FROM [dbo].[Users] u
LEFT JOIN [dbo].[Users] creator ON u.CreatedBy = creator.Id
WHERE u.IsActive = 1;
GO
/****** Object:  Table [dbo].[TenderDocuments]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TenderDocuments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TenderId] [int] NOT NULL,
	[DocumentName] [nvarchar](255) NOT NULL,
	[FileName] [nvarchar](255) NOT NULL,
	[FilePath] [nvarchar](500) NOT NULL,
	[FileSize] [bigint] NOT NULL,
	[ContentType] [nvarchar](100) NOT NULL,
	[DocumentType] [int] NOT NULL,
	[IsRequired] [bit] NOT NULL,
	[UploadedBy] [int] NOT NULL,
	[UploadedAt] [datetime2](7) NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_TenderDocuments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_TenderSummary]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_TenderSummary]
AS
SELECT 
    t.Id,
    t.TenderId,
    t.TenderTitle,
    t.Department,
    t.Status,
    t.PublishDate,
    t.LastDateEmd,
    t.EmdAmount,
    t.ProcessingFee,
    t.EstimatedValue,
    t.CreatedAt,
    t.PublishedAt,
    u.FirstName + ' ' + u.LastName as CreatedByUserName,
    u.Email as CreatedByEmail,
    (SELECT COUNT(1) FROM TenderBids WHERE TenderId = t.Id AND IsActive = 1) as BidCount,
    (SELECT COUNT(1) FROM TenderBids WHERE TenderId = t.Id AND PaymentStatus = 2 AND IsActive = 1) as PaidBidCount,
    (SELECT COUNT(1) FROM TenderDocuments WHERE TenderId = t.Id AND IsActive = 1) as DocumentCount,
    CASE 
        WHEN t.Status = 1 THEN 'Draft'
        WHEN t.Status = 2 THEN 'Published'
        WHEN t.Status = 3 THEN 'Closed'
        WHEN t.Status = 4 THEN 'Cancelled'
        ELSE 'Unknown'
    END as StatusName,
    CASE 
        WHEN t.LastDateEmd < GETUTCDATE() AND t.Status = 2 THEN 'Expired'
        WHEN t.LastDateEmd <= DATEADD(day, 7, GETUTCDATE()) AND t.Status = 2 THEN 'Expiring Soon'
        WHEN t.Status = 2 THEN 'Active'
        ELSE 'Inactive'
    END as TenderStatus
FROM Tenders t
LEFT JOIN Users u ON t.CreatedBy = u.Id
WHERE t.IsActive = 1;
GO
/****** Object:  View [dbo].[vw_BidSummary]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_BidSummary]
AS
SELECT 
    tb.Id,
    tb.TenderId,
    t.TenderTitle,
    t.TenderId as TenderNumber,
    tb.BidderName,
    tb.BidderEmail,
    tb.BidderPhone,
    tb.CompanyName,
    tb.BidAmount,
    tb.EmdAmount,
    tb.ProcessingFee,
    tb.TotalAmount,
    tb.Status,
    tb.PaymentStatus,
    tb.PaymentReference,
    tb.PaymentDate,
    tb.SubmittedAt,
    CASE 
        WHEN tb.Status = 1 THEN 'Submitted'
        WHEN tb.Status = 2 THEN 'Under Review'
        WHEN tb.Status = 3 THEN 'Accepted'
        WHEN tb.Status = 4 THEN 'Rejected'
        WHEN tb.Status = 5 THEN 'Withdrawn'
        ELSE 'Unknown'
    END as StatusName,
    CASE 
        WHEN tb.PaymentStatus = 1 THEN 'Pending'
        WHEN tb.PaymentStatus = 2 THEN 'Paid'
        WHEN tb.PaymentStatus = 3 THEN 'Failed'
        WHEN tb.PaymentStatus = 4 THEN 'Refunded'
        ELSE 'Unknown'
    END as PaymentStatusName
FROM TenderBids tb
INNER JOIN Tenders t ON tb.TenderId = t.Id
WHERE tb.IsActive = 1 AND t.IsActive = 1;
GO
/****** Object:  Table [dbo].[EMDSDDeposits_bk]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EMDSDDeposits_bk](
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
	[Status] [nvarchar](20) NOT NULL,
	[Type] [nvarchar](10) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[Remarks] [nvarchar](500) NULL,
	[TenderBidId] [int] NULL,
 CONSTRAINT [PK_EMDSDDeposits] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EMDSDTransactions_bk]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EMDSDTransactions_bk](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EMDSDDepositId] [int] NOT NULL,
	[TransactionReference] [nvarchar](50) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[TransactionType] [nvarchar](20) NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[TransactionDate] [datetime2](7) NOT NULL,
	[BankReference] [nvarchar](100) NULL,
	[Remarks] [nvarchar](500) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[CreatedBy] [nvarchar](100) NULL,
 CONSTRAINT [PK_EMDSDTransactions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentLinks]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentLinks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LinkId] [nvarchar](50) NOT NULL,
	[TenderId] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[PaymentType] [int] NOT NULL,
	[PaymentUrl] [nvarchar](500) NOT NULL,
	[SecurityToken] [nvarchar](64) NOT NULL,
	[Status] [int] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ExpiryDate] [datetime2](7) NOT NULL,
	[UsedDate] [datetime2](7) NULL,
	[UsedBy] [int] NULL,
	[TransactionId] [nvarchar](100) NULL,
	[Notes] [nvarchar](500) NULL,
	[CreatedBy] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[TenderBidId] [int] NULL,
	[RefundId] [varchar](100) NULL,
	[RefundStatus] [varchar](50) NULL,
	[RefundDate] [datetime] NULL,
	[RefundAmount] [decimal](10, 2) NULL,
	[RefundReason] [varchar](500) NULL,
	[RefundInitiatedBy] [int] NULL,
	[RefundApprovedBy] [int] NULL,
	[RefundErrorMessage] [nvarchar](500) NULL,
 CONSTRAINT [PK_PaymentLinks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RefundApprovalHistory]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RefundApprovalHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RefundRequestId] [int] NOT NULL,
	[Action] [nvarchar](50) NOT NULL,
	[ActionBy] [int] NOT NULL,
	[ActionAt] [datetime] NOT NULL,
	[Comments] [nvarchar](1000) NULL,
	[OldStatus] [nvarchar](50) NULL,
	[NewStatus] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RefundRequests]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RefundRequests](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TenderBidId] [int] NOT NULL,
	[TenderId] [int] NOT NULL,
	[BidderName] [nvarchar](200) NOT NULL,
	[CompanyName] [nvarchar](300) NOT NULL,
	[RefundAmount] [decimal](18, 2) NOT NULL,
	[EmdAmount] [decimal](18, 2) NOT NULL,
	[ProcessingFee] [decimal](18, 2) NOT NULL,
	[TotalRefundAmount] [decimal](18, 2) NOT NULL,
	[RefundReason] [nvarchar](500) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[ApprovalStatus] [nvarchar](50) NULL,
	[PaymentReference] [nvarchar](100) NULL,
	[TransactionReference] [nvarchar](100) NULL,
	[RefundDate] [datetime] NULL,
	[RequestedBy] [int] NOT NULL,
	[RequestedAt] [datetime] NOT NULL,
	[ApprovedBy] [int] NULL,
	[ApprovedAt] [datetime] NULL,
	[RejectionReason] [nvarchar](500) NULL,
	[Remarks] [nvarchar](1000) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RefundRequests_bk]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RefundRequests_bk](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RefundId] [nvarchar](50) NOT NULL,
	[TenderBidId] [int] NOT NULL,
	[PaymentLinkId] [int] NOT NULL,
	[Type] [nvarchar](20) NOT NULL,
	[RequestedAmount] [decimal](18, 2) NOT NULL,
	[ApprovedAmount] [decimal](18, 2) NULL,
	[Reason] [nvarchar](50) NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[RequestedBy] [nvarchar](200) NOT NULL,
	[ProcessedBy] [int] NULL,
	[RequestedAt] [datetime2](7) NOT NULL,
	[ProcessedAt] [datetime2](7) NULL,
	[Remarks] [nvarchar](500) NULL,
	[BankDetails] [nvarchar](1000) NULL,
	[RefundReference] [nvarchar](100) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[CreatedBy] [int] NULL,
	[EMDSDDepositId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[RefundId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[RefundId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RefundTransactions_bk]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RefundTransactions_bk](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RefundRequestId] [int] NOT NULL,
	[TransactionReference] [nvarchar](100) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[ProcessedAt] [datetime2](7) NOT NULL,
	[BankResponse] [nvarchar](1000) NULL,
	[FailureReason] [nvarchar](500) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[CreatedBy] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TenderBidDocuments]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TenderBidDocuments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TenderBidId] [int] NOT NULL,
	[DocumentName] [nvarchar](255) NOT NULL,
	[FileName] [nvarchar](255) NOT NULL,
	[FilePath] [nvarchar](500) NOT NULL,
	[FileSize] [bigint] NOT NULL,
	[ContentType] [nvarchar](100) NOT NULL,
	[DocumentType] [int] NOT NULL,
	[IsRequired] [bit] NOT NULL,
	[UploadedAt] [datetime2](7) NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_TenderBidDocuments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserAuditLog]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserAuditLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[Action] [nvarchar](50) NOT NULL,
	[Details] [nvarchar](500) NULL,
	[PerformedBy] [int] NULL,
	[IpAddress] [nvarchar](45) NULL,
	[UserAgent] [nvarchar](500) NULL,
	[Timestamp] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_UserAuditLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WebhookEvents]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WebhookEvents](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EventType] [nvarchar](50) NOT NULL,
	[RazorpayPaymentId] [nvarchar](100) NULL,
	[RazorpayOrderId] [nvarchar](100) NULL,
	[PaymentLinkId] [nvarchar](50) NULL,
	[Amount] [decimal](18, 2) NULL,
	[Status] [nvarchar](50) NULL,
	[ProcessedSuccessfully] [bit] NULL,
	[ErrorMessage] [nvarchar](500) NULL,
	[ReceivedAt] [datetime2](7) NOT NULL,
	[ProcessedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_WebhookEvents] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[EMDSDDeposits_bk] ADD  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [dbo].[EMDSDDeposits_bk] ADD  DEFAULT ('EMD') FOR [Type]
GO
ALTER TABLE [dbo].[EMDSDDeposits_bk] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[EMDSDTransactions_bk] ADD  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [dbo].[EMDSDTransactions_bk] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[PaymentLinks] ADD  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[PaymentLinks] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[PaymentLinks] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[PaymentTransactions] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RefundPayments] ADD  DEFAULT ('Tender awarded to another bidder') FOR [ReasonForRefund]
GO
ALTER TABLE [dbo].[RefundPayments] ADD  DEFAULT ('Pending') FOR [RefundStatus]
GO
ALTER TABLE [dbo].[RefundPayments] ADD  DEFAULT ((1)) FOR [InitiatedBy]
GO
ALTER TABLE [dbo].[RefundPayments] ADD  DEFAULT (getutcdate()) FOR [InitiatedAt]
GO
ALTER TABLE [dbo].[RefundPayments] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RefundRequests] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[RefundRequests] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RefundRequests_bk] ADD  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [dbo].[RefundRequests_bk] ADD  DEFAULT (getutcdate()) FOR [RequestedAt]
GO
ALTER TABLE [dbo].[RefundRequests_bk] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RefundTransactions_bk] ADD  DEFAULT (getutcdate()) FOR [ProcessedAt]
GO
ALTER TABLE [dbo].[RefundTransactions_bk] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[TenderBidDocuments] ADD  DEFAULT ((1)) FOR [DocumentType]
GO
ALTER TABLE [dbo].[TenderBidDocuments] ADD  DEFAULT ((0)) FOR [IsRequired]
GO
ALTER TABLE [dbo].[TenderBidDocuments] ADD  DEFAULT (getutcdate()) FOR [UploadedAt]
GO
ALTER TABLE [dbo].[TenderBidDocuments] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[TenderBids] ADD  CONSTRAINT [DF__TenderBid__Statu__403A8C7D]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[TenderBids] ADD  CONSTRAINT [DF__TenderBid__Payme__412EB0B6]  DEFAULT ((1)) FOR [PaymentStatus]
GO
ALTER TABLE [dbo].[TenderBids] ADD  CONSTRAINT [DF__TenderBid__Submi__4222D4EF]  DEFAULT (getutcdate()) FOR [SubmittedAt]
GO
ALTER TABLE [dbo].[TenderBids] ADD  CONSTRAINT [DF__TenderBid__IsAct__4316F928]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[TenderBids] ADD  DEFAULT ((0)) FOR [IsAllocated]
GO
ALTER TABLE [dbo].[TenderDocuments] ADD  DEFAULT ((1)) FOR [DocumentType]
GO
ALTER TABLE [dbo].[TenderDocuments] ADD  DEFAULT ((0)) FOR [IsRequired]
GO
ALTER TABLE [dbo].[TenderDocuments] ADD  DEFAULT (getutcdate()) FOR [UploadedAt]
GO
ALTER TABLE [dbo].[TenderDocuments] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Tenders] ADD  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[Tenders] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Tenders] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[UserAuditLog] ADD  DEFAULT (getutcdate()) FOR [Timestamp]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [Role]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[WebhookEvents] ADD  DEFAULT ((0)) FOR [ProcessedSuccessfully]
GO
ALTER TABLE [dbo].[EMDSDDeposits_bk]  WITH CHECK ADD  CONSTRAINT [FK_EMDSDDeposits_Tenders] FOREIGN KEY([TenderId])
REFERENCES [dbo].[Tenders] ([Id])
GO
ALTER TABLE [dbo].[EMDSDDeposits_bk] CHECK CONSTRAINT [FK_EMDSDDeposits_Tenders]
GO
ALTER TABLE [dbo].[EMDSDTransactions_bk]  WITH CHECK ADD  CONSTRAINT [FK_EMDSDTransactions_EMDSDDeposits] FOREIGN KEY([EMDSDDepositId])
REFERENCES [dbo].[EMDSDDeposits_bk] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EMDSDTransactions_bk] CHECK CONSTRAINT [FK_EMDSDTransactions_EMDSDDeposits]
GO
ALTER TABLE [dbo].[PaymentLinks]  WITH CHECK ADD  CONSTRAINT [FK_PaymentLinks_Tenders] FOREIGN KEY([TenderId])
REFERENCES [dbo].[Tenders] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PaymentLinks] CHECK CONSTRAINT [FK_PaymentLinks_Tenders]
GO
ALTER TABLE [dbo].[PaymentLinks]  WITH CHECK ADD  CONSTRAINT [FK_PaymentLinks_Users_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[PaymentLinks] CHECK CONSTRAINT [FK_PaymentLinks_Users_CreatedBy]
GO
ALTER TABLE [dbo].[PaymentLinks]  WITH CHECK ADD  CONSTRAINT [FK_PaymentLinks_Users_UsedBy] FOREIGN KEY([UsedBy])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[PaymentLinks] CHECK CONSTRAINT [FK_PaymentLinks_Users_UsedBy]
GO
ALTER TABLE [dbo].[RefundApprovalHistory]  WITH CHECK ADD FOREIGN KEY([ActionBy])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[RefundApprovalHistory]  WITH CHECK ADD FOREIGN KEY([ActionBy])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[RefundApprovalHistory]  WITH CHECK ADD FOREIGN KEY([RefundRequestId])
REFERENCES [dbo].[RefundRequests] ([Id])
GO
ALTER TABLE [dbo].[RefundApprovalHistory]  WITH CHECK ADD FOREIGN KEY([RefundRequestId])
REFERENCES [dbo].[RefundRequests] ([Id])
GO
ALTER TABLE [dbo].[RefundPayments]  WITH CHECK ADD  CONSTRAINT [FK_RefundPayments_Tender] FOREIGN KEY([TenderId])
REFERENCES [dbo].[Tenders] ([Id])
GO
ALTER TABLE [dbo].[RefundPayments] CHECK CONSTRAINT [FK_RefundPayments_Tender]
GO
ALTER TABLE [dbo].[RefundPayments]  WITH CHECK ADD  CONSTRAINT [FK_RefundPayments_TenderBid] FOREIGN KEY([TenderBidId])
REFERENCES [dbo].[TenderBids] ([Id])
GO
ALTER TABLE [dbo].[RefundPayments] CHECK CONSTRAINT [FK_RefundPayments_TenderBid]
GO
ALTER TABLE [dbo].[RefundRequests]  WITH CHECK ADD FOREIGN KEY([ApprovedBy])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[RefundRequests]  WITH CHECK ADD FOREIGN KEY([ApprovedBy])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[RefundRequests]  WITH CHECK ADD FOREIGN KEY([RequestedBy])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[RefundRequests]  WITH CHECK ADD FOREIGN KEY([RequestedBy])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[RefundRequests]  WITH CHECK ADD FOREIGN KEY([TenderBidId])
REFERENCES [dbo].[TenderBids] ([Id])
GO
ALTER TABLE [dbo].[RefundRequests]  WITH CHECK ADD FOREIGN KEY([TenderId])
REFERENCES [dbo].[Tenders] ([Id])
GO
ALTER TABLE [dbo].[RefundRequests]  WITH CHECK ADD FOREIGN KEY([TenderBidId])
REFERENCES [dbo].[TenderBids] ([Id])
GO
ALTER TABLE [dbo].[RefundRequests]  WITH CHECK ADD FOREIGN KEY([TenderId])
REFERENCES [dbo].[Tenders] ([Id])
GO
ALTER TABLE [dbo].[RefundRequests_bk]  WITH CHECK ADD  CONSTRAINT [FK_RefundRequests_EMDSDDeposits] FOREIGN KEY([EMDSDDepositId])
REFERENCES [dbo].[EMDSDDeposits_bk] ([Id])
GO
ALTER TABLE [dbo].[RefundRequests_bk] CHECK CONSTRAINT [FK_RefundRequests_EMDSDDeposits]
GO
ALTER TABLE [dbo].[RefundRequests_bk]  WITH CHECK ADD  CONSTRAINT [FK_RefundRequests_PaymentLinks] FOREIGN KEY([PaymentLinkId])
REFERENCES [dbo].[PaymentLinks] ([Id])
GO
ALTER TABLE [dbo].[RefundRequests_bk] CHECK CONSTRAINT [FK_RefundRequests_PaymentLinks]
GO
ALTER TABLE [dbo].[RefundRequests_bk]  WITH CHECK ADD  CONSTRAINT [FK_RefundRequests_TenderBids] FOREIGN KEY([TenderBidId])
REFERENCES [dbo].[TenderBids] ([Id])
GO
ALTER TABLE [dbo].[RefundRequests_bk] CHECK CONSTRAINT [FK_RefundRequests_TenderBids]
GO
ALTER TABLE [dbo].[RefundRequests_bk]  WITH CHECK ADD  CONSTRAINT [FK_RefundRequests_Users] FOREIGN KEY([ProcessedBy])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[RefundRequests_bk] CHECK CONSTRAINT [FK_RefundRequests_Users]
GO
ALTER TABLE [dbo].[RefundTransactions_bk]  WITH CHECK ADD  CONSTRAINT [FK_RefundTransactions_RefundRequests] FOREIGN KEY([RefundRequestId])
REFERENCES [dbo].[RefundRequests_bk] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RefundTransactions_bk] CHECK CONSTRAINT [FK_RefundTransactions_RefundRequests]
GO
ALTER TABLE [dbo].[TenderBidDocuments]  WITH CHECK ADD  CONSTRAINT [FK_TenderBidDocuments_TenderBids] FOREIGN KEY([TenderBidId])
REFERENCES [dbo].[TenderBids] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TenderBidDocuments] CHECK CONSTRAINT [FK_TenderBidDocuments_TenderBids]
GO
ALTER TABLE [dbo].[TenderBids]  WITH CHECK ADD  CONSTRAINT [FK_TenderBids_Tenders] FOREIGN KEY([TenderId])
REFERENCES [dbo].[Tenders] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TenderBids] CHECK CONSTRAINT [FK_TenderBids_Tenders]
GO
ALTER TABLE [dbo].[TenderDocuments]  WITH CHECK ADD  CONSTRAINT [FK_TenderDocuments_Tenders] FOREIGN KEY([TenderId])
REFERENCES [dbo].[Tenders] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TenderDocuments] CHECK CONSTRAINT [FK_TenderDocuments_Tenders]
GO
ALTER TABLE [dbo].[TenderDocuments]  WITH CHECK ADD  CONSTRAINT [FK_TenderDocuments_Users] FOREIGN KEY([UploadedBy])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[TenderDocuments] CHECK CONSTRAINT [FK_TenderDocuments_Users]
GO
ALTER TABLE [dbo].[Tenders]  WITH CHECK ADD  CONSTRAINT [FK_Tenders_Users] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Tenders] CHECK CONSTRAINT [FK_Tenders_Users]
GO
ALTER TABLE [dbo].[UserAuditLog]  WITH CHECK ADD  CONSTRAINT [FK_UserAuditLog_PerformedBy] FOREIGN KEY([PerformedBy])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[UserAuditLog] CHECK CONSTRAINT [FK_UserAuditLog_PerformedBy]
GO
ALTER TABLE [dbo].[UserAuditLog]  WITH CHECK ADD  CONSTRAINT [FK_UserAuditLog_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[UserAuditLog] CHECK CONSTRAINT [FK_UserAuditLog_UserId]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [FK_Users_CreatedBy]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [CK_Users_Role] CHECK  (([Role]=(3) OR [Role]=(2) OR [Role]=(1)))
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [CK_Users_Role]
GO
/****** Object:  StoredProcedure [dbo].[sp_GetExpiringTenders]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GetExpiringTenders]
    @Days INT = 7
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        t.*,
        u.FirstName + ' ' + u.LastName as CreatedByUserName,
        DATEDIFF(day, GETUTCDATE(), t.LastDateEmd) as DaysToExpiry
    FROM Tenders t
    LEFT JOIN Users u ON t.CreatedBy = u.Id
    WHERE t.LastDateEmd <= DATEADD(day, @Days, GETUTCDATE()) 
        AND t.Status = 2 
        AND t.IsActive = 1
    ORDER BY t.LastDateEmd ASC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetTenderBidsSummary]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GetTenderBidsSummary]
    @TenderId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        COUNT(1) as TotalBids,
        COUNT(CASE WHEN PaymentStatus = 2 THEN 1 END) as PaidBids,
        COUNT(CASE WHEN PaymentStatus = 1 THEN 1 END) as PendingBids,
        ISNULL(SUM(CASE WHEN PaymentStatus = 2 THEN EmdAmount ELSE 0 END), 0) as TotalEmdCollected,
        ISNULL(SUM(CASE WHEN PaymentStatus = 2 THEN ProcessingFee ELSE 0 END), 0) as TotalProcessingFeesCollected,
        ISNULL(AVG(CASE WHEN PaymentStatus = 2 THEN BidAmount END), 0) as AverageBidAmount,
        ISNULL(MIN(CASE WHEN PaymentStatus = 2 THEN BidAmount END), 0) as LowestBidAmount,
        ISNULL(MAX(CASE WHEN PaymentStatus = 2 THEN BidAmount END), 0) as HighestBidAmount
    FROM TenderBids 
    WHERE TenderId = @TenderId AND IsActive = 1;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetTendersByStatus]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GetTendersByStatus]
    @Status INT,
    @Page INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    
    SELECT 
        t.*,
        u.FirstName + ' ' + u.LastName as CreatedByUserName,
        u.Email as CreatedByEmail,
        (SELECT COUNT(1) FROM TenderBids WHERE TenderId = t.Id AND IsActive = 1) as BidCount,
        (SELECT COUNT(1) FROM TenderDocuments WHERE TenderId = t.Id AND IsActive = 1) as DocumentCount
    FROM Tenders t
    LEFT JOIN Users u ON t.CreatedBy = u.Id
    WHERE t.Status = @Status AND t.IsActive = 1
    ORDER BY t.CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(1) as TotalCount
    FROM Tenders 
    WHERE Status = @Status AND IsActive = 1;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetTenderStatistics]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GetTenderStatistics]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        (SELECT COUNT(1) FROM Tenders WHERE IsActive = 1) as TotalTenders,
        (SELECT COUNT(1) FROM Tenders WHERE Status = 2 AND IsActive = 1) as PublishedTenders,
        (SELECT COUNT(1) FROM Tenders WHERE Status = 1 AND IsActive = 1) as DraftTenders,
        (SELECT COUNT(1) FROM Tenders WHERE Status = 3 AND IsActive = 1) as ClosedTenders,
        (SELECT COUNT(1) FROM TenderBids WHERE IsActive = 1) as TotalBids,
        (SELECT COUNT(1) FROM TenderBids WHERE PaymentStatus = 2 AND IsActive = 1) as PaidBids,
        (SELECT ISNULL(SUM(EmdAmount), 0) FROM TenderBids WHERE PaymentStatus = 2 AND IsActive = 1) as TotalEmdCollected,
        (SELECT ISNULL(SUM(ProcessingFee), 0) FROM TenderBids WHERE PaymentStatus = 2 AND IsActive = 1) as TotalProcessingFeesCollected,
        (SELECT ISNULL(SUM(BidAmount), 0) FROM TenderBids WHERE PaymentStatus = 2 AND IsActive = 1) as TotalBidAmount
END
GO
/****** Object:  StoredProcedure [dbo].[sp_InitiateRefunds]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Stored Procedure to initiate refunds in bulk
CREATE   PROCEDURE [dbo].[sp_InitiateRefunds]
    @TenderBidIds NVARCHAR(MAX), -- Comma-separated list of IDs
    @ReasonForRefund NVARCHAR(500),
    @InitiatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Create temp table to hold IDs
    CREATE TABLE #TempIds (Id INT);
    
    -- Parse comma-separated IDs
    INSERT INTO #TempIds (Id)
    SELECT value FROM STRING_SPLIT(@TenderBidIds, ',');
    
    -- Insert refunds for each bid
    INSERT INTO RefundPayments (TenderBidId, TenderId, RefundAmount, ReasonForRefund, RefundStatus, InitiatedBy)
    SELECT 
        tb.Id,
        tb.TenderId,
        tb.TotalAmount,
        @ReasonForRefund,
        'Pending',
        @InitiatedBy
    FROM TenderBids tb
    INNER JOIN #TempIds t ON tb.Id = t.Id
    WHERE NOT EXISTS (
        SELECT 1 FROM RefundPayments rp 
        WHERE rp.TenderBidId = tb.Id 
        AND rp.RefundStatus IN ('Pending', 'Approved')
    );
    
    DROP TABLE #TempIds;
    
    SELECT @@ROWCOUNT as RecordsCreated;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ProcessRefunds]    Script Date: 01-10-2025 05:01:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Stored Procedure to approve/reject refunds in bulk
CREATE   PROCEDURE [dbo].[sp_ProcessRefunds]
    @RefundPaymentIds NVARCHAR(MAX), -- Comma-separated list of IDs
    @Action NVARCHAR(20), -- 'Approve' or 'Reject'
    @CheckerRemarks NVARCHAR(500),
    @ApprovedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Create temp table to hold IDs
        CREATE TABLE #TempIds (Id INT);
        
        -- Parse comma-separated IDs
        INSERT INTO #TempIds (Id)
        SELECT value FROM STRING_SPLIT(@RefundPaymentIds, ',');
        
        DECLARE @Status NVARCHAR(20) = CASE WHEN @Action = 'Approve' THEN 'Approved' ELSE 'Rejected' END;
        
        -- Update refund status
        UPDATE rp
        SET 
            RefundStatus = @Status,
            ApprovedBy = @ApprovedBy,
            ApprovedAt = GETUTCDATE(),
            CheckerRemarks = @CheckerRemarks
        FROM RefundPayments rp
        INNER JOIN #TempIds t ON rp.Id = t.Id
        WHERE rp.RefundStatus = 'Pending';
        
        -- If approved, update TenderBid payment status
        IF @Status = 'Approved'
        BEGIN
            UPDATE tb
            SET 
                PaymentStatus = 'Refunded',
                UpdatedAt = GETUTCDATE()
            FROM TenderBids tb
            INNER JOIN RefundPayments rp ON tb.Id = rp.TenderBidId
            INNER JOIN #TempIds t ON rp.Id = t.Id;
        END
        
        DROP TABLE #TempIds;
        
        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT as RecordsProcessed;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END

GO
