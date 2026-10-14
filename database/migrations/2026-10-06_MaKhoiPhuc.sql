SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.MaKhoiPhuc', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MaKhoiPhuc
    (
        MaKhoiPhuc BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaNguoiDung INT NOT NULL,
        MaBoMa UNIQUEIDENTIFIER NOT NULL,
        MaDaBam BINARY(32) NOT NULL,
        NgayTao DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        NgaySuDung DATETIME2 NULL,
        NgayVoHieu DATETIME2 NULL,
        CONSTRAINT FK_MaKhoiPhuc_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES dbo.NguoiDung(MaNguoiDung),
        CONSTRAINT UQ_MaKhoiPhuc_NguoiDung_Hash UNIQUE (MaNguoiDung, MaDaBam)
    );
    CREATE INDEX IX_MaKhoiPhuc_NguoiDung ON dbo.MaKhoiPhuc(MaNguoiDung, NgayVoHieu, NgaySuDung);
END;
COMMIT TRANSACTION;
