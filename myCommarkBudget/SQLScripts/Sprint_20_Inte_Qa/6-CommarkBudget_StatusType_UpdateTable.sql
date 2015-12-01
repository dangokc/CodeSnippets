/*
   23 January 201515:22:25
   User: 
   Server: AMARISDEV04\SQLSERVER2014
   Database: SMART_Amaris
   Application: 
*/

/*
Change column: Label from allow null to NOT allow null
*/


BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_CommarkBudget_StatusType
	(
	StatusTypeId int NOT NULL,
	Label nvarchar(100) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_CommarkBudget_StatusType SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM dbo.CommarkBudget_StatusType)
	 EXEC('INSERT INTO dbo.Tmp_CommarkBudget_StatusType (StatusTypeId, Label)
		SELECT StatusTypeId, Label FROM dbo.CommarkBudget_StatusType WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE dbo.CommarkBudget_BudgetHistory
	DROP CONSTRAINT FK_CommarkBudget_BudgetHistory_CommarkBudget_StatusType
GO
ALTER TABLE dbo.CommarkBudget_BudgetItemHistory
	DROP CONSTRAINT FK_CommarkBudget_BudgetItemHistory_CommarkBudget_StatusType
GO
ALTER TABLE dbo.CommarkBudget_ItemHistory
	DROP CONSTRAINT FK_CommarkBudget_ItemHistory_CommarkBudget_StatusType
GO
DROP TABLE dbo.CommarkBudget_StatusType
GO
EXECUTE sp_rename N'dbo.Tmp_CommarkBudget_StatusType', N'CommarkBudget_StatusType', 'OBJECT' 
GO
ALTER TABLE dbo.CommarkBudget_StatusType ADD CONSTRAINT
	PK_CommarkBudget_StatusType PRIMARY KEY CLUSTERED 
	(
	StatusTypeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.CommarkBudget_ItemHistory ADD CONSTRAINT
	FK_CommarkBudget_ItemHistory_CommarkBudget_StatusType FOREIGN KEY
	(
	StatusTypeId
	) REFERENCES dbo.CommarkBudget_StatusType
	(
	StatusTypeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.CommarkBudget_ItemHistory SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.CommarkBudget_BudgetItemHistory ADD CONSTRAINT
	FK_CommarkBudget_BudgetItemHistory_CommarkBudget_StatusType FOREIGN KEY
	(
	StatusTypeId
	) REFERENCES dbo.CommarkBudget_StatusType
	(
	StatusTypeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.CommarkBudget_BudgetItemHistory SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.CommarkBudget_BudgetHistory ADD CONSTRAINT
	FK_CommarkBudget_BudgetHistory_CommarkBudget_StatusType FOREIGN KEY
	(
	StatusTypeId
	) REFERENCES dbo.CommarkBudget_StatusType
	(
	StatusTypeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.CommarkBudget_BudgetHistory SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
