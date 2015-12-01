CREATE TABLE [dbo].[Gate_ProductPassword](
	[ProductPasswordId] [int] NOT NULL IDENTITY	(1,1),
	[StockId] [int] NULL,
	[Password] [nvarchar](max) NULL,
 CONSTRAINT [PK_Gate_ProductPassword] PRIMARY KEY CLUSTERED 
(
	[ProductPasswordId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [dbo].[Gate_ProductPassword]  WITH CHECK ADD  CONSTRAINT [FK_Gate_ProductPassword_Gate_Stock] FOREIGN KEY([StockId])
REFERENCES [dbo].[Gate_Stock] ([StockId])
GO

ALTER TABLE [dbo].[Gate_ProductPassword] CHECK CONSTRAINT [FK_Gate_ProductPassword_Gate_Stock]
GO

CREATE TABLE [dbo].[Gate_StockHistory](
	[StockHistoryId] [int] IDENTITY(1,1) NOT NULL,
	[StockId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[StartDate] [datetime2](7) NOT NULL,
	[EndDate] [datetime2](7) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[CreatedById] [int] NOT NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[UpdatedById] [int] NULL,
 CONSTRAINT [PK_Gate_StockHistory] PRIMARY KEY CLUSTERED 
(
	[StockHistoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Gate_StockHistory]  WITH CHECK ADD  CONSTRAINT [FK_Gate_StockHistory_Employee] FOREIGN KEY([UpdatedById])
REFERENCES [dbo].[Employee] ([EmployeeId])
GO

ALTER TABLE [dbo].[Gate_StockHistory] CHECK CONSTRAINT [FK_Gate_StockHistory_Employee]
GO

ALTER TABLE [dbo].[Gate_StockHistory]  WITH CHECK ADD  CONSTRAINT [FK_Gate_StockHistory_Employee1] FOREIGN KEY([CreatedById])
REFERENCES [dbo].[Employee] ([EmployeeId])
GO

ALTER TABLE [dbo].[Gate_StockHistory] CHECK CONSTRAINT [FK_Gate_StockHistory_Employee1]
GO

ALTER TABLE [dbo].[Gate_StockHistory]  WITH CHECK ADD  CONSTRAINT [FK_Gate_StockHistory_Employee2] FOREIGN KEY([UserId])
REFERENCES [dbo].[Employee] ([EmployeeId])
GO

ALTER TABLE [dbo].[Gate_StockHistory] CHECK CONSTRAINT [FK_Gate_StockHistory_Employee2]
GO

ALTER TABLE [dbo].[Gate_StockHistory]  WITH CHECK ADD  CONSTRAINT [FK_Gate_StockHistory_Gate_Stock] FOREIGN KEY([StockId])
REFERENCES [dbo].[Gate_Stock] ([StockId])
GO

ALTER TABLE [dbo].[Gate_StockHistory] CHECK CONSTRAINT [FK_Gate_StockHistory_Gate_Stock]
GO

--New FKs
ALTER TABLE [dbo].[GATE_ComponentRequest]  WITH CHECK ADD  CONSTRAINT [FK_GATE_ComponentRequest_GATE_MaterialRequest] FOREIGN KEY([MaterialRequestId])
REFERENCES [dbo].[GATE_MaterialRequest] ([MaterialRequestId])
GO

ALTER TABLE [dbo].[GATE_ComponentRequest] CHECK CONSTRAINT [FK_GATE_ComponentRequest_GATE_MaterialRequest]
GO



CREATE TABLE dbo.Gate_RequestStock
	(
	MaterialRequestId int NOT NULL,
	StockId int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Gate_RequestStock ADD CONSTRAINT
	PK_Gate_RequestStock PRIMARY KEY CLUSTERED 
	(
	MaterialRequestId,
	StockId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.Gate_RequestStock ADD CONSTRAINT
	FK_Gate_RequestStock_GATE_MaterialRequest FOREIGN KEY
	(
	MaterialRequestId
	) REFERENCES dbo.GATE_MaterialRequest
	(
	MaterialRequestId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Gate_RequestStock ADD CONSTRAINT
	FK_Gate_RequestStock_Gate_Stock FOREIGN KEY
	(
	StockId
	) REFERENCES dbo.Gate_Stock
	(
	StockId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Gate_RequestStock SET (LOCK_ESCALATION = TABLE)
GO

ALTER TABLE dbo.GATE_ComponentRequest SET (LOCK_ESCALATION = TABLE)
GO