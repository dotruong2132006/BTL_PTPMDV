-- Script khoi tao database moi, cap nhat theo project ngay 08/10/2026.
-- Database dang su dung: ap dung migration trong project thay vi chay lai script tao bang.

IF DB_ID(N'QuanLyNganQuyCaNhan') IS NULL
BEGIN
    CREATE DATABASE QuanLyNganQuyCaNhan;
END
GO

USE QuanLyNganQuyCaNhan;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
GO
--Bảng
CREATE TABLE VaiTro
(
    MaVaiTro INT IDENTITY(1,1) NOT NULL,
    TenVaiTro NVARCHAR(50) NOT NULL,
    MoTa NVARCHAR(255) NULL,
    CONSTRAINT PK_VaiTro PRIMARY KEY (MaVaiTro),
    CONSTRAINT UQ_VaiTro_TenVaiTro UNIQUE (TenVaiTro));
GO

CREATE TABLE NguoiDung
(
    MaNguoiDung INT IDENTITY(1,1) NOT NULL,
    TenDangNhap NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    MatKhau NVARCHAR(255) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    SoDienThoai VARCHAR(20) NULL,
    DaBat2FA BIT NOT NULL CONSTRAINT DF_NguoiDung_DaBat2FA DEFAULT 0,
    MaBiMat2FA NVARCHAR(255) NULL,
    TrangThai BIT NOT NULL CONSTRAINT DF_NguoiDung_TrangThai DEFAULT 1,
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_NguoiDung_NgayTao DEFAULT SYSDATETIME(),
    NgayCapNhat DATETIME2 NULL,
    NgayXoa DATETIME2 NULL,CONSTRAINT PK_NguoiDung PRIMARY KEY (MaNguoiDung));
GO

CREATE TABLE NguoiDung_VaiTro
(
    MaNguoiDung INT NOT NULL,
    MaVaiTro INT NOT NULL,
    CONSTRAINT PK_NguoiDung_VaiTro PRIMARY KEY (MaNguoiDung, MaVaiTro),
    CONSTRAINT FK_NguoiDung_VaiTro_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    CONSTRAINT FK_NguoiDung_VaiTro_VaiTro FOREIGN KEY (MaVaiTro) REFERENCES VaiTro(MaVaiTro));
GO

CREATE TABLE Vi
(
    MaVi INT IDENTITY(1,1) NOT NULL,
    MaNguoiDung INT NOT NULL,
    TenVi NVARCHAR(100) NOT NULL,
    SoDu DECIMAL(18,2) NOT NULL CONSTRAINT DF_Vi_SoDu DEFAULT 0,
    MoTa NVARCHAR(255) NULL,
    TrangThai BIT NOT NULL CONSTRAINT DF_Vi_TrangThai DEFAULT 1,
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_Vi_NgayTao DEFAULT SYSDATETIME(),
    NgayCapNhat DATETIME2 NULL,
    NgayXoa DATETIME2 NULL, 
    CONSTRAINT PK_Vi PRIMARY KEY (MaVi),
    CONSTRAINT FK_Vi_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung));
GO

CREATE TABLE DanhMuc
(
    MaDanhMuc INT IDENTITY(1,1) NOT NULL,
    MaNguoiDung INT NULL, -- NULL: danh mục hệ thống do Admin quản lý
    TenDanhMuc NVARCHAR(100) NOT NULL,
    LoaiDanhMuc VARCHAR(10) NOT NULL,
    MoTa NVARCHAR(255) NULL,
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_DanhMuc_NgayTao DEFAULT SYSDATETIME(),
    NgayXoa DATETIME2 NULL,
    CONSTRAINT PK_DanhMuc PRIMARY KEY (MaDanhMuc),
    CONSTRAINT FK_DanhMuc_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    CONSTRAINT CK_DanhMuc_Loai CHECK (LoaiDanhMuc IN ('THU', 'CHI')));
GO

