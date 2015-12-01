USE SMART_Amaris
IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'GATE_MaterialCategory'))
BEGIN
	DROP TABLE dbo.GATE_MaterialCategory
END
ELSE
BEGIN
CREATE TABLE dbo.GATE_MaterialCategory
(	
	CategoryId int IDENTITY(1,1) PRIMARY KEY ,	
	CategoryLabel nvarchar(50) UNIQUE
)
END