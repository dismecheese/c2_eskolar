CREATE TABLE [Announcements] (
    [AnnouncementId] int NOT NULL IDENTITY,
    [Title] nvarchar(200) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [Summary] nvarchar(500) NULL,
    [AuthorId] nvarchar(max) NOT NULL,
    [AuthorName] nvarchar(max) NOT NULL,
    [AuthorType] int NOT NULL,
    [OrganizationName] nvarchar(200) NULL,
    [Category] nvarchar(100) NULL,
    [Priority] int NOT NULL,
    [IsPublic] bit NOT NULL,
    [TargetAudience] nvarchar(max) NULL,
    [ImageUrl] nvarchar(500) NULL,
    [AttachmentUrl] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    [IsPinned] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [PublishDate] datetime2 NULL,
    [ExpiryDate] datetime2 NULL,
    [ViewCount] int NOT NULL,
    [Tags] nvarchar(max) NULL,
    CONSTRAINT [PK_Announcements] PRIMARY KEY ([AnnouncementId])
);
GO


CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Benefactor] (
    [BenefactorId] int NOT NULL IDENTITY,
    [Name] nvarchar(255) NOT NULL,
    [Address] nvarchar(255) NULL,
    [Email] nvarchar(255) NULL,
    [ContactNumber] nvarchar(50) NULL,
    [Description] nvarchar(max) NULL,
    [Logo] nvarchar(255) NULL,
    CONSTRAINT [PK_Benefactor] PRIMARY KEY ([BenefactorId])
);
GO


CREATE TABLE [BenefactorProfiles] (
    [BenefactorProfileId] int NOT NULL IDENTITY,
    [UserId] nvarchar(max) NOT NULL,
    [AdminFirstName] nvarchar(50) NOT NULL,
    [AdminMiddleName] nvarchar(50) NULL,
    [AdminLastName] nvarchar(50) NOT NULL,
    [Sex] nvarchar(10) NULL,
    [Nationality] nvarchar(50) NULL,
    [BirthDate] datetime2 NULL,
    [AdminPosition] nvarchar(100) NULL,
    [OrganizationName] nvarchar(150) NOT NULL,
    [OrganizationType] nvarchar(100) NULL,
    [Address] nvarchar(255) NULL,
    [ContactNumber] nvarchar(15) NULL,
    [Website] nvarchar(100) NULL,
    [ContactEmail] nvarchar(100) NULL,
    [Mission] nvarchar(2000) NULL,
    [Description] nvarchar(2000) NULL,
    [Logo] nvarchar(255) NULL,
    [IsVerified] bit NOT NULL,
    [VerificationStatus] nvarchar(max) NULL,
    [VerificationDate] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_BenefactorProfiles] PRIMARY KEY ([BenefactorProfileId])
);
GO


CREATE TABLE [Institution] (
    [InstitutionId] int NOT NULL IDENTITY,
    [Name] nvarchar(255) NOT NULL,
    [Address] nvarchar(255) NULL,
    [Email] nvarchar(255) NULL,
    [ContactNumber] nvarchar(50) NULL,
    [Description] nvarchar(max) NULL,
    [Logo] nvarchar(255) NULL,
    CONSTRAINT [PK_Institution] PRIMARY KEY ([InstitutionId])
);
GO


CREATE TABLE [InstitutionProfiles] (
    [InstitutionProfileId] int NOT NULL IDENTITY,
    [UserId] nvarchar(max) NOT NULL,
    [AdminFirstName] nvarchar(50) NOT NULL,
    [AdminMiddleName] nvarchar(50) NULL,
    [AdminLastName] nvarchar(50) NOT NULL,
    [AdminPosition] nvarchar(100) NULL,
    [InstitutionName] nvarchar(150) NOT NULL,
    [InstitutionType] nvarchar(100) NULL,
    [Address] nvarchar(255) NULL,
    [ContactNumber] nvarchar(15) NULL,
    [Website] nvarchar(100) NULL,
    [ContactEmail] nvarchar(100) NULL,
    [Mission] nvarchar(2000) NULL,
    [Description] nvarchar(2000) NULL,
    [Logo] nvarchar(255) NULL,
    [TotalStudents] int NULL,
    [EstablishedDate] datetime2 NULL,
    [Accreditation] nvarchar(100) NULL,
    [IsVerified] bit NOT NULL,
    [VerificationStatus] nvarchar(max) NULL,
    [VerificationDate] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_InstitutionProfiles] PRIMARY KEY ([InstitutionProfileId])
);
GO


