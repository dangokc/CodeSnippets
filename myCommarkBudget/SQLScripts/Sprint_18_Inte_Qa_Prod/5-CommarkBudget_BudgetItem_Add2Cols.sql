/*
   11 December 201410:25:26
   User: 
   Server: AMARISDEV04\SQLSERVER2014
   Database: SMART_Amaris
   Application: 
*/

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/

-- Objective:
-- Add two cols into CommarkBudget_BudgetItem table
-- updatedBy
-- updatedDate

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
ALTER TABLE dbo.Employee SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.CommarkBudget_BudgetItem ADD
	UpdatedDate datetime2(7) NULL,
	UpdatedById int NULL
GO
ALTER TABLE dbo.CommarkBudget_BudgetItem ADD CONSTRAINT
	FK_CommarkBudget_BudgetItem_Employee FOREIGN KEY
	(
	UpdatedById
	) REFERENCES dbo.Employee
	(
	EmployeeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.CommarkBudget_BudgetItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
