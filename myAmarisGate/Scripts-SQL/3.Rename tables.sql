EXECUTE sp_rename N'dbo.GATE_Material', N'Gate_GenericMaterial', 'OBJECT' 
GO
EXECUTE sp_rename N'dbo.Gate_GenericMaterial.MaterialId', N'Tmp_GenericMaterialId', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.Gate_GenericMaterial.Tmp_GenericMaterialId', N'GenericMaterialId', 'COLUMN' 
GO
ALTER TABLE dbo.Gate_GenericMaterial SET (LOCK_ESCALATION = TABLE)
GO