CREATE DATABASE AdventureWorks
ON (FILENAME = 'C:\Users\Vicent\Documents\GitHub\GridViewEX\demo\Demo\App_Data\AdventureWorks2008R2_Data.mdf')
FOR ATTACH_REBUILD_LOG;

CREATE TABLE [AdventureWorks].[dbo].[UserTableViews](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TableName] [nvarchar](50) NOT NULL,
	[UserID] [int] NOT NULL,
	[ViewName] [nvarchar](50) NOT NULL,
	[JSON] [nvarchar](max) NOT NULL,
	[IsDefault] [bit] NOT NULL,
 CONSTRAINT [PK_UserTableView] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]