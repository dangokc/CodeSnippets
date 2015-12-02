USE SMART_Amaris
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'OpeningCountryPriority_UpdatePriorityOrder')
BEGIN	
	DROP PROCEDURE OpeningCountryPriority_UpdatePriorityOrder	
END
-- Start Procedure
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE OpeningCountryPriority_UpdatePriorityOrder 
	@PriorityId Integer,
	@newPriority Integer
AS
BEGIN
	SET NOCOUNT ON;
	update [dbo].[OpeningCountry_Priority] set [dbo].[OpeningCountry_Priority].PriorityOrder = @newPriority 
		WHERE [dbo].[OpeningCountry_Priority].PriorityId =  @PriorityId
	update [dbo].[OpeningCountry_Priority] set [dbo].[OpeningCountry_Priority].PriorityOrder = 
							[dbo].[OpeningCountry_Priority].PriorityOrder + 1 
		WHERE [dbo].[OpeningCountry_Priority].PriorityOrder >= @newPriority 
		AND [dbo].[OpeningCountry_Priority].PriorityId <> @PriorityId		
END
GO
-- End Procedure






