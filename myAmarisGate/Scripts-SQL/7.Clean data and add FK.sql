--Clean bad data to be able to create FK constraint...
UPDATE dbo.GATE_MaterialRequest
SET
    dbo.GATE_MaterialRequest.AmarisOfficeId = NULL 
WHERE dbo.GATE_MaterialRequest.AmarisOfficeId = 0
GO
UPDATE dbo.GATE_MaterialRequest 
SET
	dbo.GATE_MaterialRequest.AmarisOfficeId = o.OfficeId
FROM dbo.GATE_MaterialRequest gmr
INNER JOIN dbo.OfficeExtension oe ON oe.OfficeExtensionId = gmr.AmarisOfficeId
INNER JOIN dbo.Office o ON o.OfficeId = oe.OfficeId
WHERE gmr.MaterialRequestId IN (
SELECT gmr.MaterialRequestId FROM dbo.GATE_MaterialRequest gmr
WHERE gmr.AmarisOfficeId NOT IN (SELECT oe.OfficeId FROM dbo.Office oe))
GO

ALTER TABLE [dbo].[GATE_MaterialRequest]  WITH CHECK ADD  CONSTRAINT [FK_GATE_MaterialRequest_Office] FOREIGN KEY([AmarisOfficeId])
REFERENCES [dbo].[Office] ([OfficeId])
GO

ALTER TABLE [dbo].[GATE_MaterialRequest] CHECK CONSTRAINT [FK_GATE_MaterialRequest_Office]
GO
