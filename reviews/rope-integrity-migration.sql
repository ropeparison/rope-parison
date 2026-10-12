BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918090336_RopeNameUniquenessAndConcurrency'
)
BEGIN
    IF EXISTS (SELECT 1 FROM [Ropes] WHERE [Name] IS NULL
        OR LEN(LTRIM(RTRIM([Name]))) = 0 OR DATALENGTH([Name]) > 300)
        THROW 51001, 'Rope names must be nonblank and at most 150 characters. Resolve invalid names before migrating.', 1;
    IF EXISTS (SELECT 1 FROM [Ropes]
        GROUP BY LTRIM(RTRIM([Name])) COLLATE Latin1_General_100_CI_AS HAVING COUNT(*) > 1)
        THROW 51002, 'Duplicate rope names exist after trimming and ignoring case. Resolve duplicates before migrating.', 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918090336_RopeNameUniquenessAndConcurrency'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Ropes]') AND [c].[name] = N'Name');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Ropes] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Ropes] ALTER COLUMN [Name] nvarchar(150) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918090336_RopeNameUniquenessAndConcurrency'
)
BEGIN
    ALTER TABLE [Ropes] ADD [RowVersion] rowversion NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918090336_RopeNameUniquenessAndConcurrency'
)
BEGIN
    EXEC(N'ALTER TABLE [Ropes] ADD [NormalizedName] AS LTRIM(RTRIM([Name])) COLLATE Latin1_General_100_CI_AS PERSISTED');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918090336_RopeNameUniquenessAndConcurrency'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Ropes_NormalizedName] ON [Ropes] ([NormalizedName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918090336_RopeNameUniquenessAndConcurrency'
)
BEGIN
    EXEC(N'ALTER TABLE [Ropes] ADD CONSTRAINT [CK_Ropes_Name_NotBlank] CHECK (LEN(LTRIM(RTRIM([Name]))) > 0)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918090336_RopeNameUniquenessAndConcurrency'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260918090336_RopeNameUniquenessAndConcurrency', N'10.0.12');
END;

COMMIT;
GO
