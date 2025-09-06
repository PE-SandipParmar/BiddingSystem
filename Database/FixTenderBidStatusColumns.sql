-- Fix TenderBid Status and PaymentStatus columns if they are incorrectly defined as int
-- This script will check and fix the column types if needed

-- Check if Status column exists and is of wrong type
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'TenderBids' 
           AND COLUMN_NAME = 'Status' 
           AND DATA_TYPE = 'int')
BEGIN
    PRINT 'Status column is int, converting to nvarchar(20)...'
    
    -- Add a temporary column
    ALTER TABLE TenderBids ADD Status_temp nvarchar(20) NOT NULL DEFAULT 'Submitted'
    
    -- Copy data from int to string (convert int values to string equivalents)
    UPDATE TenderBids SET Status_temp = 
        CASE Status
            WHEN 0 THEN 'Submitted'
            WHEN 1 THEN 'Under Review'
            WHEN 2 THEN 'Accepted'
            WHEN 3 THEN 'Rejected'
            WHEN 4 THEN 'Withdrawn'
            ELSE 'Submitted'
        END
    
    -- Drop the old column
    ALTER TABLE TenderBids DROP COLUMN Status
    
    -- Rename the new column
    EXEC sp_rename 'TenderBids.Status_temp', 'Status', 'COLUMN'
    
    PRINT 'Status column converted successfully'
END
ELSE
BEGIN
    PRINT 'Status column is already correct type (nvarchar)'
END

-- Check if PaymentStatus column exists and is of wrong type
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'TenderBids' 
           AND COLUMN_NAME = 'PaymentStatus' 
           AND DATA_TYPE = 'int')
BEGIN
    PRINT 'PaymentStatus column is int, converting to nvarchar(20)...'
    
    -- Add a temporary column
    ALTER TABLE TenderBids ADD PaymentStatus_temp nvarchar(20) NOT NULL DEFAULT 'Pending'
    
    -- Copy data from int to string (convert int values to string equivalents)
    UPDATE TenderBids SET PaymentStatus_temp = 
        CASE PaymentStatus
            WHEN 0 THEN 'Pending'
            WHEN 1 THEN 'Paid'
            WHEN 2 THEN 'Failed'
            WHEN 3 THEN 'Refunded'
            ELSE 'Pending'
        END
    
    -- Drop the old column
    ALTER TABLE TenderBids DROP COLUMN PaymentStatus
    
    -- Rename the new column
    EXEC sp_rename 'TenderBids.PaymentStatus_temp', 'PaymentStatus', 'COLUMN'
    
    PRINT 'PaymentStatus column converted successfully'
END
ELSE
BEGIN
    PRINT 'PaymentStatus column is already correct type (nvarchar)'
END

-- Verify the column types
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'TenderBids' 
AND COLUMN_NAME IN ('Status', 'PaymentStatus')

PRINT 'TenderBid status columns fix completed!'
