# Hướng dẫn cho đồng nghiệp: cài CSDL V3 và chạy backend

Tài liệu này áp dụng khi bạn **clone repo `Project1_backend` về máy Windows** và muốn bắt đầu với CSDL `QLSCTBDT_V3` rỗng. Không cần CSDL `QLSCTBDT` cũ và không chuyển dữ liệu cũ. Làm lần lượt các bước dưới đây từ thư mục gốc của repo (thư mục chứa `RepairSystem.sln`).

## 1. Chuẩn bị

- Cài **SQL Server** và **SQL Server Management Studio (SSMS)**. Ghi lại tên instance dùng để kết nối, ví dụ `.` (instance mặc định), `.\SQLEXPRESS` hoặc `(localdb)\MSSQLLocalDB`. Tài khoản Windows cần quyền tạo CSDL trong instance đó.
- Nên cài **.NET SDK 8** (SDK mới hơn cũng dùng được nếu có bộ build cho `net8.0`) và **ASP.NET Core Runtime 8**. Kiểm tra bằng `dotnet --list-sdks` và `dotnet --list-runtimes`; danh sách runtime cần có `Microsoft.AspNetCore.App 8.x` và `Microsoft.NETCore.App 8.x`. Dùng Visual Studio có workload **ASP.NET and web development** nếu muốn chạy từ IDE.
- Clone repo backend đầy đủ. Các file `RepairSystem.Model/ScaffoldV3` và `database/TAO_MOI_QLSCTBDT_V3.sql` phải có trong bản clone.

## 2. Tạo CSDL mới trong SSMS

1. Mở SSMS, kết nối đúng SQL Server instance của bạn bằng **Windows Authentication**.
2. Mở file [`database/TAO_MOI_QLSCTBDT_V3.sql`](database/TAO_MOI_QLSCTBDT_V3.sql) từ bản clone.
3. Bấm **Execute** để chạy **toàn bộ** file. Script tự chọn `master`, tạo `QLSCTBDT_V3`, tạo bảng, khóa, thủ tục và loại linh kiện mẫu.
4. Refresh **Databases**. Kiểm tra đã có `QLSCTBDT_V3` và các bảng như `dbo.tai_khoan`, `dbo.linh_kien`, `dbo.phieu_sua_chua`.
5. Chọn database `QLSCTBDT_V3`, mở và chạy
   [`database/BO_SUNG_QUY_TAC_KIEM_KE_20261108.sql`](database/BO_SUNG_QUY_TAC_KIEM_KE_20261108.sql).
   Bản bổ sung này tạo quy tắc vận hành, xác nhận tiếp nhận, kiểm kê và cách ly.
   Với CSDL V3 đang sử dụng, sao lưu rồi chạy riêng file này; không chạy lại file tạo mới.
6. Trên cùng CSDL, chạy tiếp
   [`database/BO_SUNG_CONG_VIEC_THANH_LY_TAI_CHINH_20261108.sql`](database/BO_SUNG_CONG_VIEC_THANH_LY_TAI_CHINH_20261108.sql).
   Script tạo bảng công việc sửa chữa, phân tích bảo hành, hồ sơ thanh lý và chứng từ tài chính.
   Với CSDL đang dùng, dừng backend và sao lưu trước khi chạy. Chỉ chạy một lần.

Có thể kiểm tra trong cửa sổ query mới:

```sql
USE QLSCTBDT_V3;
SELECT COUNT(*) AS so_loai FROM dbo.loai_linh_kien;
SELECT COUNT(*) AS so_tai_khoan FROM dbo.tai_khoan;
SELECT version FROM dbo.v3_schema_migration WHERE version = 'V3_20261108_RULES';
SELECT version FROM dbo.v3_schema_migration WHERE version = 'V3_20261108_WORKFLOW';
```

Sau khi tạo mới, `so_loai` là `3` (RAM, quạt laptop, màn hình); `so_tai_khoan` là `0` cho tới khi backend chạy lần đầu. Script **dừng nếu CSDL `QLSCTBDT_V3` đã tồn tại**, không ghi đè hay xóa dữ liệu. Không chạy file nâng cấp CSDL cũ sau file tạo mới này. Nếu CSDL đã có, dùng lại hoặc nhờ người phụ trách kiểm tra; không tự xóa CSDL để thử lại.

Nếu dùng CSDL đã tạo bằng script cũ với các bảng hậu tố `_v3`, sao lưu CSDL rồi chạy [database/DOI_TEN_BANG_BO_V3_20261111.sql](database/DOI_TEN_BANG_BO_V3_20261111.sql) trên đúng CSDL sau khi áp dụng các migration còn thiếu. Script đổi tên bảng tại chỗ và cập nhật các procedure, view, trigger liên quan; không tạo lại CSDL và không xóa dữ liệu. CSDL tạo mới bằng file hiện tại đã có tên bảng mới nên không cần bước này.

## 3. Cấu hình kết nối backend

File `RepairSystem/appsettings.Development.json` đã trỏ tới:

```text
Server=.;Database=QLSCTBDT_V3;Trusted_Connection=True;TrustServerCertificate=True
```

