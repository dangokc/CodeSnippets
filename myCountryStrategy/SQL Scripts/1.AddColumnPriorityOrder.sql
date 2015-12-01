USE SMART_Amaris
IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'OpeningCountry_Priority'))
BEGIN

-- ADDING NEW COLUMN
ALTER TABLE dbo.OpeningCountry_Priority
ADD PriorityOrder INTEGER NULL

-- INSERT INTO
UPDATE dbo.OpeningCountry_Priority 
SET PriorityOrder = PriorityId

ALTER TABLE dbo.OpeningCountry_Priority ALTER COLUMN PriorityOrder INTEGER NOT NULL

END