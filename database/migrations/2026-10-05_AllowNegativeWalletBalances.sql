/*
   Run in the application's database before using the updated transaction API.
   Allows a wallet balance below zero when recorded expenses exceed income.
   Keeps CK_GiaoDich_SoTien (SoTien > 0) for both income and expenses.
   Does not change existing balances or transactions. Safe to run again.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Vi', N'U') IS NULL
        THROW 51001, 'Không tìm thấy bảng dbo.Vi; hãy chọn đúng database.', 1;

    IF EXISTS
    (
        SELECT 1 FROM sys.check_constraints
        WHERE parent_object_id = OBJECT_ID(N'dbo.Vi')
          AND name = N'CK_Vi_SoDu'
    )
        ALTER TABLE dbo.Vi DROP CONSTRAINT CK_Vi_SoDu;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
