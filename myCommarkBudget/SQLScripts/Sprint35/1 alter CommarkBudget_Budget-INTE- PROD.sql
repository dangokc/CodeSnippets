-- set billing entity id and domain id and year to NOT NULL

alter table CommarkBudget_Budget alter column billingentityid integer not null

alter table CommarkBudget_Budget alter column domainid integer not null

