CREATE DATABASE WEBVER2;
GO
USE WEBVER2;
GO
/*
CREATE TRIGGER TG_TaiKhoan_MaTuDong
ON TaiKhoan
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @prefix NVARCHAR(2);
    DECLARE @next_id INT;
    DECLARE @VaiTro NVARCHAR(50);
    DECLARE @TenDangNhap NVARCHAR(100);
    DECLARE @MatKhau NVARCHAR(255);
    DECLARE @Email NVARCHAR(150);
    DECLARE @SoDienThoai NVARCHAR(20);
    DECLARE @AnhID INT;
    DECLARE @MaTaiKhoan NVARCHAR(20);

    -- Lấy dữ liệu từ bảng tạm inserted
    SELECT 
        @VaiTro = VaiTro,
        @TenDangNhap = TenDangNhap,
        @MatKhau = MatKhau,
        @Email = Email,
        @SoDienThoai = SoDienThoai,
        @AnhID = AnhID
    FROM inserted;

    -- Xác định tiền tố dựa theo VaiTro
    IF @VaiTro = 'Admin'
        SET @prefix = 'AD';
    ELSE IF @VaiTro = 'NguoiUngTuyen'
        SET @prefix = 'UR';
    ELSE IF @VaiTro = 'NhaTuyenDung'
        SET @prefix = 'HR';
    ELSE IF @VaiTro = 'CongTy'
        SET @prefix = 'CM';

    -- Lấy số thứ tự tiếp theo
    SELECT @next_id = COUNT(*) + 1 FROM TaiKhoan WHERE VaiTro = @VaiTro;

    -- Sinh mã dạng: AD001, HR002, ...
    SET @MaTaiKhoan = @prefix + RIGHT('000' + CAST(@next_id AS VARCHAR(3)), 3);

    -- Thực hiện chèn dữ liệu thật sự
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, Email, SoDienThoai, VaiTro, NgayTao, TrangThai, AnhID)
    SELECT 
        @TenDangNhap, 
        @MatKhau, 
        @Email, 
        @SoDienThoai, 
        @VaiTro, 
        GETDATE(), 
        1, 
        @AnhID;
END;
GO
*/
CREATE TABLE TaiKhoan (
    TaiKhoanID     INT IDENTITY(1,1) PRIMARY KEY,       -- Khóa chính
    TenDangNhap    VARCHAR(100) NOT NULL UNIQUE,        -- Tên đăng nhập duy nhất
    MatKhau        VARCHAR(255) NOT NULL,               -- Mật khẩu (đã mã hóa + salt)
    Email          VARCHAR(150) NOT NULL UNIQUE,        -- Email đăng ký
    SoDienThoai    VARCHAR(20),                         -- Số điện thoại
    VaiTro         NVARCHAR(30) NOT NULL CHECK (VaiTro IN (N'Admin', N'CongTy', N'NhaTuyenDung', N'NguoiUngTuyen')), 
                                                       -- Loại tài khoản
    NgayTao        DATETIME DEFAULT GETDATE(),          -- Ngày tạo tài khoản
    TrangThai      TINYINT DEFAULT 1,                   -- 1 = hoạt động, 0 = khóa
    AnhID          INT                                  -- Liên kết tới bảng Ảnh
);
GO

CREATE TABLE Admin (
    AdminID       INT IDENTITY(1,1) PRIMARY KEY,       -- Khóa chính
    TaiKhoanID    INT NOT NULL UNIQUE,                 -- Liên kết 1-1 đến bảng TaiKhoan
    HoTen         NVARCHAR(100) NOT NULL,              -- Họ và tên admin
    EmailLienHe   VARCHAR(150),                        -- Email nội bộ
    QuyenHan      NVARCHAR(100) DEFAULT N'ToanQuyen',  -- Mức quyền
    NgayTao       DATETIME DEFAULT GETDATE()           -- Ngày tạo
);
GO

CREATE TABLE CongTy (
    CongTyID        INT IDENTITY(1,1) PRIMARY KEY,     -- Khóa chính
    TaiKhoanID      INT NOT NULL UNIQUE,                -- Liên kết với bảng TaiKhoan
    TenCongTy       NVARCHAR(255) NOT NULL,             -- Tên công ty
    MaSoThue        VARCHAR(20),                        -- Mã số thuế
    LinhVucHoatDong NVARCHAR(150),                      -- Lĩnh vực hoạt động
    DiaChi          NVARCHAR(255),                      -- Địa chỉ
    SoDienThoai     VARCHAR(20),                        -- Số điện thoại
    EmailCongTy     VARCHAR(150),                       -- Email chính thức
    Website         VARCHAR(200),                       -- Trang web
    MoTa            NVARCHAR(1000),                     -- Mô tả
    NgayTao         DATETIME DEFAULT GETDATE(),          -- Ngày đăng ký
    TrangThai       TINYINT DEFAULT 1                   -- Trạng thái
);
GO

