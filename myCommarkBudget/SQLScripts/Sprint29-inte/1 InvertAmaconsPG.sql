--Script to invert the domain of the company 31 domainId 5 <==> 17


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