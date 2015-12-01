/*
   08 January 201518:51:06
   User: 
   Server: AMARISDEV04\SQLSERVER2014
   Database: SMART_Amaris
   Application: 
*/

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/

/*
Create table
Object: Save all logs when user request budget validate
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
ALTER TABLE dbo.CommarkBudget_StatusType SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Employee SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.CommarkBudget_Budget SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.CommarkBudget_BudgetHistory
	(
	BudgetHistoryId int NOT NULL IDENTITY (1, 1),
	BudgetId int NOT NULL,
	StatusTypeId int NOT NULL,
	CreatedById int NOT NULL,
	CreatedDate datetime2(7) NULL,
	Comment nvarchar(MAX) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.CommarkBudget_BudgetHistory ADD CONSTRAINT
	PK_CommarkBudget_BudgetHistory PRIMARY KEY CLUSTERED 
	(
	BudgetHistoryId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.CommarkBudget_BudgetHistory ADD CONSTRAINT
	FK_CommarkBudget_BudgetHistory_CommarkBudget_Budget FOREIGN KEY
	(
	BudgetId
	) REFERENCES dbo.CommarkBudget_Budget
	(
	BudgetId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.CommarkBudget_BudgetHistory ADD CONSTRAINT
	FK_CommarkBudget_BudgetHistory_Employee FOREIGN KEY
	(
	CreatedById
	) REFERENCES dbo.Employee
	(
	EmployeeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
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
