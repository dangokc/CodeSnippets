update Purchasable_Item 
set SubCategoryId = 7
where SubCategoryId = 84


delete from Purchasable_Subcategory
where SubcategoryId = 84


SET IDENTITY_INSERT Purchasable_Subcategory ON

insert into Purchasable_Subcategory(subCategoryId, Label, CategoryId)
values(84, 'Uncategorized', 19)


SET IDENTITY_INSERT Purchasable_Subcategory OFF