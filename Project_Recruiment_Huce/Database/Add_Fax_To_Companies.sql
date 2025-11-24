/*
  Script: Add_Fax_To_Companies.sql
  Mô tả: Thêm cột `Fax` (NVARCHAR(20) NULL) vào bảng `Companies` nếu cột chưa tồn tại.
  Sử dụng trên SQL Server.
*/

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'Fax' AND Object_ID = Object_ID(N'dbo.Companies')
)
BEGIN
    ALTER TABLE dbo.Companies
    ADD Fax NVARCHAR(20) NULL;
END
GO

-- Ghi chú: Sau khi chạy script này, bạn có thể cần cập nhật lại DBML hoặc regenerate model nếu đang dùng LINQ-to-SQL.