CREATE TABLE [PasswordResets] (
    [Id] int NOT NULL IDENTITY,
    [Email] nvarchar(max) NOT NULL,
    [Token] nvarchar(max) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsUsed] bit NOT NULL,
    [UsedAt] datetime2 NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    CONSTRAINT [PK_PasswordResets] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Role] (
    [RoleId] int NOT NULL IDENTITY,
    [RoleName] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Role] PRIMARY KEY ([RoleId])
);
GO


CREATE TABLE [ScholarshipTypes] (
    [ScholarshipTypeId] int NOT NULL IDENTITY,
    [TypeName] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_ScholarshipTypes] PRIMARY KEY ([ScholarshipTypeId])
);
GO


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [InstitutionBenefactorPartnerships] (
    [PartnershipId] int NOT NULL IDENTITY,
    [InstitutionId] int NOT NULL,
    [BenefactorId] int NOT NULL,
    [Status] nvarchar(50) NULL,
    [CreatedAt] datetime2 NULL,
    CONSTRAINT [PK_InstitutionBenefactorPartnerships] PRIMARY KEY ([PartnershipId]),
    CONSTRAINT [FK_InstitutionBenefactorPartnerships_Benefactor_BenefactorId] FOREIGN KEY ([BenefactorId]) REFERENCES [Benefactor] ([BenefactorId]) ON DELETE CASCADE,
    CONSTRAINT [FK_InstitutionBenefactorPartnerships_Institution_InstitutionId] FOREIGN KEY ([InstitutionId]) REFERENCES [Institution] ([InstitutionId]) ON DELETE CASCADE
);
GO


