/*
   12 June 201514:18:33
   User: 
   Server: AMARISDEV04\SQLSERVER2014
   Database: SMART_Amaris
   Application: 
*/

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
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
ALTER TABLE dbo.BillingEntity SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.CommarkBudget_BudgetMonitoringParameter
	(
	BillingEntityId int NOT NULL,
	Year int NOT NULL,
	Turnover float NOT NULL,
	AveragePricePerConsultant float NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.CommarkBudget_BudgetMonitoringParameter ADD CONSTRAINT
	DF_CommarkBudget_BudgetMonitoringParameter_Turnover DEFAULT 0 FOR Turnover
GO
ALTER TABLE dbo.CommarkBudget_BudgetMonitoringParameter ADD CONSTRAINT
	DF_CommarkBudget_BudgetMonitoringParameter_AveragePricePerConsultant DEFAULT 0 FOR AveragePricePerConsultant
GO
ALTER TABLE dbo.CommarkBudget_BudgetMonitoringParameter ADD CONSTRAINT
	PK_CommarkBudget_BudgetMonitoringParameter PRIMARY KEY CLUSTERED 
	(
	BillingEntityId,
	Year
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.CommarkBudget_BudgetMonitoringParameter ADD CONSTRAINT
	FK_CommarkBudget_BudgetMonitoringParameter_BillingEntity FOREIGN KEY
	(
	BillingEntityId
	) REFERENCES dbo.BillingEntity
	(
	BillingEntityId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.CommarkBudget_BudgetMonitoringParameter SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
