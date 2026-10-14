SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.BaoCaoThang', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BaoCaoThang
    (
        MaBaoCao BIGINT IDENTITY(1,1) NOT NULL,
        MaNguoiDung INT NOT NULL,
        MaYeuCau UNIQUEIDENTIFIER NOT NULL,
        Thang INT NOT NULL,
        Nam INT NOT NULL,
        TrangThai VARCHAR(20) NOT NULL CONSTRAINT DF_BaoCaoThang_TrangThai DEFAULT 'CHO_XU_LY',
        SoLanThu INT NOT NULL CONSTRAINT DF_BaoCaoThang_SoLanThu DEFAULT 0,
        NgayTao DATETIME2 NOT NULL CONSTRAINT DF_BaoCaoThang_NgayTao DEFAULT SYSUTCDATETIME(),
        NgayThuLai DATETIME2 NOT NULL CONSTRAINT DF_BaoCaoThang_NgayThuLai DEFAULT SYSUTCDATETIME(),
        NgayBatDau DATETIME2 NULL,
        NgayHoanThanh DATETIME2 NULL,
        HetHanXuLy DATETIME2 NULL,
        MaLanXuLy UNIQUEIDENTIFIER NULL,
        LoiCuoi NVARCHAR(500) NULL,
        FilePDF VARBINARY(MAX) NULL,
        CONSTRAINT PK_BaoCaoThang PRIMARY KEY (MaBaoCao),
        CONSTRAINT FK_BaoCaoThang_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES dbo.NguoiDung(MaNguoiDung),
        CONSTRAINT UQ_BaoCaoThang_YeuCau UNIQUE (MaNguoiDung, MaYeuCau),
        CONSTRAINT CK_BaoCaoThang_Ky CHECK (Thang BETWEEN 1 AND 12 AND Nam BETWEEN 2000 AND 9998),
        CONSTRAINT CK_BaoCaoThang_TrangThai CHECK (TrangThai IN ('CHO_XU_LY', 'DANG_XU_LY', 'HOAN_THANH', 'LOI')),
        CONSTRAINT CK_BaoCaoThang_SoLanThu CHECK (SoLanThu BETWEEN 0 AND 3),
        CONSTRAINT CK_BaoCaoThang_File CHECK (TrangThai <> 'HOAN_THANH' OR FilePDF IS NOT NULL)
    );
    CREATE INDEX IX_BaoCaoThang_HangDoi ON dbo.BaoCaoThang(TrangThai, NgayThuLai, MaBaoCao);
    CREATE INDEX IX_BaoCaoThang_NguoiDung ON dbo.BaoCaoThang(MaNguoiDung, MaBaoCao);
END;
COMMIT TRANSACTION;