CREATE TABLE NhaTuyenDung (
    NhaTuyenDungID   INT IDENTITY(1,1) PRIMARY KEY,   -- Khóa chính
    TaiKhoanID       INT NOT NULL UNIQUE,             -- Liên kết 1-1 với bảng TaiKhoan
    CongTyID         INT,                             -- Liên kết tới bảng CôngTy
    HoTen            NVARCHAR(100) NOT NULL,          -- Họ tên người tuyển dụng
    ChucVu           NVARCHAR(100),                   -- Chức vụ
    EmailCongTy      VARCHAR(150),                    -- Email công ty
    SoDienThoai      VARCHAR(20),                     -- Liên hệ
    NgayTao          DATETIME DEFAULT GETDATE(),       -- Ngày tạo
    TrangThai        TINYINT DEFAULT 1                -- Trạng thái
);
GO

CREATE TABLE NguoiUngTuyen (
    NguoiUngTuyenID INT IDENTITY(1,1) PRIMARY KEY,   -- Khóa chính
    TaiKhoanID INT NOT NULL,                         -- Liên kết đến bảng Tài khoản
    HoTen NVARCHAR(100) NOT NULL,                    -- Họ tên
    NgaySinh DATE,                                   -- Ngày sinh
    GioiTinh NVARCHAR(5) NOT NULL CHECK (GioiTinh IN (N'Nam', N'Nữ')),-- Giới tính                                                     
    SDT VARCHAR(15),                                 -- Số điện thoại
    Email VARCHAR(100),                              -- Email
    DiaChi NVARCHAR(255),                            -- Địa chỉ
    GioiThieu NVARCHAR(500),                         -- Giới thiệu bản thân
    AnhID INT NULL,                                  -- Ảnh đại diện
    NgayTao DATETIME DEFAULT GETDATE(),               -- Ngày tạo
    TrangThai TINYINT DEFAULT 1                      -- Trạng thái
);
GO

CREATE TABLE Anh (
    AnhID        INT IDENTITY(1,1) PRIMARY KEY,       -- Khóa chính
    TenAnh       VARCHAR(255) NOT NULL,               -- Tên file ảnh
    DuongDan     VARCHAR(500) NOT NULL,               -- Đường dẫn file
    KichThuoc    INT,                                 -- Dung lượng ảnh (KB)
    DinhDang     VARCHAR(50),                         -- Định dạng
    NgayTaiLen   DATETIME DEFAULT GETDATE()           -- Ngày tải lên
);
GO

