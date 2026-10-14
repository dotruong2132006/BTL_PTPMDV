-- Chạy sau khi triển khai MaKhoiPhucRepository mới, không còn truy cập token email.
-- Sao lưu toàn bộ database bằng COPY_ONLY trước khi xóa dữ liệu của hai tính năng đã bỏ.
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'QuanLyNganQuyCaNhan'
    THROW 50001, N'Chọn đúng database QuanLyNganQuyCaNhan trước khi chạy script.', 1;

IF OBJECT_ID(N'dbo.CanhBaoNganSach', N'U') IS NOT NULL
    OR OBJECT_ID(N'dbo.DatLaiMatKhauToken', N'U') IS NOT NULL
BEGIN
    DECLARE @ThuMucSaoLuu NVARCHAR(4000) = CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(4000));
    IF NULLIF(@ThuMucSaoLuu, N'') IS NULL
        THROW 50002, N'Không xác định được thư mục backup; chưa xóa bảng nào.', 1;
    IF RIGHT(@ThuMucSaoLuu, 1) NOT IN (N'\', N'/') SET @ThuMucSaoLuu += N'\';
    DECLARE @DuongDanSaoLuu NVARCHAR(4000) = @ThuMucSaoLuu
        + N'QuanLyNganQuyCaNhan_TruocXoaBangEmailCu_'
        + CONVERT(NVARCHAR(36), NEWID()) + N'.bak';

    BACKUP DATABASE [QuanLyNganQuyCaNhan] TO DISK = @DuongDanSaoLuu
        WITH COPY_ONLY, CHECKSUM;
    RESTORE VERIFYONLY FROM DISK = @DuongDanSaoLuu WITH CHECKSUM;
    PRINT N'Backup: ' + @DuongDanSaoLuu;

    BEGIN TRANSACTION;
    -- Không xóa tự động phụ thuộc từ bảng khác: nếu còn, DROP sẽ lỗi và rollback.
    DROP TABLE IF EXISTS dbo.CanhBaoNganSach;
    DROP TABLE IF EXISTS dbo.DatLaiMatKhauToken;
    COMMIT TRANSACTION;
END;

SELECT s.name AS TenSchema, t.name AS TenBang
FROM sys.tables t INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
ORDER BY s.name, t.name;