CREATE TABLE GiaoDich
(
    MaGiaoDich BIGINT IDENTITY(1,1) NOT NULL,
    MaNguoiDung INT NOT NULL,
    MaVi INT NOT NULL,
    MaDanhMuc INT NOT NULL,
    LoaiGiaoDich VARCHAR(10) NOT NULL,
    SoTien DECIMAL(18,2) NOT NULL,
    NoiDung NVARCHAR(255) NULL,
    NgayGiaoDich DATETIME2 NOT NULL,
    PhienBan INT NOT NULL CONSTRAINT DF_GiaoDich_PhienBan DEFAULT 1,
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_GiaoDich_NgayTao DEFAULT SYSDATETIME(),
    NgayCapNhat DATETIME2 NULL,
    NgayXoa DATETIME2 NULL,
    CONSTRAINT PK_GiaoDich PRIMARY KEY (MaGiaoDich),
    CONSTRAINT FK_GiaoDich_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    CONSTRAINT FK_GiaoDich_Vi FOREIGN KEY (MaVi) REFERENCES Vi(MaVi),
    CONSTRAINT FK_GiaoDich_DanhMuc FOREIGN KEY (MaDanhMuc) REFERENCES DanhMuc(MaDanhMuc),
    CONSTRAINT CK_GiaoDich_Loai CHECK (LoaiGiaoDich IN ('THU', 'CHI')),
    CONSTRAINT CK_GiaoDich_SoTien CHECK (SoTien > 0),
    CONSTRAINT CK_GiaoDich_PhienBan CHECK (PhienBan >= 1));
GO

CREATE TABLE NganSach
(
    MaNganSach INT IDENTITY(1,1) NOT NULL,
    MaNguoiDung INT NOT NULL,
    MaDanhMuc INT NOT NULL,
    SoTienGioiHan DECIMAL(18,2) NOT NULL,
    Thang INT NOT NULL,
    Nam INT NOT NULL,
    PhanTramCanhBao DECIMAL(5,2) NOT NULL CONSTRAINT DF_NganSach_PhanTramCanhBao DEFAULT 80,
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_NganSach_NgayTao DEFAULT SYSDATETIME(),
    NgayCapNhat DATETIME2 NULL,
    NgayXoa DATETIME2 NULL,
    CONSTRAINT PK_NganSach PRIMARY KEY (MaNganSach),
    CONSTRAINT FK_NganSach_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    CONSTRAINT FK_NganSach_DanhMuc FOREIGN KEY (MaDanhMuc) REFERENCES DanhMuc(MaDanhMuc),
    CONSTRAINT CK_NganSach_SoTien CHECK (SoTienGioiHan > 0),
    CONSTRAINT CK_NganSach_Thang CHECK (Thang BETWEEN 1 AND 12),
    CONSTRAINT CK_NganSach_Nam CHECK (Nam >= 2000),
    CONSTRAINT CK_NganSach_CanhBao CHECK (PhanTramCanhBao > 0 AND PhanTramCanhBao <= 100));
GO

CREATE TABLE NhacNho
(
    MaNhacNho INT IDENTITY(1,1) NOT NULL,
    MaNguoiDung INT NOT NULL,
    TieuDe NVARCHAR(200) NOT NULL,
    NoiDung NVARCHAR(500) NULL,
    NgayNhac DATETIME2 NOT NULL,
    NgayNhacTiepTheo DATETIME2 NULL,
    PhienBan INT NOT NULL CONSTRAINT DF_NhacNho_PhienBan DEFAULT 1,
    LapLai BIT NOT NULL CONSTRAINT DF_NhacNho_LapLai DEFAULT 0,
    KieuLapLai NVARCHAR(50) NULL,
    TrangThai BIT NOT NULL CONSTRAINT DF_NhacNho_TrangThai DEFAULT 1,
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_NhacNho_NgayTao DEFAULT SYSDATETIME(),
    NgayCapNhat DATETIME2 NULL,
    NgayXoa DATETIME2 NULL,
    CONSTRAINT PK_NhacNho PRIMARY KEY (MaNhacNho),
    CONSTRAINT FK_NhacNho_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung));
