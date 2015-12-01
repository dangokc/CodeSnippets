USE [SMART_Amaris]
GO
/****** Object:  UserDefinedFunction [dbo].[CommarkBudget_HasDataForBudget]    Script Date: 15/12/2014 09:36:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	Check if a budget has data or not	
-- =============================================
create FUNCTION [dbo].[CommarkBudget_HasDataForBudget](@budgetId int)
RETURNS int
AS
BEGIN
	
	declare @count int;
	
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
