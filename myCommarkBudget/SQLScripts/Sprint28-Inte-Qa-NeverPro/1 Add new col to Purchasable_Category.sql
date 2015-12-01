-- Move all items which have categoryId = 19 to another category
update Purchasable_Subcategory 
set CategoryId = 3
where categoryid = 19

-- Delete categories 19
delete from Purchasable_Category
where CategoryId = 19

-- Turn on indentity_insert mode
SET IDENTITY_INSERT Purchasable_Category ON

-- Insert category 19 with label = 'Uncategorized'
insert into Purchasable_Category(CategoryId, Label, DomainId)
values(19, 'Uncategorized', 5)

-- Turn off indentity_insert mode
SET IDENTITY_INSERT Purchasable_Category OFF