CREATE TABLE KinhNghiemLamViec (
    KinhNghiemID INT IDENTITY(1,1) PRIMARY KEY,         -- Khóa chính tự tăng
    NguoiUngTuyenID INT NOT NULL,                       -- Ứng viên sở hữu kinh nghiệm
    CongTyCu NVARCHAR(255) NOT NULL,                    -- Tên công ty cũ
    ChucVu NVARCHAR(150) NOT NULL,                      -- Vị trí đảm nhiệm
    NganhNghe NVARCHAR(150),                            -- Ngành nghề
    ChuyenNganh NVARCHAR(150),                          -- Chuyên ngành (nếu có)
    MoTaCongViec NVARCHAR(MAX),                         -- Mô tả chi tiết công việc
    NgayBatDau DATE NOT NULL,                           -- Ngày bắt đầu làm
    NgayKetThuc DATE,                                   -- Ngày kết thúc (NULL nếu vẫn làm)
    MucLuong DECIMAL(18,2),                             -- Mức lương (tuỳ chọn)
    DonViLuong NVARCHAR(20) DEFAULT N'VNĐ',             -- Đơn vị tiền
    ThanhTuu NVARCHAR(500),                             -- Thành tích, ghi chú
    NgayTao DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE ChungChi (
    ChungChiID INT IDENTITY(1,1) PRIMARY KEY,        -- Khóa chính
    TenChungChi NVARCHAR(100) NOT NULL,              -- Tên chứng chỉ
    CoQuanCap NVARCHAR(100),                         -- Đơn vị cấp
    NganhNghe NVARCHAR(100) NOT NULL,                -- Ngành nghề
    ChuyenNganh NVARCHAR(150),                       -- Chuyên ngành
    MoTaVeChungChi NVARCHAR(255)                     -- Mô tả ngắn gọn
);
GO

CREATE TABLE ChiTietChungChi (
    ChiTietChungChiID INT IDENTITY(1,1) PRIMARY KEY, -- Khóa chính
    NguoiUngTuyenID INT NOT NULL,                    -- FK đến ứng viên
    ChungChiID INT NOT NULL,                         -- FK đến chứng chỉ
    NgayCap DATE,                                    -- Ngày cấp
    NgayHetHan DATE,                                 -- Hết hạn
    TepTinDuongDan VARCHAR(500),                     -- Đường dẫn file
    DiemSo NVARCHAR(50),                             -- Điểm số
    GhiChu NVARCHAR(255),                            -- Ghi chú
    NgayTaiLen DATETIME DEFAULT GETDATE()             -- Ngày tải lên
);
GO

CREATE TABLE LichSuUngTuyen (
    LichSuUngTuyenID INT IDENTITY(1,1) PRIMARY KEY,       -- Khóa chính
    NguoiUngTuyenID INT NOT NULL,                         -- Liên kết đến người ứng tuyển
    TinTuyenDungID INT NOT NULL,                          -- Liên kết đến tin tuyển dụng
    NgayUngTuyen DATETIME DEFAULT GETDATE(),              -- Ngày nộp hồ sơ
    TrangThai NVARCHAR(50) DEFAULT N'Đang xét duyệt',     -- Trạng thái: Đang xét, Đã phỏng vấn, Trúng tuyển, Từ chối...
    TepCV NVARCHAR(500),                                  -- Đường dẫn file CV tải lên
    TepChungChi NVARCHAR(500),                            -- File chứng chỉ kèm theo (nếu có)
    GhiChu NVARCHAR(500),                                 -- Ghi chú thêm của ứng viên hoặc hệ thống
    NgayCapNhat DATETIME DEFAULT GETDATE()                 -- Thời gian cập nhật trạng thái
);
GO

CREATE TABLE LichSuGiaoDich (
    GiaoDichID INT IDENTITY(1,1) PRIMARY KEY,           -- Mã giao dịch tự tăng
    MaGiaoDich NVARCHAR(20) UNIQUE,                     -- Mã giao dịch hiển thị (VD: GD0001)
    TaiKhoanID INT NOT NULL,                            -- Tài khoản thực hiện giao dịch
    LoaiGiaoDich NVARCHAR(100) NOT NULL,                -- Nạp tiền / Thanh toán / Mua gói / Hoàn tiền...
    SoTien DECIMAL(18,2) NOT NULL,                      -- Số tiền giao dịch
    PhuongThuc NVARCHAR(50) NOT NULL,                   -- Phương thức (Chuyển khoản, Momo, Thẻ, v.v.)
    TrangThai NVARCHAR(50) DEFAULT N'Đang xử lý',       -- Trạng thái: Đang xử lý / Hoàn tất / Thất bại
    NgayGiaoDich DATETIME DEFAULT GETDATE(),            -- Ngày giao dịch
    MoTa NVARCHAR(500),                                 -- Ghi chú, mô tả thêm
    NgayCapNhat DATETIME DEFAULT GETDATE()              -- Cập nhật lần cuối
);
GO

CREATE TABLE TinTuyenDung (
    TinID INT IDENTITY(1,1) PRIMARY KEY,                  -- Khóa chính tự tăng
    MaTin NVARCHAR(20) UNIQUE,                            -- Mã tin (TTD001, TTD002,...)
    NhaTuyenDungID INT NOT NULL,                          -- Người đăng tin (liên kết đến NhaTuyenDung)
    CongTyID INT,                                         -- Công ty đăng tin (nếu có)
    TieuDe NVARCHAR(200) NOT NULL,                        -- Tiêu đề tin tuyển dụng
    MoTa NVARCHAR(MAX),                                   -- Mô tả công việc
    YeuCau NVARCHAR(MAX),                                 -- Yêu cầu công việc
    MucLuongTu DECIMAL(18,2),                             -- Mức lương tối thiểu
    MucLuongDen DECIMAL(18,2),                            -- Mức lương tối đa
    DonViLuong NVARCHAR(50) DEFAULT N'VNĐ',               -- Đơn vị lương (VNĐ, USD,…)
    DiaDiem NVARCHAR(255),                                -- Địa điểm làm việc
    HinhThuc NVARCHAR(100),                               -- Toàn thời gian / Bán thời gian / Remote
    HanNopHoSo DATE,                                      -- Hạn nộp hồ sơ
    TrangThai NVARCHAR(50) DEFAULT N'Đang hiển thị',      -- Trạng thái tin
    NgayDang DATETIME DEFAULT GETDATE(),                  -- Ngày đăng
    NgayCapNhat DATETIME DEFAULT GETDATE()                -- Lần cập nhật gần nhất
);
GO

CREATE TABLE ChiTietTinTuyenDung (
    ChiTietID INT IDENTITY(1,1) PRIMARY KEY,              -- Mã chi tiết
    TinID INT NOT NULL,                                   -- Liên kết đến TinTuyenDung
    NganhNghe NVARCHAR(150) NOT NULL,                     -- Ngành nghề tuyển dụng
    ChuyenNganh NVARCHAR(150),                            -- Chuyên ngành cụ thể
    SoNamKinhNghiem INT DEFAULT 0,                        -- Số năm kinh nghiệm yêu cầu
    BangCap NVARCHAR(100),                                -- Trình độ học vấn
    KyNang NVARCHAR(MAX),                                 -- Kỹ năng cần thiết
    SoLuong INT DEFAULT 1,                                -- Số lượng cần tuyển
    GioiTinh NVARCHAR(10) CHECK (GioiTinh IN (N'Nam', N'Nữ', N'Không yêu cầu')), -- Giới tính yêu cầu
    DoTuoiTu INT,                                         -- Độ tuổi tối thiểu
    DoTuoiDen INT,                                        -- Độ tuổi tối đa
    TrangThai NVARCHAR(50) DEFAULT N'Đang hiển thị'
);
GO
/* ===== TAIKHOAN → ANH (optional avatar) ===== */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TaiKhoan_Anh')
ALTER TABLE dbo.TaiKhoan
    WITH CHECK ADD CONSTRAINT FK_TaiKhoan_Anh
    FOREIGN KEY (AnhID) REFERENCES dbo.Anh(AnhID);
GO

/* ===== ADMIN → TAIKHOAN (1–1) ===== */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Admin_TaiKhoan')
ALTER TABLE dbo.Admin
    WITH CHECK ADD CONSTRAINT FK_Admin_TaiKhoan
    FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID);