GO

CREATE TABLE RefreshToken
(
    MaRefreshToken BIGINT IDENTITY(1,1) NOT NULL,
    MaNguoiDung INT NOT NULL,
    Token NVARCHAR(500) NOT NULL,
    NgayHetHan DATETIME2 NOT NULL,
    DaThuHoi BIT NOT NULL CONSTRAINT DF_RefreshToken_DaThuHoi DEFAULT 0,
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_RefreshToken_NgayTao DEFAULT SYSDATETIME(),
    CONSTRAINT PK_RefreshToken PRIMARY KEY (MaRefreshToken),
    CONSTRAINT FK_RefreshToken_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    CONSTRAINT UQ_RefreshToken_Token UNIQUE (Token));
GO


CREATE TABLE MaKhoiPhuc
(
    MaKhoiPhuc BIGINT IDENTITY(1,1) NOT NULL,
    MaNguoiDung INT NOT NULL,
    MaBoMa UNIQUEIDENTIFIER NOT NULL,
    MaDaBam BINARY(32) NOT NULL,
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_MaKhoiPhuc_NgayTao DEFAULT SYSUTCDATETIME(),
    NgaySuDung DATETIME2 NULL,
    NgayVoHieu DATETIME2 NULL,
    CONSTRAINT PK_MaKhoiPhuc PRIMARY KEY (MaKhoiPhuc),
    CONSTRAINT FK_MaKhoiPhuc_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    CONSTRAINT UQ_MaKhoiPhuc_NguoiDung_Hash UNIQUE (MaNguoiDung, MaDaBam));
GO

CREATE TABLE ThongBaoNhacNho
(
    MaThongBao BIGINT IDENTITY(1,1) NOT NULL,
    MaNhacNho INT NOT NULL,
    MaNguoiDung INT NOT NULL,
    TieuDe NVARCHAR(200) NOT NULL,
    NoiDung NVARCHAR(500) NULL,
    NgayDenHan DATETIME2 NOT NULL,
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_ThongBaoNhacNho_NgayTao DEFAULT SYSUTCDATETIME(),
    NgayDoc DATETIME2 NULL,
    CONSTRAINT PK_ThongBaoNhacNho PRIMARY KEY (MaThongBao),
    CONSTRAINT FK_ThongBaoNhacNho_NhacNho FOREIGN KEY (MaNhacNho) REFERENCES NhacNho(MaNhacNho),
    CONSTRAINT FK_ThongBaoNhacNho_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    CONSTRAINT UQ_ThongBaoNhacNho_LanNhac UNIQUE (MaNhacNho, NgayDenHan));
GO

CREATE TABLE LanNhapGiaoDich
(
    MaLanNhap BIGINT IDENTITY(1,1) NOT NULL,
    MaNguoiDung INT NOT NULL,
    MaYeuCau UNIQUEIDENTIFIER NOT NULL,
    HashFile BINARY(32) NOT NULL,
    SoDong INT NOT NULL,
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_LanNhapGiaoDich_NgayTao DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_LanNhapGiaoDich PRIMARY KEY (MaLanNhap),
    CONSTRAINT FK_LanNhapGiaoDich_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    CONSTRAINT UQ_LanNhapGiaoDich_YeuCau UNIQUE (MaNguoiDung, MaYeuCau));
GO

CREATE TABLE BaoCaoThang
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
    CONSTRAINT FK_BaoCaoThang_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    CONSTRAINT UQ_BaoCaoThang_YeuCau UNIQUE (MaNguoiDung, MaYeuCau),
    CONSTRAINT CK_BaoCaoThang_Ky CHECK (Thang BETWEEN 1 AND 12 AND Nam BETWEEN 2000 AND 9998),
    CONSTRAINT CK_BaoCaoThang_TrangThai CHECK (TrangThai IN ('CHO_XU_LY', 'DANG_XU_LY', 'HOAN_THANH', 'LOI')),
    CONSTRAINT CK_BaoCaoThang_SoLanThu CHECK (SoLanThu BETWEEN 0 AND 3),
    CONSTRAINT CK_BaoCaoThang_File CHECK (TrangThai <> 'HOAN_THANH' OR FilePDF IS NOT NULL)
);
GO

