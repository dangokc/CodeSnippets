/*
   02 April 201514:48:51
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
ALTER TABLE dbo.CommarkBudget_Budget ADD
	StartDate datetime2(7) NULL
GO
ALTER TABLE dbo.CommarkBudget_Budget SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
