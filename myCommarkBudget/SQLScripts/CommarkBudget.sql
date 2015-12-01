USE [SMART_Amaris]

-- CREATE FUNCTION
create FUNCTION dbo.CommarkBudget_HasDataForBudget(@budgetId int) 
RETURNS int
AS
BEGIN	
	declare @count int;	
	
	-- Convert
	set @count = CONVERT(int, (select count(*) from dbo.CommarkBudget_BudgetItem bi
		where bi.BudgetId = @budgetId
	));
	if(@count > 0)
	begin
		return 1;
	end

	set @count = CONVERT(int, (select count(*) from dbo.CommarkBudget_Provisions pro
		where pro.BudgetId = @budgetId
	));
	if(@count > 0)
	begin
		return 1;
	end

	set @count = CONVERT(int, (select count(*) from dbo.CommarkBudget_MonthlyValueAfterCOB mon
		where mon.BudgetId = @budgetId
	));
	if(@count > 0)
	begin
		return 1;
	end

	return 0;

END

-- CREATE TRANSACTION
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
ALTER TABLE dbo.CommarkBudget_Budget ADD
	UpdatedDate datetime2(7) NULL,
	UpdatedById int NULL
GO
ALTER TABLE dbo.CommarkBudget_Budget ADD CONSTRAINT
	FK_CommarkBudget_Budget_Employee FOREIGN KEY
	(
	UpdatedById
	) REFERENCES dbo.Employee
	(
	EmployeeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.CommarkBudget_Budget SET (LOCK_ESCALATION = TABLE)
GO
COMMIT


-- CREATE STOREPROCEDURE
create PROCEDURE [dbo].[CommarkBudget_CreateNewYearBudgetForAnEntity]
	@YearSource nvarchar(5),
	@YearTarget nvarchar(5),
	@EntityId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Budget list for billingEntityId = id, year = @YearSource
	declare @BudgetSource table(
		BudgetId int,
		DomainId int, 
		Year nvarchar(5), 
		RoleReadId int, 
		RoleContributeId int, 
		BillingEntityId int
	);

	-- Budget list for billingEntityId = id, year = @YearTarget
	declare @BudgetTarget table(
		BudgetId int,
		DomainId int, 
		Year nvarchar(5), 
		RoleReadId int, 
		RoleContributeId int, 
		BillingEntityId int
	);

	-- COPY 1: Budget 2014 to budget 2015
	insert into dbo.CommarkBudget_Budget (DomainId, Year, RoleReadId, RoleContributeId, BillingEntityId)
	select DomainId, @YearTarget, RoleReadId, RoleContributeId, BillingEntityId
	from dbo.CommarkBudget_Budget
	where Year = @YearSource and BillingEntityId = @EntityId
	order by BudgetId

	-- Insert all budgets in source: @YearSource, EntityId
	insert into @BudgetSource
	select BudgetId ,DomainId, Year, RoleReadId, RoleContributeId, BillingEntityId
	from dbo.CommarkBudget_Budget
	where Year = @YearSource and BillingEntityId = @EntityId
	order by BudgetId;

	-- Insert all budgets in target: @YearTarget, EntityId
	insert into @BudgetTarget
	select BudgetId ,DomainId, Year, RoleReadId, RoleContributeId, BillingEntityId
	from dbo.CommarkBudget_Budget
	where Year = @YearTarget and BillingEntityId = @EntityId
	order by BudgetId;

	-- the number of elements of BudgetSource and BudgetTarget are the same
	while(select count(*) from @BudgetSource) > 0
	begin
		-- Get pair (Old - New)
		declare @IdSource int;
		declare @IdTarget int;
		set @IdSource = CONVERT(int, (select top 1 BudgetId from @BudgetSource));
		set @IdTarget = CONVERT(int, (select top 1 BudgetId from @BudgetTarget));
		


		-- COPY 2: All BudgetItems correspond with each budgets
		insert into dbo.CommarkBudget_BudgetItem (BudgetId, QuantityLow, QuantityAdditional, QuantityOrdered, Comment, Price, ItemId, DefaultPrice, RoleReadId, RoleContributeId)
		select @IdTarget, 0, 0, 0, Comment, Price, ItemId, DefaultPrice, RoleReadId, RoleContributeId
		from dbo.CommarkBudget_BudgetItem
		where BudgetId = @IdSource;

		-- COPY 3: 12 months provision for each budgets
		begin
			insert into dbo.CommarkBudget_Provisions (BudgetId, Month, Value, COB)
			(
				select @IdTarget, Month, Value, 0
				from dbo.CommarkBudget_Provisions
				where (BudgetId = @IdSource)
			);
		end

		-- COPY 4: Create 12 monthly value for each budgets
		begin
			insert into dbo.CommarkBudget_MonthlyValueAfterCOB (BudgetId, Month, Value)
			(	
				select @IdTarget, Month, Value
				from dbo.CommarkBudget_MonthlyValueAfterCOB
				where (BudgetId = @IdSource)
			)
				
		end

				-- Remove pair
		delete top(1) from @BudgetSource;
		delete top(1) from @BudgetTarget;
	end

-- INSERT
USE [SMART_Amaris]
GO
INSERT [dbo].[CommarkBudget_StatusType] ([StatusTypeId], [Label]) VALUES (1, N'Budget item is DELETING')
INSERT [dbo].[CommarkBudget_StatusType] ([StatusTypeId], [Label]) VALUES (2, N'Budget item is REFUSED by Commark')
INSERT [dbo].[CommarkBudget_StatusType] ([StatusTypeId], [Label]) VALUES (3, N'Budget item is APPROVED by Commark')

-- ALATER TABLE
ALTER TABLE dbo.CommarkBudget_Budget ADD
	IsAvailable bit NOT NULL CONSTRAINT DF_CommarkBudget_Budget_IsAvailable DEFAULT 1
GO

alter table CommarkBudget_Budget alter column billingentityid integer not null

alter table CommarkBudget_Budget alter column domainid integer not null

-- ADDED COLUMNS
-- CHANGE DATA TYPE
ALTER TABLE dbo.CommarkBudget_Budget ADD
	StartDate datetime2(7) NULL
GO

-- Script to invert the domain of the company 31 domainId 5 <==> 17
	declare @CM table(budgetId int);
	insert into @CM
	select budgetId from CommarkBudget_Budget
	where BillingEntityId = 31 and domainid = 5;



	declare @Other table(budgetId int);
	insert into @Other
	select budgetId from CommarkBudget_Budget
	where BillingEntityId = 31 and domainid = 17;

	
	update CommarkBudget_Budget
	set domainid = 17
	where BudgetId in (select budgetId from @CM);

	
	update CommarkBudget_Budget
	set domainid = 5
	where BudgetId in (select budgetId from @Other);
