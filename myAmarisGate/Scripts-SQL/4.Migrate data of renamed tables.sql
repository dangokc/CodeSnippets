CREATE TABLE dbo.Tmp_Gate_Stock
	(
	StockId int NOT NULL IDENTITY (1, 1),
	ProductCode nvarchar(MAX) NOT NULL,
	CompanyId int NOT NULL,
	GenericMaterialId int NOT NULL,
	--MaterialRequestId int NOT NULL,
	SeqNumber int NOT NULL,
	Note nvarchar(MAX) NULL,
	StockStatusId int NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_Gate_Stock SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_Gate_Stock ON
GO
IF EXISTS(SELECT * FROM dbo.GATE_MaterialScreenCode)
	 EXEC('INSERT INTO dbo.Tmp_Gate_Stock (StockId, ProductCode, CompanyId, GenericMaterialId, SeqNumber, Note)
		SELECT MaterialScreenCodeId, MaterialScreenCode, CompanyId, MaterialId, SeqNumber, Note FROM dbo.GATE_MaterialScreenCode WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_Gate_Stock OFF
GO
DROP TABLE dbo.GATE_MaterialScreenCode
GO
EXECUTE sp_rename N'dbo.Tmp_Gate_Stock', N'Gate_Stock', 'OBJECT' 
GO
ALTER TABLE dbo.Gate_Stock ADD CONSTRAINT
	PK_GATE_MaterialScreenCode PRIMARY KEY CLUSTERED 
	(
	StockId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.Gate_Stock ADD CONSTRAINT
	FK_GATE_MaterialScreenCode_Company FOREIGN KEY
	(
	CompanyId
	) REFERENCES dbo.Company
	(
	ID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Gate_Stock ADD CONSTRAINT
	FK_GATE_MaterialScreenCode_GATE_Material FOREIGN KEY
	(
	GenericMaterialId
	) REFERENCES dbo.Gate_GenericMaterial
	(
	GenericMaterialId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Gate_Stock ADD CONSTRAINT
	FK_Gate_Stock_Gate_StockStatus FOREIGN KEY
	(
	StockStatusId
	) REFERENCES dbo.Gate_StockStatus
	(
	StockStatusId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO

ALTER TABLE Gate_Stock 
ADD CONSTRAINT AK_StockCode UNIQUE (GenericMaterialId, SeqNumber, CompanyId)
GO



--ALTER TABLE dbo.Gate_Stock ADD CONSTRAINT
--	FK_Gate_Stock_Gate_MaterialRequest FOREIGN KEY
--	(
--	MaterialRequestId
--	) REFERENCES dbo.GATE_MaterialRequest
--	(
--	MaterialRequestId
--	) ON UPDATE  NO ACTION 
--	 ON DELETE  NO ACTION 
	
--GO