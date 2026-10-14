/*
   Rebuild wallet balances from active transactions before enabling transaction balance sync.
   Assumption: all wallets were opened at 0 and their balances have not been adjusted manually.
   Review the assumption and take a database backup before running this script.
   Run 2026-10-05_AllowNegativeWalletBalances.sql first to allow expenses exceeding income.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @SoDuTheoGiaoDich TABLE
    (
        MaVi INT NOT NULL PRIMARY KEY,
        SoDu DECIMAL(38, 2) NOT NULL
    );

    INSERT INTO @SoDuTheoGiaoDich (MaVi, SoDu)
    SELECT MaVi,
           SUM(CASE WHEN LoaiGiaoDich = 'THU' THEN SoTien ELSE -SoTien END)
    FROM GiaoDich
    WHERE NgayXoa IS NULL
    GROUP BY MaVi;

    UPDATE v
    SET SoDu = COALESCE(sd.SoDu, 0),
        NgayCapNhat = SYSDATETIME()
    FROM Vi v
    LEFT JOIN @SoDuTheoGiaoDich sd ON sd.MaVi = v.MaVi;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
