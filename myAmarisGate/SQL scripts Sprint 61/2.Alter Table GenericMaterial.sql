USE SMART_Amaris
IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'Gate_GenericMaterial'))
BEGIN

-- ADDING NEW COLUMN
ALTER TABLE dbo.Gate_GenericMaterial
ADD CategoryId int REFERENCES dbo.GATE_MaterialCategory(CategoryId)