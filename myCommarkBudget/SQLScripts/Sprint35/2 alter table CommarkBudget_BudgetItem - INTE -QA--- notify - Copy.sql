-- change datatype of QuantityOrder from int to float

ALTER TABLE CommarkBudget_BudgetItem DROP CONSTRAINT DF_CommarkBudget_BudgetItem_QuantityOrdered;

alter table CommarkBudget_BudgetItem alter column QuantityOrdered float not null

ALTER TABLE [dbo].[CommarkBudget_BudgetItem] ADD  CONSTRAINT [DF_CommarkBudget_BudgetItem_QuantityOrdered]  DEFAULT ((0)) FOR [QuantityOrdered]


