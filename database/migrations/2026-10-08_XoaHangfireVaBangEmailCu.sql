-- Chỉ chạy sau khi dừng mọi API/worker Hangfire cũ và dùng code BackgroundService mới.
-- Giữ nguyên MaKhoiPhuc, NhacNho, ThongBaoNhacNho, LanNhapGiaoDich và dữ liệu nghiệp vụ.
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'QuanLyNganQuyCaNhan'
    THROW 50001, N'Chọn đúng database QuanLyNganQuyCaNhan.', 1;

DECLARE @SchemaHangfire INT = SCHEMA_ID(N'HangFire');
-- Dừng nếu schema có bảng lạ thay vì xóa rộng tất cả bảng trong schema.
IF EXISTS (SELECT 1 FROM sys.tables WHERE schema_id = @SchemaHangfire AND name NOT IN
    (N'AggregatedCounter', N'Counter', N'Hash', N'Job', N'JobParameter', N'JobQueue',
     N'List', N'Schema', N'Server', N'Set', N'State'))
    THROW 50002, N'Schema HangFire có bảng ngoài danh sách chuẩn; chưa xóa bảng nào.', 1;

IF EXISTS (SELECT 1 FROM sys.tables WHERE schema_id = @SchemaHangfire)
    OR OBJECT_ID(N'dbo.CanhBaoNganSach', N'U') IS NOT NULL
    OR OBJECT_ID(N'dbo.DatLaiMatKhauToken', N'U') IS NOT NULL
BEGIN
    DECLARE @ThuMuc NVARCHAR(4000) = CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(4000));
    IF NULLIF(@ThuMuc, N'') IS NULL
        THROW 50003, N'Không xác định được thư mục backup; chưa xóa bảng nào.', 1;
    IF RIGHT(@ThuMuc, 1) NOT IN (N'\', N'/') SET @ThuMuc += N'\';
    DECLARE @FileBackup NVARCHAR(4000) = @ThuMuc + N'QuanLyNganQuyCaNhan_TruocXoaHangfire_'
        + CONVERT(NVARCHAR(36), NEWID()) + N'.bak';
    BACKUP DATABASE [QuanLyNganQuyCaNhan] TO DISK = @FileBackup WITH COPY_ONLY, CHECKSUM;
    RESTORE VERIFYONLY FROM DISK = @FileBackup WITH CHECKSUM;
    PRINT N'Backup: ' + @FileBackup;

    BEGIN TRANSACTION;
    -- Xóa bảng con trước bảng Job; không tự gỡ khóa ngoại của bảng nghiệp vụ.
    DROP TABLE IF EXISTS [HangFire].[JobParameter];
    DROP TABLE IF EXISTS [HangFire].[JobQueue];
    DROP TABLE IF EXISTS [HangFire].[State];
    DROP TABLE IF EXISTS [HangFire].[Job];
    DROP TABLE IF EXISTS [HangFire].[AggregatedCounter];
    DROP TABLE IF EXISTS [HangFire].[Counter];
    DROP TABLE IF EXISTS [HangFire].[Hash];
    DROP TABLE IF EXISTS [HangFire].[List];
    DROP TABLE IF EXISTS [HangFire].[Server];
    DROP TABLE IF EXISTS [HangFire].[Set];
    DROP TABLE IF EXISTS [HangFire].[Schema];
    DROP TABLE IF EXISTS dbo.CanhBaoNganSach;
    DROP TABLE IF EXISTS dbo.DatLaiMatKhauToken;
    COMMIT TRANSACTION;
END;

SELECT s.name AS TenSchema, t.name AS TenBang
FROM sys.tables t INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
ORDER BY s.name, t.name;
