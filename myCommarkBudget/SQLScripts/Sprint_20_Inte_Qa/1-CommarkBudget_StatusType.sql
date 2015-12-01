/*
   08 January 201518:23:03
   User: 
   Server: AMARISDEV04\SQLSERVER2014
   Database: SMART_Amaris_Temp
   Application: 
*/

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/

/*
Create statusType table
Objective: Store all status for all budgetitem , budget, item
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
CREATE TABLE dbo.CommarkBudget_StatusType
	(
	StatusTypeId int NOT NULL,
	Label nvarchar(100) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.CommarkBudget_StatusType ADD CONSTRAINT
	PK_CommarkBudget_StatusType PRIMARY KEY CLUSTERED 
	(
	StatusTypeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.CommarkBudget_StatusType SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
