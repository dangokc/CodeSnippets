USE [SMART_Amaris]
GO
/****** Object:  StoredProcedure [dbo].[CommarkBudget_CreateNewYearBudget]    Script Date: 15/12/2014 09:31:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		TDU003
-- Create date: <Create Date,,>
-- Description:	Duplicate all budgets from @YearSource to @YearTarget
--				If some budgets are already available, check they have data or not
--					If no data: Delete them and make duplicated version
--					If yes: Skip
-- =============================================
create PROCEDURE [dbo].[CommarkBudget_CreateNewYearBudget]
	@YearSource nvarchar(5),
	@YearTarget nvarchar(5)
AS
BEGIN
	
	declare @count int;


	
	declare @entityId int;
	declare billingEntities cursor local for select BillingEntityId from dbo.BillingEntity;
	open billingEntities;
	
	fetch next from billingEntities into @entityId
	-- Loop BILLINGENTITY table
	while @@FETCH_STATUS = 0 -- Example: The first billing entity is AMAUT
	begin		
		set @count = CONVERT(
			int, 
			(	select count(*) 
				from dbo.CommarkBudget_Budget b 
				where b.BillingEntityId = @entityId and b.Year = @YearTarget
			)
		);

		-- If no budget AMAUT 2015
		if (@count = 0) 
		begin
			-- create AMAUT 2015
			exec dbo.CommarkBudget_CreateNewYearBudgetForAnEntity
			@YearSource, @YearTarget, @entityId;
		end

		-- If AMAUT 2015 have one or more budgets
		else
		begin
			declare @budgetId int;
			declare budgetIds cursor local for select b.BudgetId
			from dbo.CommarkBudget_Budget b 
			where b.BillingEntityId = @entityId and b.Year = @YearTarget
			
			open budgetIds 
			
			-- loop BUDGET table
			fetch next from budgetIds into @budgetId
			while @@FETCH_STATUS = 0
			begin
				declare @hasData int;
				set @hasData = dbo.CommarkBudget_HasDataForBudget(@budgetId);

				if (@hasData = 0) -- Example: No data for AMAUT 2015
				begin
					-- Delete AMAUT 2015
					delete from dbo.CommarkBudget_Budget
					where BillingEntityId = @entityId and Year = @YearTarget;

					-- Create AMAUT 2015 which is copyed from AMAUT 2014
					exec dbo.CommarkBudget_CreateNewYearBudgetForAnEntity
					@YearSource, @YearTarget, @entityId;
				end

				-- If @hasDate = 1, it means AMAUT2015 is already available
				-- -> DO NOT CREATE

				-- Go to next element
				fetch next from budgetIds into @budgetId;
			end
			close budgetIds;
			deallocate budgetIds;
		end
		fetch next from billingEntities into @entityId;

	end



	close billingEntities;
	deallocate billingEntities;
END