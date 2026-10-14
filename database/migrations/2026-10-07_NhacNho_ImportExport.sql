SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('dbo.NhacNho', 'NgayXoa') IS NULL ALTER TABLE dbo.NhacNho ADD NgayXoa DATETIME2 NULL;
IF COL_LENGTH('dbo.NhacNho', 'NgayCapNhat') IS NULL ALTER TABLE dbo.NhacNho ADD NgayCapNhat DATETIME2 NULL;
IF COL_LENGTH('dbo.NhacNho', 'PhienBan') IS NULL ALTER TABLE dbo.NhacNho ADD PhienBan INT NOT NULL CONSTRAINT DF_NhacNho_PhienBan DEFAULT 1;
IF COL_LENGTH('dbo.NhacNho', 'NgayNhacTiepTheo') IS NULL
BEGIN
    ALTER TABLE dbo.NhacNho ADD NgayNhacTiepTheo DATETIME2 NULL;
    -- Giữ nguyên lịch cũ, giả định NgayNhac đã là UTC; kiểm tra dữ liệu cũ trước khi bật job.
    EXEC(N'UPDATE dbo.NhacNho SET NgayNhacTiepTheo = NgayNhac WHERE TrangThai = 1;');
END;
IF OBJECT_ID(N'dbo.ThongBaoNhacNho', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ThongBaoNhacNho
    (
        MaThongBao BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaNhacNho INT NOT NULL,
        MaNguoiDung INT NOT NULL,
        TieuDe NVARCHAR(200) NOT NULL,
        NoiDung NVARCHAR(500) NULL,
        NgayDenHan DATETIME2 NOT NULL,
        NgayTao DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        NgayDoc DATETIME2 NULL,
        CONSTRAINT FK_ThongBaoNhacNho_NhacNho FOREIGN KEY (MaNhacNho) REFERENCES dbo.NhacNho(MaNhacNho),
        CONSTRAINT FK_ThongBaoNhacNho_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES dbo.NguoiDung(MaNguoiDung),
        CONSTRAINT UQ_ThongBaoNhacNho_LanNhac UNIQUE (MaNhacNho, NgayDenHan)
    );
    CREATE INDEX IX_ThongBaoNhacNho_NguoiDung ON dbo.ThongBaoNhacNho(MaNguoiDung, NgayDoc, MaThongBao);
END;
IF OBJECT_ID(N'dbo.LanNhapGiaoDich', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LanNhapGiaoDich
    (
        MaLanNhap BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaNguoiDung INT NOT NULL,
        MaYeuCau UNIQUEIDENTIFIER NOT NULL,
        HashFile BINARY(32) NOT NULL,
        SoDong INT NOT NULL,
        NgayTao DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_LanNhapGiaoDich_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES dbo.NguoiDung(MaNguoiDung),
        CONSTRAINT UQ_LanNhapGiaoDich_YeuCau UNIQUE (MaNguoiDung, MaYeuCau)
    );
END;
COMMIT TRANSACTION;
