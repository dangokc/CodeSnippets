USE SMART_Amaris
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'CorpDevPriority_Update')
BEGIN	
	DROP PROCEDURE CorpDevPriority_Update	
END
GO

USE SMART_Amaris
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'CorpDevPriority_InsertInto')
BEGIN	
	DROP PROCEDURE CorpDevPriority_InsertInto	
END
GO

USE SMART_Amaris
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'AfmPriority_Update')
BEGIN	
	DROP PROCEDURE AfmPriority_Update	
END
GO

USE SMART_Amaris
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'AfmPriority_InsertInto')
BEGIN	
	DROP PROCEDURE AfmPriority_InsertInto	
END
GO

USE SMART_Amaris
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'DirectorPriority_Update')
BEGIN	
	DROP PROCEDURE DirectorPriority_Update	
END
GO

USE SMART_Amaris
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'DirectorPriority_InsertInto')
BEGIN	
	DROP PROCEDURE DirectorPriority_InsertInto	
END
GO


