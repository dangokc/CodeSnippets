USE [SMART_Amaris]
GO
/****** Object:  Table [dbo].[CommarkBudget_BudgetEventType]    Script Date: 30/10/2015 14:13:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommarkBudget_BudgetEventType](
	[BudgetEventTypeId] [int] NOT NULL,
	[Label] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_CommarkBudget_BudgetEventType] PRIMARY KEY CLUSTERED 
(
	[BudgetEventTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
INSERT [dbo].[CommarkBudget_BudgetEventType] ([BudgetEventTypeId], [Label]) VALUES (1, N'Director')
INSERT [dbo].[CommarkBudget_BudgetEventType] ([BudgetEventTypeId], [Label]) VALUES (2, N'Country Manager')
