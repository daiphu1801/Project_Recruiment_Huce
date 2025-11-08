-- Add foreign key constraint for Recruiters.PhotoID -> ProfilePhotos.PhotoID
-- Run this script if the foreign key doesn't exist yet

USE JOBPORTAL_EN;
GO

-- Check if PhotoID column exists in Recruiters table, if not add it
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Recruiters') AND name = 'PhotoID')
BEGIN
    ALTER TABLE dbo.Recruiters
    ADD PhotoID INT NULL;
    PRINT 'Added PhotoID column to Recruiters table';
END
ELSE
BEGIN
    PRINT 'PhotoID column already exists in Recruiters table';
END
GO

-- Add foreign key constraint if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Recruiters_ProfilePhotos')
BEGIN
    ALTER TABLE dbo.Recruiters
    ADD CONSTRAINT FK_Recruiters_ProfilePhotos
    FOREIGN KEY (PhotoID) REFERENCES dbo.ProfilePhotos(PhotoID)
    ON DELETE SET NULL;
    PRINT 'Added FK_Recruiters_ProfilePhotos foreign key constraint';
END
ELSE
BEGIN
    PRINT 'FK_Recruiters_ProfilePhotos foreign key constraint already exists';
END
GO

