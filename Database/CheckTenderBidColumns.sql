-- Check the current column types for TenderBids table
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'TenderBids' 
ORDER BY ORDINAL_POSITION

-- Check if there are any existing records
SELECT COUNT(*) as TotalRecords FROM TenderBids

-- Show sample data to understand current values
SELECT TOP 5 
    Id, 
    Status, 
    PaymentStatus, 
    BidderName,
    CreatedAt
FROM TenderBids
ORDER BY Id DESC
