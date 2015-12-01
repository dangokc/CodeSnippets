ALTER TABLE dbo.GATE_ComponentRequest ADD
	StockHistoryId int
GO

ALTER TABLE [dbo].[GATE_ComponentRequest]  WITH CHECK ADD  CONSTRAINT [FK_GATE_ComponentRequest_StockHistory] FOREIGN KEY([StockHistoryId])
REFERENCES [dbo].[Gate_StockHistory] ([StockHistoryId])
GO