DROP TABLE dbo.Gate_Screen
GO
DROP TABLE dbo.GATE_MaterialRequest_GATE_OutlookAppointment
GO
DROP TABLE dbo.GATE_OutlookAppointment
GO
DROP TABLE dbo.GATE_MaterialStatus	
GO
DROP TABLE	dbo.GATE_MaterialRequest__GATE_Material
GO
DROP TABLE	dbo.GATE_MaterialRequest_GATE_Material
GO 

--drop constraint
IF EXISTS(SELECT * FROM sys.all_objects where name = 'FK_GATE_MaterialRequest_GATE_MaterialRequest')
	EXEC('ALTER TABLE GATE_MaterialRequest DROP CONSTRAINT FK_GATE_MaterialRequest_GATE_MaterialRequest')
GO