Nếu SSMS của bạn kết nối bằng instance `.`, không cần sửa. Nếu dùng instance khác, đặt chuỗi kết nối cho **cửa sổ PowerShell đang chạy API**. Ví dụ với SQL Server Express:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=.\SQLEXPRESS;Database=QLSCTBDT_V3;Trusted_Connection=True;TrustServerCertificate=True'
```

Thay phần sau `Server=` bằng **đúng tên instance đã dùng trong SSMS**. Biến môi trường này ghi đè cấu hình của backend và không cần commit thay đổi máy cá nhân vào Git. Nếu chạy bằng nút Start của Visual Studio thay vì PowerShell, đặt biến môi trường cho tiến trình Visual Studio trước khi mở IDE; tránh commit chuỗi kết nối riêng của máy vào `appsettings.Development.json`.

File `.env.example` chỉ liệt kê tên biến đúng của ASP.NET Core; backend **không tự nạp** file `.env`. `appsettings.json` không chứa chuỗi kết nối hay khóa ký JWT dùng cho triển khai. Profile Development có khóa JWT dành riêng cho chạy local. Khi chạy ngoài Development, đặt cả `ConnectionStrings__DefaultConnection` và `JwtSettings__SecretKey` (ít nhất 32 byte) qua biến môi trường hoặc nơi lưu bí mật của môi trường đó.

## 4. Restore, build và chạy API

Trong PowerShell tại thư mục gốc repo backend:

```powershell
dotnet restore RepairSystem.sln
dotnet build RepairSystem.sln --no-restore -m:1
dotnet run --project RepairSystem/RepairSystem.csproj --launch-profile http
```

Chờ dòng `Now listening on: http://localhost:5107`, rồi mở `http://localhost:5107/swagger`.

**Không cần chạy `dotnet ef database update` hoặc scaffold lại**: schema đã được tạo bằng SQL script; `RepairDbContext` và các entity đã có trong `RepairSystem.Model/ScaffoldV3`. Nếu cần scaffold lại sau khi thay schema, phối hợp với nhóm để tránh ghi đè mã sinh và dùng lệnh trong tài liệu CSDL của dự án.

## 5. Đăng nhập admin lần đầu

Khi chạy với profile `http`, môi trường là **Development**. Nếu `QLSCTBDT_V3` chưa có admin, backend tự tạo:

| Tên đăng nhập | Mật khẩu | Vai trò |
|---|---|---|
| `admin` | `Admin@123` | `ADMIN` |

Trong Swagger, gọi `POST /api/auth/login` với JSON:

```json
{"username":"admin","password":"Admin@123"}
```

Phản hồi thành công có `data.user.vaiTro = "ADMIN"`. Mật khẩu trong CSDL được lưu bằng hash BCrypt. Backend **chỉ tạo khi chưa có admin**; nếu một admin đã tồn tại, lần chạy sau không đặt lại mật khẩu của người đó. Mật khẩu dùng chung trong `appsettings.Development.json` chỉ dành cho môi trường phát triển; không dùng để triển khai hệ thống thật.

## 6. Sau khi đăng nhập

Để có dữ liệu nghiệp vụ đầu tiên, admin tạo nhân viên lễ tân, thủ kho và kỹ thuật viên; sau đó tạo kho, nhà cung cấp, dịch vụ và mẫu linh kiện. Thủ kho mới có thể lập phiếu nhập. Với linh kiện theo dõi serial, nhập đủ serial cho từng đơn vị trước khi xác nhận phiếu.

## Lỗi thường gặp

| Hiện tượng | Kiểm tra |
|---|---|
| `Cannot open database QLSCTBDT_V3` | Đã chạy đúng script trên đúng SQL Server instance chưa? Chuỗi kết nối có trùng tên instance trong SSMS không? |
| `Login failed for user ...` khi backend nối SQL | Backend dùng **Windows Authentication**; tài khoản Windows chạy backend cần quyền truy cập CSDL. |
| SQL báo CSDL V3 đã tồn tại | Script bảo vệ dữ liệu và dừng có chủ đích. Không chạy lại lên CSDL đó. |
| Swagger chạy nhưng không có admin | Kiểm tra profile `http` đang dùng `ASPNETCORE_ENVIRONMENT=Development`, chuỗi kết nối trỏ đúng V3 và xem log khởi động. |
| `admin / Admin@123` không đăng nhập được | Kiểm tra CSDL có admin từ trước không; seed không ghi đè mật khẩu đã có. |
| EF scaffold báo lỗi khóa ngoại `thong_so_linh_kien` | Bạn có thể đang dùng script V3 cũ. File `database/TAO_MOI_QLSCTBDT_V3.sql` trong repo này đã có bản sửa; CSDL mới không cần chạy script sửa riêng. |
| `dotnet restore` báo `NU1301` với `api.nuget.org` | Máy chưa truy cập được nguồn NuGet. Kiểm tra Internet, proxy hoặc mạng công ty rồi chạy lại `dotnet restore`; lần clone đầu cần tải các gói chưa có trong bộ nhớ đệm. |

Nếu nhiều máy cùng truy cập **một CSDL**, dữ liệu mật khẩu thiết bị được mã hóa bằng ASP.NET Core Data Protection cần dùng chung key ring được lưu an toàn ngoài Git. Cấu hình hiện tại phù hợp với mỗi thành viên dùng một CSDL local riêng.

Trước khi chia sẻ bản clone cho nhóm, người phụ trách repo cần **commit và push** file SQL, mã backend V3 và tài liệu này. Các file chưa commit trên máy người viết sẽ không xuất hiện khi đồng nghiệp clone. Kiểm tra `git status` trước khi commit; không đưa `.vs`, `*.csproj.user`, file `.env` hay khóa riêng vào Git.
