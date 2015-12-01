--Model edits
ALTER TABLE dbo.GATE_MaterialScreenCode
	DROP CONSTRAINT FK_GATE_MaterialScreenCode_Employee
GO
ALTER TABLE dbo.Employee SET (LOCK_ESCALATION = TABLE)
GO

ALTER TABLE dbo.GATE_MaterialScreenCode
	DROP CONSTRAINT FK_GATE_MaterialScreenCode_Company
GO
ALTER TABLE dbo.Company SET (LOCK_ESCALATION = TABLE)
GO

CREATE TABLE dbo.Gate_StockStatus
	(
	StockStatusId int NOT NULL,
	Label nvarchar(MAX) NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Gate_StockStatus ADD CONSTRAINT
	PK_Gate_StockStatus PRIMARY KEY CLUSTERED 
	(
	StockStatusId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.Gate_StockStatus SET (LOCK_ESCALATION = TABLE)
GO

ALTER TABLE dbo.Gate_Package SET (LOCK_ESCALATION = TABLE)
GO

--ALTER TABLE dbo.GATE_MaterialRequest ADD
--	PacakgeId int NOT NULL
--GO
ALTER TABLE dbo.GATE_MaterialRequest ADD CONSTRAINT
	FK_GATE_MaterialRequest_Gate_Package FOREIGN KEY
	(
	PackageId
	) REFERENCES dbo.Gate_Package
	(
	PackageId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.GATE_MaterialRequest SET (LOCK_ESCALATION = TABLE)
GO

--CREATE TABLE dbo.Gate_RequestOrder
--	(
--	MaterialRequestId int NOT NULL,
--	OrderId int NOT NULL
--	)  ON [PRIMARY]
--GO
--ALTER TABLE dbo.Gate_RequestOrder ADD CONSTRAINT
--	PK_Gate_RequestOrder PRIMARY KEY CLUSTERED 
--	(
--	MaterialRequestId,
--	OrderId
--	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

--GO
--ALTER TABLE dbo.Gate_RequestOrder ADD CONSTRAINT
--	FK_Gate_RequestOrder_GATE_MaterialRequest FOREIGN KEY
--	(
--	MaterialRequestId
--	) REFERENCES dbo.GATE_MaterialRequest
--	(
--	MaterialRequestId
--	) ON UPDATE  NO ACTION 
--	 ON DELETE  NO ACTION 
	
--GO
--ALTER TABLE dbo.Gate_RequestOrder SET (LOCK_ESCALATION = TABLE)
--GO

ALTER TABLE dbo.GATE_MaterialScreenCode
	DROP CONSTRAINT FK_GATE_MaterialScreenCode_GATE_Material
GO