CREATE TABLE [User] (
    [UserId] nvarchar(450) NOT NULL,
    [Email] nvarchar(255) NOT NULL,
    [PasswordHash] nvarchar(255) NOT NULL,
    [RoleId] int NOT NULL,
    [IsVerified] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_User] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_User_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Role] ([RoleId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Scholarships] (
    [ScholarshipId] int NOT NULL IDENTITY,
    [Title] nvarchar(150) NOT NULL,
    [Description] nvarchar(2000) NULL,
    [Benefits] nvarchar(1000) NOT NULL,
    [MonetaryValue] decimal(18,2) NULL,
    [ApplicationDeadline] datetime2 NOT NULL,
    [Requirements] nvarchar(3000) NULL,
    [SlotsAvailable] int NULL,
    [MinimumGPA] decimal(18,2) NULL,
    [RequiredCourse] nvarchar(100) NULL,
    [RequiredYearLevel] int NULL,
    [RequiredUniversity] nvarchar(100) NULL,
    [IsActive] bit NOT NULL,
    [IsInternal] bit NOT NULL,
    [ExternalApplicationUrl] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [BenefactorProfileId] int NULL,
    [InstitutionProfileId] int NULL,
    [ScholarshipTypeId] int NULL,
    CONSTRAINT [PK_Scholarships] PRIMARY KEY ([ScholarshipId]),
    CONSTRAINT [FK_Scholarships_BenefactorProfiles_BenefactorProfileId] FOREIGN KEY ([BenefactorProfileId]) REFERENCES [BenefactorProfiles] ([BenefactorProfileId]),
    CONSTRAINT [FK_Scholarships_InstitutionProfiles_InstitutionProfileId] FOREIGN KEY ([InstitutionProfileId]) REFERENCES [InstitutionProfiles] ([InstitutionProfileId]),
    CONSTRAINT [FK_Scholarships_ScholarshipTypes_ScholarshipTypeId] FOREIGN KEY ([ScholarshipTypeId]) REFERENCES [ScholarshipTypes] ([ScholarshipTypeId])
);
GO


CREATE TABLE [AnnouncementRecipient] (
    [AnnouncementRecipientId] int NOT NULL IDENTITY,
    [AnnouncementId] int NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AnnouncementRecipient] PRIMARY KEY ([AnnouncementRecipientId]),
    CONSTRAINT [FK_AnnouncementRecipient_Announcements_AnnouncementId] FOREIGN KEY ([AnnouncementId]) REFERENCES [Announcements] ([AnnouncementId]) ON DELETE CASCADE,
    CONSTRAINT [FK_AnnouncementRecipient_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([UserId]) ON DELETE CASCADE
);
GO


CREATE TABLE [BenefactorAdminProfiles] (
    [BenefactorAdminProfileId] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [FullName] nvarchar(255) NULL,
    [BirthDate] datetime2 NULL,
    [Address] nvarchar(255) NULL,
    [ContactNumber] nvarchar(50) NULL,
    [BenefactorId] int NOT NULL,
    [Position] nvarchar(100) NULL,
    [ProfilePicture] nvarchar(255) NULL,
    [UserId1] nvarchar(450) NULL,
    CONSTRAINT [PK_BenefactorAdminProfiles] PRIMARY KEY ([BenefactorAdminProfileId]),
    CONSTRAINT [FK_BenefactorAdminProfiles_Benefactor_BenefactorId] FOREIGN KEY ([BenefactorId]) REFERENCES [Benefactor] ([BenefactorId]) ON DELETE CASCADE,
    CONSTRAINT [FK_BenefactorAdminProfiles_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([UserId]) ON DELETE CASCADE,
    CONSTRAINT [FK_BenefactorAdminProfiles_User_UserId1] FOREIGN KEY ([UserId1]) REFERENCES [User] ([UserId])
);
GO


CREATE TABLE [InstitutionAdminProfiles] (
    [InstitutionAdminProfileId] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [FullName] nvarchar(255) NULL,
    [BirthDate] datetime2 NULL,
    [Address] nvarchar(255) NULL,
    [ContactNumber] nvarchar(50) NULL,
    [InstitutionId] int NOT NULL,
    [Position] nvarchar(100) NULL,
    [ProfilePicture] nvarchar(255) NULL,
    [UserId1] nvarchar(450) NULL,
    CONSTRAINT [PK_InstitutionAdminProfiles] PRIMARY KEY ([InstitutionAdminProfileId]),
    CONSTRAINT [FK_InstitutionAdminProfiles_Institution_InstitutionId] FOREIGN KEY ([InstitutionId]) REFERENCES [Institution] ([InstitutionId]) ON DELETE CASCADE,
    CONSTRAINT [FK_InstitutionAdminProfiles_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([UserId]) ON DELETE CASCADE,
    CONSTRAINT [FK_InstitutionAdminProfiles_User_UserId1] FOREIGN KEY ([UserId1]) REFERENCES [User] ([UserId])
);
GO


CREATE TABLE [StudentProfiles] (
    [StudentProfileId] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [FirstName] nvarchar(50) NOT NULL,
    [MiddleName] nvarchar(50) NULL,
    [LastName] nvarchar(50) NOT NULL,
    [Sex] nvarchar(20) NULL,
    [Nationality] nvarchar(50) NULL,
    [PermanentAddress] nvarchar(255) NULL,
    [Email] nvarchar(100) NULL,
    [MobileNumber] nvarchar(15) NULL,
    [BirthDate] datetime2 NULL,
    [UniversityName] nvarchar(100) NULL,
    [YearLevel] int NULL,
    [Course] nvarchar(100) NULL,
    [StudentNumber] nvarchar(50) NULL,
    [ProfilePicture] nvarchar(255) NULL,
    [IsVerified] bit NOT NULL,
    [VerificationStatus] nvarchar(max) NULL,
    [VerificationDate] datetime2 NULL,
    [GPA] decimal(18,2) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_StudentProfiles] PRIMARY KEY ([StudentProfileId]),
    CONSTRAINT [FK_StudentProfiles_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([UserId]) ON DELETE CASCADE
);
GO


CREATE TABLE [VerificationDocuments] (
    [DocumentId] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [DocumentType] nvarchar(100) NULL,
    [FilePath] nvarchar(255) NULL,
    [OCRExtractedData] nvarchar(max) NULL,
    [UploadedAt] datetime2 NULL,
    [Status] nvarchar(50) NULL,
    CONSTRAINT [PK_VerificationDocuments] PRIMARY KEY ([DocumentId]),
    CONSTRAINT [FK_VerificationDocuments_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([UserId]) ON DELETE CASCADE
);
GO


CREATE TABLE [RecentlyViewedScholarships] (
    [ViewId] int NOT NULL IDENTITY,
    [StudentId] nvarchar(450) NOT NULL,
    [ScholarshipId] int NOT NULL,
    [ViewedAt] datetime2 NULL,
    CONSTRAINT [PK_RecentlyViewedScholarships] PRIMARY KEY ([ViewId]),
    CONSTRAINT [FK_RecentlyViewedScholarships_Scholarships_ScholarshipId] FOREIGN KEY ([ScholarshipId]) REFERENCES [Scholarships] ([ScholarshipId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RecentlyViewedScholarships_User_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [User] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [StudentProfileId] int NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUsers_StudentProfiles_StudentProfileId] FOREIGN KEY ([StudentProfileId]) REFERENCES [StudentProfiles] ([StudentProfileId])
);
GO


CREATE TABLE [ScholarshipApplications] (
    [ScholarshipApplicationId] int NOT NULL IDENTITY,
    [StudentProfileId] int NOT NULL,
    [ScholarshipId] int NOT NULL,
    [IsExternalApplication] bit NOT NULL,
    [ExternalApplicationUrl] nvarchar(500) NULL,
    [ExternalApplicationDate] datetime2 NULL,
    [HasAppliedExternally] bit NOT NULL,
    [PersonalStatement] nvarchar(2000) NULL,
    [UploadedDocuments] nvarchar(1000) NULL,
    [Status] nvarchar(50) NOT NULL,
    [ApplicationReference] nvarchar(100) NULL,
    [ReviewNotes] nvarchar(1000) NULL,
    [ReviewDate] datetime2 NULL,
    [ReviewedBy] nvarchar(max) NULL,
    [ApplicationDate] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_ScholarshipApplications] PRIMARY KEY ([ScholarshipApplicationId]),
    CONSTRAINT [FK_ScholarshipApplications_Scholarships_ScholarshipId] FOREIGN KEY ([ScholarshipId]) REFERENCES [Scholarships] ([ScholarshipId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ScholarshipApplications_StudentProfiles_StudentProfileId] FOREIGN KEY ([StudentProfileId]) REFERENCES [StudentProfiles] ([StudentProfileId]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_AnnouncementRecipient_AnnouncementId] ON [AnnouncementRecipient] ([AnnouncementId]);
GO


CREATE INDEX [IX_AnnouncementRecipient_UserId] ON [AnnouncementRecipient] ([UserId]);
GO


CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO


CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO


CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO


CREATE INDEX [IX_AspNetUsers_StudentProfileId] ON [AspNetUsers] ([StudentProfileId]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE INDEX [IX_BenefactorAdminProfiles_BenefactorId] ON [BenefactorAdminProfiles] ([BenefactorId]);
GO


CREATE INDEX [IX_BenefactorAdminProfiles_UserId] ON [BenefactorAdminProfiles] ([UserId]);
GO


CREATE UNIQUE INDEX [IX_BenefactorAdminProfiles_UserId1] ON [BenefactorAdminProfiles] ([UserId1]) WHERE [UserId1] IS NOT NULL;
GO


CREATE INDEX [IX_InstitutionAdminProfiles_InstitutionId] ON [InstitutionAdminProfiles] ([InstitutionId]);
GO


CREATE INDEX [IX_InstitutionAdminProfiles_UserId] ON [InstitutionAdminProfiles] ([UserId]);
GO


CREATE UNIQUE INDEX [IX_InstitutionAdminProfiles_UserId1] ON [InstitutionAdminProfiles] ([UserId1]) WHERE [UserId1] IS NOT NULL;
GO


CREATE INDEX [IX_InstitutionBenefactorPartnerships_BenefactorId] ON [InstitutionBenefactorPartnerships] ([BenefactorId]);
GO


CREATE INDEX [IX_InstitutionBenefactorPartnerships_InstitutionId] ON [InstitutionBenefactorPartnerships] ([InstitutionId]);
GO


CREATE INDEX [IX_RecentlyViewedScholarships_ScholarshipId] ON [RecentlyViewedScholarships] ([ScholarshipId]);
GO


CREATE INDEX [IX_RecentlyViewedScholarships_StudentId] ON [RecentlyViewedScholarships] ([StudentId]);
GO


CREATE INDEX [IX_ScholarshipApplications_ScholarshipId] ON [ScholarshipApplications] ([ScholarshipId]);
GO


CREATE INDEX [IX_ScholarshipApplications_StudentProfileId] ON [ScholarshipApplications] ([StudentProfileId]);
GO


CREATE INDEX [IX_Scholarships_BenefactorProfileId] ON [Scholarships] ([BenefactorProfileId]);
GO


CREATE INDEX [IX_Scholarships_InstitutionProfileId] ON [Scholarships] ([InstitutionProfileId]);
GO


CREATE INDEX [IX_Scholarships_ScholarshipTypeId] ON [Scholarships] ([ScholarshipTypeId]);
GO


CREATE UNIQUE INDEX [IX_StudentProfiles_UserId] ON [StudentProfiles] ([UserId]);
GO


CREATE INDEX [IX_User_RoleId] ON [User] ([RoleId]);
GO


CREATE INDEX [IX_VerificationDocuments_UserId] ON [VerificationDocuments] ([UserId]);
GO


