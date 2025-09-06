-- Create TenderBids table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TenderBids' AND xtype='U')
BEGIN
    CREATE TABLE TenderBids (
        Id int IDENTITY(1,1) PRIMARY KEY,
        TenderId int NOT NULL,
        BidderName nvarchar(100) NOT NULL,
        BidderEmail nvarchar(100) NOT NULL,
        BidderPhone nvarchar(20) NOT NULL,
        CompanyName nvarchar(200) NOT NULL,
        CompanyAddress nvarchar(500) NOT NULL,
        BidAmount decimal(18,2) NOT NULL,
        EmdAmount decimal(18,2) NOT NULL,
        ProcessingFee decimal(18,2) NOT NULL,
        TotalAmount decimal(18,2) NOT NULL,
        Status nvarchar(20) NOT NULL DEFAULT 'Submitted',
        PaymentStatus nvarchar(20) NOT NULL DEFAULT 'Pending',
        PaymentReference nvarchar(100) NULL,
        PaymentDate datetime2 NULL,
        SubmittedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
        IsActive bit NOT NULL DEFAULT 1,
        CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt datetime2 NULL,
        Remarks nvarchar(500) NULL,
        
        CONSTRAINT FK_TenderBids_Tenders FOREIGN KEY (TenderId) REFERENCES Tenders(Id)
    );
    
    -- Create indexes for better performance
    CREATE INDEX IX_TenderBids_TenderId ON TenderBids(TenderId);
    CREATE INDEX IX_TenderBids_Status ON TenderBids(Status);
    CREATE INDEX IX_TenderBids_PaymentStatus ON TenderBids(PaymentStatus);
    CREATE INDEX IX_TenderBids_SubmittedAt ON TenderBids(SubmittedAt);
    CREATE INDEX IX_TenderBids_IsActive ON TenderBids(IsActive);
    CREATE INDEX IX_TenderBids_PaymentReference ON TenderBids(PaymentReference);
END
GO

-- Insert sample data
IF NOT EXISTS (SELECT 1 FROM TenderBids WHERE Id = 1)
BEGIN
    INSERT INTO TenderBids (
        TenderId, BidderName, BidderEmail, BidderPhone, CompanyName, CompanyAddress,
        BidAmount, EmdAmount, ProcessingFee, TotalAmount, Status, PaymentStatus,
        PaymentReference, PaymentDate, SubmittedAt, Remarks
    ) VALUES 
    -- Sample bid for Tender 1
    (1, 'John Smith', 'john.smith@abc-construction.com', '+91-9876543210', 
     'ABC Construction Ltd', '123 Industrial Area, Mumbai, Maharashtra 400001',
     500000.00, 25000.00, 5000.00, 530000.00, 'Submitted', 'Pending',
     'BID-2024-000001', NULL, '2024-01-15 10:30:00', 'Initial bid submission'),
    
    -- Sample bid for Tender 1
    (1, 'Sarah Johnson', 'sarah.johnson@xyz-builders.com', '+91-9876543211',
     'XYZ Builders Pvt Ltd', '456 Business Park, Delhi, Delhi 110001',
     480000.00, 24000.00, 4800.00, 508800.00, 'Under Review', 'Paid',
     'BID-2024-000002', '2024-01-16 14:20:00', '2024-01-16 09:15:00', 'Competitive bid with good pricing'),
    
    -- Sample bid for Tender 2
    (2, 'Michael Brown', 'michael.brown@def-contractors.com', '+91-9876543212',
     'DEF Contractors Inc', '789 Commercial Street, Bangalore, Karnataka 560001',
     750000.00, 37500.00, 7500.00, 795000.00, 'Accepted', 'Paid',
     'BID-2024-000003', '2024-01-17 11:45:00', '2024-01-17 08:30:00', 'Winning bid for infrastructure project'),
    
    -- Sample bid for Tender 2
    (2, 'Emily Davis', 'emily.davis@ghi-enterprises.com', '+91-9876543213',
     'GHI Enterprises', '321 Corporate Plaza, Chennai, Tamil Nadu 600001',
     780000.00, 39000.00, 7800.00, 826800.00, 'Rejected', 'Refunded',
     'BID-2024-000004', '2024-01-18 16:30:00', '2024-01-18 10:45:00', 'Bid rejected due to incomplete documentation'),
    
    -- Sample bid for Tender 3
    (3, 'David Wilson', 'david.wilson@jkl-solutions.com', '+91-9876543214',
     'JKL Solutions Pvt Ltd', '654 Tech Hub, Hyderabad, Telangana 500001',
     320000.00, 16000.00, 3200.00, 339200.00, 'Submitted', 'Pending',
     'BID-2024-000005', NULL, '2024-01-19 13:20:00', 'Software development bid'),
    
    -- Sample bid for Tender 3
    (3, 'Lisa Anderson', 'lisa.anderson@mno-tech.com', '+91-9876543215',
     'MNO Technologies', '987 Innovation Center, Pune, Maharashtra 411001',
     300000.00, 15000.00, 3000.00, 318000.00, 'Under Review', 'Paid',
     'BID-2024-000006', '2024-01-20 09:15:00', '2024-01-20 07:30:00', 'Competitive pricing for tech solution'),
    
    -- Sample bid for Tender 4
    (4, 'Robert Taylor', 'robert.taylor@pqr-services.com', '+91-9876543216',
     'PQR Services Ltd', '147 Service District, Kolkata, West Bengal 700001',
     450000.00, 22500.00, 4500.00, 477000.00, 'Withdrawn', 'Refunded',
     'BID-2024-000007', '2024-01-21 12:00:00', '2024-01-21 11:00:00', 'Bid withdrawn due to change in requirements'),
    
    -- Sample bid for Tender 4
    (4, 'Jennifer Martinez', 'jennifer.martinez@stu-consulting.com', '+91-9876543217',
     'STU Consulting Group', '258 Advisory Avenue, Ahmedabad, Gujarat 380001',
     420000.00, 21000.00, 4200.00, 445200.00, 'Accepted', 'Paid',
     'BID-2024-000008', '2024-01-22 15:45:00', '2024-01-22 14:20:00', 'Consulting services bid accepted'),
    
    -- Sample bid for Tender 5
    (5, 'Christopher Lee', 'christopher.lee@vwx-industries.com', '+91-9876543218',
     'VWX Industries', '369 Manufacturing Zone, Jaipur, Rajasthan 302001',
     600000.00, 30000.00, 6000.00, 636000.00, 'Submitted', 'Failed',
     'BID-2024-000009', NULL, '2024-01-23 16:30:00', 'Payment failed during processing'),
    
    -- Sample bid for Tender 5
    (5, 'Amanda White', 'amanda.white@yza-corp.com', '+91-9876543219',
     'YZA Corporation', '741 Corporate Complex, Lucknow, Uttar Pradesh 226001',
     580000.00, 29000.00, 5800.00, 614800.00, 'Under Review', 'Paid',
     'BID-2024-000010', '2024-01-24 10:30:00', '2024-01-24 09:15:00', 'Manufacturing equipment bid');
END
GO

PRINT 'TenderBids table created and sample data inserted successfully!';
