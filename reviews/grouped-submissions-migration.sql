BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918143951_GroupedRopeEditSubmissions'
)
BEGIN
    DROP INDEX [IX_RopeEditSuggestions_RopeId] ON [RopeEditSuggestions];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918143951_GroupedRopeEditSubmissions'
)
BEGIN
    ALTER TABLE [RopeEditSuggestions] ADD [SubmissionId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918143951_GroupedRopeEditSubmissions'
)
BEGIN
    CREATE INDEX [IX_RopeEditSuggestions_RopeId_SubmissionId] ON [RopeEditSuggestions] ([RopeId], [SubmissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918143951_GroupedRopeEditSubmissions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260918143951_GroupedRopeEditSubmissions', N'10.0.12');
END;

COMMIT;
GO
