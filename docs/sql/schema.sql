-- ============================================================
-- GestaoWeb - Schema SQL Server 2022
-- Alternativa às migrations do EF Core
-- Execute este script no banco de dados antes de rodar a aplicação
-- ============================================================

USE GestaoWebDb;
GO

-- ============================================================
-- Tabelas do ASP.NET Core Identity
-- ============================================================

CREATE TABLE [AspNetRoles] (
    [Id]               NVARCHAR(450) NOT NULL,
    [Name]             NVARCHAR(256) NULL,
    [NormalizedName]   NVARCHAR(256) NULL,
    [ConcurrencyStamp] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName])
    WHERE [NormalizedName] IS NOT NULL;
GO

CREATE TABLE [AspNetUsers] (
    -- Colunas padrão do IdentityUser
    [Id]                   NVARCHAR(450)  NOT NULL,
    [UserName]             NVARCHAR(256)  NULL,
    [NormalizedUserName]   NVARCHAR(256)  NULL,
    [Email]                NVARCHAR(256)  NULL,
    [NormalizedEmail]      NVARCHAR(256)  NULL,
    [EmailConfirmed]       BIT            NOT NULL DEFAULT 0,
    [PasswordHash]         NVARCHAR(MAX)  NULL,
    [SecurityStamp]        NVARCHAR(MAX)  NULL,
    [ConcurrencyStamp]     NVARCHAR(MAX)  NULL,
    [PhoneNumber]          NVARCHAR(MAX)  NULL,
    [PhoneNumberConfirmed] BIT            NOT NULL DEFAULT 0,
    [TwoFactorEnabled]     BIT            NOT NULL DEFAULT 0,
    [LockoutEnd]           DATETIMEOFFSET NULL,
    [LockoutEnabled]       BIT            NOT NULL DEFAULT 0,
    [AccessFailedCount]    INT            NOT NULL DEFAULT 0,
    -- Colunas customizadas de AppUser
    [FullName]             NVARCHAR(MAX)  NOT NULL DEFAULT '',
    [BirthDate]            DATE           NOT NULL DEFAULT '1900-01-01',
    [HomePhone]            NVARCHAR(MAX)  NULL,
    [MobilePhone]          NVARCHAR(MAX)  NOT NULL DEFAULT '',
    [Address]              NVARCHAR(MAX)  NOT NULL DEFAULT '',
    [ProfilePhotoPath]     NVARCHAR(MAX)  NULL,
    [IsManager]            BIT            NOT NULL DEFAULT 0,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [UserNameIndex]  ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
CREATE        INDEX [EmailIndex]     ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id]         INT           NOT NULL IDENTITY,
    [RoleId]     NVARCHAR(450) NOT NULL,
    [ClaimType]  NVARCHAR(MAX) NULL,
    [ClaimValue] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_AspNetRoleClaims]    PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
        FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id]         INT           NOT NULL IDENTITY,
    [UserId]     NVARCHAR(450) NOT NULL,
    [ClaimType]  NVARCHAR(MAX) NULL,
    [ClaimValue] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_AspNetUserClaims]    PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider]       NVARCHAR(128) NOT NULL,
    [ProviderKey]         NVARCHAR(128) NOT NULL,
    [ProviderDisplayName] NVARCHAR(MAX) NULL,
    [UserId]              NVARCHAR(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins]    PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] NVARCHAR(450) NOT NULL,
    [RoleId] NVARCHAR(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles]          PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
        FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId]        NVARCHAR(450) NOT NULL,
    [LoginProvider] NVARCHAR(128) NOT NULL,
    [Name]          NVARCHAR(128) NOT NULL,
    [Value]         NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

-- ============================================================
-- Tabela de tarefas
-- ============================================================

CREATE TABLE [TaskItems] (
    [Id]           INT           NOT NULL IDENTITY,
    [Description]  NVARCHAR(MAX) NOT NULL DEFAULT '',
    [DueDate]      DATETIME2     NOT NULL,
    -- Status: 0 = Pending, 1 = InProgress, 2 = Completed
    [Status]       INT           NOT NULL DEFAULT 0,
    [AssignedToId] NVARCHAR(450) NOT NULL,
    [CreatedById]  NVARCHAR(450) NOT NULL,
    [CreatedAt]    DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    [CompletedAt]  DATETIME2     NULL,
    CONSTRAINT [PK_TaskItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TaskItems_AspNetUsers_AssignedToId]
        FOREIGN KEY ([AssignedToId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE RESTRICT,
    CONSTRAINT [FK_TaskItems_AspNetUsers_CreatedById]
        FOREIGN KEY ([CreatedById])  REFERENCES [AspNetUsers] ([Id]) ON DELETE RESTRICT
);
GO

CREATE INDEX [IX_TaskItems_AssignedToId] ON [TaskItems] ([AssignedToId]);
CREATE INDEX [IX_TaskItems_CreatedById]  ON [TaskItems] ([CreatedById]);
GO
