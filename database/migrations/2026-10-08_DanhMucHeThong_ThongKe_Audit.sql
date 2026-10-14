-- Danh mục hệ thống dùng bảng DanhMuc hiện có, MaNguoiDung = NULL.
-- Không thêm bảng, không thay đổi dữ liệu ví/giao dịch/danh mục cá nhân.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 15000;
BEGIN TRY
    BEGIN TRANSACTION;
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.DanhMuc')
        AND name = N'MaNguoiDung' AND is_nullable = 0)
    BEGIN
        DROP INDEX IF EXISTS UX_DanhMuc_NguoiDung_TenDanhMuc ON dbo.DanhMuc;
        DROP INDEX IF EXISTS IX_DanhMuc_MaNguoiDung ON dbo.DanhMuc;
        ALTER TABLE dbo.DanhMuc ALTER COLUMN MaNguoiDung INT NULL;
    END;
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.DanhMuc')
        AND name = N'UX_DanhMuc_NguoiDung_TenDanhMuc')
        CREATE UNIQUE INDEX UX_DanhMuc_NguoiDung_TenDanhMuc ON dbo.DanhMuc(MaNguoiDung, TenDanhMuc) WHERE NgayXoa IS NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.DanhMuc') AND name = N'IX_DanhMuc_MaNguoiDung')
        CREATE INDEX IX_DanhMuc_MaNguoiDung ON dbo.DanhMuc(MaNguoiDung);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.NhatKyAudit') AND name = N'IX_NhatKyAudit_ThoiGian')
        CREATE INDEX IX_NhatKyAudit_ThoiGian ON dbo.NhatKyAudit(ThoiGian DESC, MaAudit DESC);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
PRINT N'Đã cập nhật danh mục hệ thống và index Audit Log.';
