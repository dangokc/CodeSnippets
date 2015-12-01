USE [SMART_Amaris]
GO
/****** Object:  Table [dbo].[CommarkBudget_BudgetEvent]    Script Date: 30/10/2015 14:13:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommarkBudget_BudgetEvent](
	[BudgetEventId] [int] IDENTITY(1,1) NOT NULL,
	[EventTypeId] [int] NOT NULL,
	[Label] [nvarchar](max) NULL,
 CONSTRAINT [PK_CommarkBudget_BudgetEvent] PRIMARY KEY CLUSTERED 
(
	[BudgetEventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET IDENTITY_INSERT [dbo].[CommarkBudget_BudgetEvent] ON 
SET IDENTITY_INSERT [dbo].[CommarkBudget_BudgetEvent] OFF
ALTER TABLE [dbo].[CommarkBudget_BudgetEvent]  WITH CHECK ADD  CONSTRAINT [FK_CommarkBudget_BudgetEvent_CommarkBudget_BudgetEventType] FOREIGN KEY([EventTypeId])
REFERENCES [dbo].[CommarkBudget_BudgetEventType] ([BudgetEventTypeId])
GO
ALTER TABLE [dbo].[CommarkBudget_BudgetEvent] CHECK CONSTRAINT [FK_CommarkBudget_BudgetEvent_CommarkBudget_BudgetEventType]
GO
