CREATE TABLE [dbo].[Attachment](	[Id] [bigint] IDENTITY(1,1) NOT NULL,	[Title] [nvarchar](50) NOT NULL,	[FileName] [nvarchar](128) NOT NULL,	[Active] [bit] NOT NULL,	[DateCreated] [datetime2](7) NOT NULL,	[DateModified] [datetime2](7) NOT NULL,	[User_Creator_Id] [bigint] NOT NULL,	[User_Modifier_Id] [bigint] NOT NULL,	[RecordingId] [uniqueidentifier] NOT NULL, CONSTRAINT [PK_Attachment2] PRIMARY KEY CLUSTERED (	[Id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]

CREATE TABLE [dbo].[Recording](	[Id] [uniqueidentifier] NOT NULL,	[Duration] [time](7) NOT NULL,	[Title] [nvarchar](50) NOT NULL,	[FileName] [nvarchar](128) NOT NULL,	[Status] [nvarchar](50) NOT NULL,	[Active] [bit] NOT NULL,	[DateCreated] [datetime2](7) NOT NULL,	[DateModified] [datetime2](7) NOT NULL,	[User_Creator_Id] [bigint] NOT NULL,	[User_Modifier_Id] [bigint] NOT NULL,	[User_Assigned_To] [bigint] NULL, CONSTRAINT [PK_Recording2] PRIMARY KEY CLUSTERED (	[Id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]

CREATE TABLE [dbo].[Secretary_Employee](	[Id] [bigint] IDENTITY(1,1) NOT NULL,	[EmployeeId] [bigint] NOT NULL,	[SecretaryId] [bigint] NOT NULL, CONSTRAINT [PK_Secretary_Employee2] PRIMARY KEY CLUSTERED (	[Id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]

CREATE TABLE [dbo].[User](	[Id] [bigint] IDENTITY(1,1) NOT NULL,	[UserName] [nvarchar](50) NOT NULL,	[EmailAddress] [nvarchar](50) NOT NULL,	[FullName] [nvarchar](50) NOT NULL,	[Title] [nvarchar](50) NOT NULL,	[Active] [bit] NULL, CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED (	[Id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]

CREATE TABLE [dbo].[User_UserClaim](	[Id] [bigint] IDENTITY(1,1) NOT NULL,	[UserId] [bigint] NOT NULL,	[UserClaimId] [bigint] NOT NULL, CONSTRAINT [PK_User_UserClaim] PRIMARY KEY CLUSTERED (	[Id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]

CREATE TABLE [dbo].[UserClaim](	[Id] [bigint] IDENTITY(1,1) NOT NULL,	[Type] [nvarchar](128) NOT NULL,	[Value] [nvarchar](max) NOT NULL,	[Active] [bit] NOT NULL,	[DateCreated] [datetime2](7) NOT NULL,	[DateModified] [datetime2](7) NOT NULL,	[User_Creator_Id] [bigint] NOT NULL,	[User_Modifier_Id] [bigint] NOT NULL, CONSTRAINT [PK_UserClaim] PRIMARY KEY CLUSTERED (	[Id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[Attachment]  WITH CHECK ADD  CONSTRAINT [FK_Attachment_Recording] FOREIGN KEY([RecordingId])REFERENCES [dbo].[Recording] ([Id])ON UPDATE CASCADE ON DELETE CASCADE

ALTER TABLE [dbo].[Attachment] CHECK CONSTRAINT [FK_Attachment_Recording]

ALTER TABLE [dbo].[Attachment]  WITH CHECK ADD  CONSTRAINT [FK_Attachment_User] FOREIGN KEY([User_Creator_Id])REFERENCES [dbo].[User] ([Id])

ALTER TABLE [dbo].[Attachment] CHECK CONSTRAINT [FK_Attachment_User]

ALTER TABLE [dbo].[Attachment]  WITH CHECK ADD  CONSTRAINT [FK_Attachment_User1] FOREIGN KEY([User_Modifier_Id])REFERENCES [dbo].[User] ([Id])

ALTER TABLE [dbo].[Attachment] CHECK CONSTRAINT [FK_Attachment_User1]

ALTER TABLE [dbo].[Recording]  WITH CHECK ADD  CONSTRAINT [FK_Recording_User] FOREIGN KEY([User_Creator_Id])REFERENCES [dbo].[User] ([Id])

ALTER TABLE [dbo].[Recording] CHECK CONSTRAINT [FK_Recording_User]

ALTER TABLE [dbo].[Recording]  WITH CHECK ADD  CONSTRAINT [FK_Recording_User1] FOREIGN KEY([User_Modifier_Id])REFERENCES [dbo].[User] ([Id])

ALTER TABLE [dbo].[Recording] CHECK CONSTRAINT [FK_Recording_User1]

ALTER TABLE [dbo].[Recording]  WITH CHECK ADD  CONSTRAINT [FK_Recording_User2] FOREIGN KEY([User_Assigned_To])REFERENCES [dbo].[User] ([Id])

ALTER TABLE [dbo].[Recording] CHECK CONSTRAINT [FK_Recording_User2]

ALTER TABLE [dbo].[Secretary_Employee]  WITH CHECK ADD  CONSTRAINT [FK_Secretary_Employee_User] FOREIGN KEY([EmployeeId])REFERENCES [dbo].[User] ([Id])

ALTER TABLE [dbo].[Secretary_Employee] CHECK CONSTRAINT [FK_Secretary_Employee_User]

ALTER TABLE [dbo].[Secretary_Employee]  WITH CHECK ADD  CONSTRAINT [FK_Secretary_Employee_User1] FOREIGN KEY([SecretaryId])REFERENCES [dbo].[User] ([Id])

ALTER TABLE [dbo].[Secretary_Employee] CHECK CONSTRAINT [FK_Secretary_Employee_User1]

ALTER TABLE [dbo].[User_UserClaim]  WITH CHECK ADD  CONSTRAINT [FK_User_UserClaim_User] FOREIGN KEY([UserId])REFERENCES [dbo].[User] ([Id]) ON UPDATE CASCADE ON DELETE CASCADE

ALTER TABLE [dbo].[User_UserClaim] CHECK CONSTRAINT [FK_User_UserClaim_User]

ALTER TABLE [dbo].[User_UserClaim]  WITH CHECK ADD  CONSTRAINT [FK_User_UserClaim_UserClaim] FOREIGN KEY([UserClaimId])REFERENCES [dbo].[UserClaim] ([Id])ON UPDATE CASCADE ON DELETE CASCADE

ALTER TABLE [dbo].[User_UserClaim] CHECK CONSTRAINT [FK_User_UserClaim_UserClaim]

ALTER TABLE [dbo].[UserClaim]  WITH CHECK ADD  CONSTRAINT [FK_UserClaim_User] FOREIGN KEY([User_Creator_Id])REFERENCES [dbo].[User] ([Id])

ALTER TABLE [dbo].[UserClaim] CHECK CONSTRAINT [FK_UserClaim_User]

ALTER TABLE [dbo].[UserClaim]  WITH CHECK ADD  CONSTRAINT [FK_UserClaim_User1] FOREIGN KEY([User_Modifier_Id])REFERENCES [dbo].[User] ([Id])

ALTER TABLE [dbo].[UserClaim] CHECK CONSTRAINT [FK_UserClaim_User1]