CREATE TABLE NhatKyAudit
(
    MaAudit BIGINT IDENTITY(1,1) NOT NULL,
    MaNguoiDung INT NULL,
    HanhDong NVARCHAR(50) NOT NULL,
    TenBang NVARCHAR(100) NOT NULL,
    MaBanGhi NVARCHAR(100) NULL,
    DuLieuCu NVARCHAR(MAX) NULL,
    DuLieuMoi NVARCHAR(MAX) NULL,
    DiaChiIP VARCHAR(50) NULL,
    ThoiGian DATETIME2 NOT NULL CONSTRAINT DF_NhatKyAudit_ThoiGian DEFAULT SYSDATETIME(),
    CONSTRAINT PK_NhatKyAudit PRIMARY KEY (MaAudit),
    CONSTRAINT FK_NhatKyAudit_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung));
GO

CREATE UNIQUE INDEX UX_NguoiDung_TenDangNhap
ON NguoiDung(TenDangNhap);
GO

CREATE UNIQUE INDEX UX_NguoiDung_Email
ON NguoiDung(Email);
GO

CREATE UNIQUE INDEX UX_DanhMuc_NguoiDung_TenDanhMuc
ON DanhMuc(MaNguoiDung, TenDanhMuc)
WHERE NgayXoa IS NULL;
GO

CREATE UNIQUE INDEX UX_NganSach_NguoiDung_DanhMuc_Thang_Nam
ON NganSach(MaNguoiDung,MaDanhMuc,Thang,Nam)
WHERE NgayXoa IS NULL;
GO

CREATE INDEX IX_GiaoDich_NguoiDung_NgayGiaoDich
ON GiaoDich(MaNguoiDung,NgayGiaoDich);
GO

CREATE INDEX IX_Vi_MaNguoiDung
ON Vi(MaNguoiDung);
GO

CREATE INDEX IX_DanhMuc_MaNguoiDung
ON DanhMuc(MaNguoiDung);
GO

CREATE INDEX IX_NhatKyAudit_ThoiGian
ON NhatKyAudit(ThoiGian DESC, MaAudit DESC);
GO

CREATE INDEX IX_GiaoDich_MaVi
ON GiaoDich(MaVi);
GO

CREATE INDEX IX_GiaoDich_MaDanhMuc
ON GiaoDich(MaDanhMuc);
GO

CREATE INDEX IX_NganSach_MaNguoiDung
ON NganSach(MaNguoiDung);
GO

CREATE INDEX IX_NhacNho_MaNguoiDung
ON NhacNho(MaNguoiDung);
GO

CREATE INDEX IX_RefreshToken_MaNguoiDung
ON RefreshToken(MaNguoiDung);
GO


CREATE INDEX IX_NhatKyAudit_NguoiDung_ThoiGian
ON NhatKyAudit(MaNguoiDung, ThoiGian);
GO

CREATE INDEX IX_MaKhoiPhuc_NguoiDung
ON MaKhoiPhuc(MaNguoiDung, NgayVoHieu, NgaySuDung);
GO

CREATE INDEX IX_ThongBaoNhacNho_NguoiDung
ON ThongBaoNhacNho(MaNguoiDung, NgayDoc, MaThongBao);
GO

CREATE INDEX IX_BaoCaoThang_HangDoi
ON BaoCaoThang(TrangThai, NgayThuLai, MaBaoCao);
GO

CREATE INDEX IX_BaoCaoThang_NguoiDung
ON BaoCaoThang(MaNguoiDung, MaBaoCao);
GO

INSERT INTO VaiTro (TenVaiTro, MoTa) VALUES
(N'Admin', N'Quản trị toàn bộ hệ thống'),
(N'User', N'Người dùng cá nhân');
GO

