# Quản lý ngân quỹ và chi tiêu cá nhân

Backend ASP.NET Core .NET 8, truy cập SQL Server bằng Dapper; frontend HTML, CSS, JavaScript và AngularJS 1.8.2. Ocelot là API Gateway và phục vụ frontend.

## Cấu trúc

| Thư mục | Vai trò |
| --- | --- |
| `QuanLyNganQuyCaNhan.API.Gateway` | Ocelot và frontend trong `wwwroot/QuanLyNganQuyCaNhan` |
| `QuanLyNganQuyCaNhan` | API dùng chung: đăng nhập, refresh token, đăng xuất, 2FA và khôi phục mật khẩu |
| `QuanLyNganQuyCaNhan.API.User` | Đăng ký, ví, danh mục, giao dịch, ngân sách, nhắc nhở, thống kê, nhập/xuất và báo cáo |
| `QuanLyNganQuyCaNhan.API.Admin` | Quản lý người dùng, danh mục hệ thống, Audit Log và Health Check |
| `QuanLyNganQuyCaNhan.API.Shared` | Middleware Audit, Health Check và nhận IP từ Gateway |
| `QuanLyNganQuyCaNhan.BLL` | Service và quy tắc nghiệp vụ |
| `QuanLyNganQuyCaNhan.DAL` | Model, kết nối và Repository dùng Dapper |
| `database` | Script khởi tạo và các migration SQL |

Luồng xử lý: frontend → Gateway → Controller → Service → Repository/Dapper → SQL Server.

## Thiết lập trên máy mới

Yêu cầu: .NET SDK 8 và SQL Server. Có thể mở solution bằng Visual Studio 2022.

### 1. Khởi tạo database

Với database mới, chạy `database/database QL Ngân Quỹ.sql` trong SQL Server Management Studio. Script chứa cấu trúc bảng và vai trò Admin/User, không chứa tài khoản hay dữ liệu tài chính cá nhân.

Với database đã có dữ liệu, chọn migration cần thiết trong `database/migrations` sau khi đọc nội dung từng script. Không chạy lại script khởi tạo hoặc tự động chạy toàn bộ migration lịch sử.

### 2. Tạo cấu hình riêng

Chạy PowerShell tại thư mục chứa solution. Các lệnh dưới đây tạo file còn thiếu và giữ nguyên cấu hình đã có:

```powershell
$cacProject = 'QuanLyNganQuyCaNhan', 'QuanLyNganQuyCaNhan.API.User', 'QuanLyNganQuyCaNhan.API.Admin', 'QuanLyNganQuyCaNhan.API.Gateway'
foreach ($project in $cacProject) {
    $cauHinh = Join-Path $project 'appsettings.json'
    if (-not (Test-Path -LiteralPath $cauHinh)) {
        Copy-Item -LiteralPath (Join-Path $project 'appsettings.example.json') -Destination $cauHinh
    }
}
```

Trong `appsettings.json` của ba API backend:

- Thay `YOUR_SQL_SERVER` bằng SQL Server trên máy của bạn. Ví dụ: `localhost\\SQLEXPRESS` trong chuỗi JSON.
- Điền `Jwt:Key` bằng một khóa ngẫu nhiên riêng, tối thiểu 32 byte. Có thể tạo bằng đoạn PowerShell dưới đây, rồi dùng cùng khóa cho cả ba API.
- `Jwt:Issuer` và `Jwt:Audience` phải giống nhau giữa ba API.

```powershell
$cacByte = New-Object byte[] 32
$boSinh = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$boSinh.GetBytes($cacByte)
$boSinh.Dispose()
[Convert]::ToBase64String($cacByte)
```

Các khóa JWT trong file mẫu để rỗng và phải được cấu hình trước khi sử dụng. Không đưa khóa vừa tạo lên GitHub. Cấu hình thực tế và file Development được `.gitignore` giữ riêng; có thể dùng User Secrets hoặc biến môi trường thay thế.

### 3. Chạy hệ thống

```powershell
dotnet restore QuanLyNganQuyCaNhan.sln
dotnet build QuanLyNganQuyCaNhan.sln
dotnet dev-certs https --trust
```

Mở bốn terminal tại thư mục solution và chạy mỗi lệnh trong một terminal:

```powershell
dotnet run --project QuanLyNganQuyCaNhan --launch-profile https
dotnet run --project QuanLyNganQuyCaNhan.API.User --launch-profile https
dotnet run --project QuanLyNganQuyCaNhan.API.Admin --launch-profile https
dotnet run --project QuanLyNganQuyCaNhan.API.Gateway --launch-profile https
```

Trong Visual Studio, cũng có thể chọn bốn project trên làm Startup Projects với hành động Start và profile `https`.

| Dịch vụ | Địa chỉ HTTPS |
| --- | --- |
| Frontend/Gateway | `https://localhost:7240/` |
| Swagger API dùng chung | `https://localhost:7084/swagger` |
| Swagger API.User | `https://localhost:7182/swagger` |
| Swagger API.Admin | `https://localhost:7183/swagger` |

Khi đổi cổng backend, cập nhật `QuanLyNganQuyCaNhan.API.Gateway/QuanLyNganQuyCaNhan.Ocelot.json` cho khớp. Các địa chỉ trên dùng profile Project `https`.

Đăng ký tạo tài khoản User. Tài khoản Admin cần được phân quyền riêng trong môi trường của bạn; repository không cung cấp tài khoản hoặc mật khẩu Admin dùng chung.

## Các chức năng chính

- Đăng ký, JWT Access/Refresh Token và 2FA TOTP tùy chọn.
- Khôi phục mật khẩu bằng bộ mã dùng một lần; phải lưu mã khi được hiển thị. Cần OTP nếu tài khoản đã bật 2FA.
- CRUD ví, danh mục, giao dịch và ngân sách; cập nhật giao dịch và số dư ví trong cùng transaction.
- Cảnh báo ngân sách trên giao diện.
- Nhắc nhở theo lịch bằng BackgroundService trong API.User; thông báo trong ứng dụng.
- Nhập/xuất CSV, Excel; chống nhập trùng bằng mã yêu cầu.
- Tạo PDF tài chính qua hàng đợi lưu trong database; worker chạy trong API.User.
- Thống kê, quản trị người dùng, Audit Log và Health Check.

## File công khai và file giữ riêng

Đưa lên GitHub: mã nguồn, solution/project, frontend, cấu hình định tuyến Ocelot, cấu hình mẫu, SQL schema/migrations và font/thư viện kèm giấy phép.

Giữ riêng: `appsettings.json`, `appsettings.Development.json`, User Secrets, token/mật khẩu/mã khôi phục, file cấu hình IDE, `.vs`, `bin`, `obj`, `.codex-build`, bản sao lưu database và file xuất chứa dữ liệu cá nhân.

Bỏ một file khỏi phiên bản hiện tại không xóa nó khỏi các commit cũ. Nếu khóa hoặc thông tin đăng nhập đã được commit trước đây, cần thay bằng giá trị mới trong môi trường riêng.

## Trạng thái

Đã triển khai backend, frontend và kết nối qua Ocelot. Kiểm thử toàn bộ luồng thực tế trên trình duyệt qua Gateway và các đầu việc Sprint 7 vẫn cần hoàn thành.