GO

/* ===== CONGTY → TAIKHOAN (1–1) ===== */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CongTy_TaiKhoan')
ALTER TABLE dbo.CongTy
    WITH CHECK ADD CONSTRAINT FK_CongTy_TaiKhoan
    FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID);
GO

/* ===== NHATUYENDUNG → TAIKHOAN (1–1) ===== */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NhaTuyenDung_TaiKhoan')
ALTER TABLE dbo.NhaTuyenDung
    WITH CHECK ADD CONSTRAINT FK_NhaTuyenDung_TaiKhoan
    FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID);
GO

/* ===== NHATUYENDUNG → CONGTY (N–1) ===== */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NhaTuyenDung_CongTy')
ALTER TABLE dbo.NhaTuyenDung
    WITH CHECK ADD CONSTRAINT FK_NhaTuyenDung_CongTy
    FOREIGN KEY (CongTyID) REFERENCES dbo.CongTy(CongTyID);
GO

/* ===== NGUOIUNGTUYEN → TAIKHOAN (N–1, thực tế 1–1 vì UNIQUE) ===== */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NguoiUngTuyen_TaiKhoan')
ALTER TABLE dbo.NguoiUngTuyen
    WITH CHECK ADD CONSTRAINT FK_NguoiUngTuyen_TaiKhoan
    FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID);
GO

/* ===== NGUOIUNGTUYEN → ANH (optional avatar) ===== */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NguoiUngTuyen_Anh')
ALTER TABLE dbo.NguoiUngTuyen
    WITH CHECK ADD CONSTRAINT FK_NguoiUngTuyen_Anh
    FOREIGN KEY (AnhID) REFERENCES dbo.Anh(AnhID);
GO

CREATE INDEX IX_Admin_TaiKhoanID           ON dbo.Admin(TaiKhoanID);
GO
CREATE INDEX IX_CongTy_TaiKhoanID          ON dbo.CongTy(TaiKhoanID);
GO
CREATE INDEX IX_NhaTuyenDung_TaiKhoanID    ON dbo.NhaTuyenDung(TaiKhoanID);
GO
CREATE INDEX IX_NhaTuyenDung_CongTyID      ON dbo.NhaTuyenDung(CongTyID);
GO
CREATE INDEX IX_NguoiUngTuyen_TaiKhoanID   ON dbo.NguoiUngTuyen(TaiKhoanID);
GO
CREATE INDEX IX_NguoiUngTuyen_AnhID        ON dbo.NguoiUngTuyen(AnhID);
GO
CREATE INDEX IX_TaiKhoan_AnhID             ON dbo.TaiKhoan(AnhID);
GO
