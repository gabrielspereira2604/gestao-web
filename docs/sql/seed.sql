-- ============================================================
-- GestaoWeb - Seed: usuário gestor inicial
-- Email: ti@leveinvestimentos.com.br  /  Senha: teste123
--
-- Execute APÓS schema.sql e SOMENTE se a tabela AspNetUsers
-- estiver vazia (a aplicação também executa este seed
-- automaticamente na primeira inicialização via SeedData.cs).
-- ============================================================

USE GestaoWebDb;
GO

IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [NormalizedEmail] = 'TI@LEVEINVESTIMENTOS.COM.BR')
BEGIN
    DECLARE @userId NVARCHAR(450) = NEWID();

    INSERT INTO [AspNetUsers] (
        [Id],
        [UserName],
        [NormalizedUserName],
        [Email],
        [NormalizedEmail],
        [EmailConfirmed],
        [PasswordHash],
        [SecurityStamp],
        [ConcurrencyStamp],
        [PhoneNumber],
        [PhoneNumberConfirmed],
        [TwoFactorEnabled],
        [LockoutEnabled],
        [AccessFailedCount],
        [FullName],
        [BirthDate],
        [HomePhone],
        [MobilePhone],
        [Address],
        [ProfilePhotoPath],
        [IsManager]
    ) VALUES (
        @userId,
        'ti@leveinvestimentos.com.br',
        'TI@LEVEINVESTIMENTOS.COM.BR',
        'ti@leveinvestimentos.com.br',
        'TI@LEVEINVESTIMENTOS.COM.BR',
        1,                            -- EmailConfirmed
        -- Hash gerado pelo PasswordHasher do ASP.NET Core Identity para "teste123"
        'AQAAAAIAAYagAAAAEEx1BZpO6UQscQcGM1t1NAiD+Puj3sss7RSawN9Ujk4AuKt8G7m3EE2tmuLpq5F99Q==',
        NEWID(),                      -- SecurityStamp
        NEWID(),                      -- ConcurrencyStamp
        NULL,                         -- PhoneNumber
        0,                            -- PhoneNumberConfirmed
        0,                            -- TwoFactorEnabled
        1,                            -- LockoutEnabled
        0,                            -- AccessFailedCount
        'TI Leve Investimentos',      -- FullName
        '1990-01-01',                 -- BirthDate
        NULL,                         -- HomePhone
        '(11) 99999-0000',            -- MobilePhone
        'Rua Exemplo, 123',           -- Address
        NULL,                         -- ProfilePhotoPath
        1                             -- IsManager = true
    );

    PRINT 'Usuário gestor criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Usuário gestor já existe. Nenhuma alteração realizada.';
END
GO
