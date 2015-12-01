USE [SMART_Amaris]
GO
/****** Object:  StoredProcedure [dbo].[CommarkBudget_CreateNewYearBudgetForAnEntity]    Script Date: 15/12/2014 09:32:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	Duplicate budget information from @YearSource to @YearTarget
--				billing entity which is duplicated has id  = @EntityId
-- =============================================
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
END
