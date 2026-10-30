# Chạy trên Windows với SQL Server và PowerShell

## Chuẩn bị

- Cài **.NET 8 SDK**, **SQL Server 2022** (Developer hoặc Express), **SQL Server Management Studio (SSMS)** và **Python 3**.
- SSMS là công cụ quản lý; cần có SQL Server Database Engine đang chạy.
- Tải ZIP nhánh `codex/complete-backend-20261030`, giải nén và mở PowerShell tại thư mục chứa `RepairSystem.sln`.

Kiểm tra:

```powershell
dotnet --list-sdks
py --version
```

Danh sách SDK cần có một bản `8.0.xxx`. Nếu máy không nhận lệnh `py` nhưng nhận `python`, thay `py` bằng `python` ở bước dưới.

## Tạo database mới

Tạo file schema trống (không có tài khoản hoặc dữ liệu minh họa):

```powershell
py scripts/init-database.py --output .local/schema.sql
```

Lệnh này không dùng Docker và không kết nối database. Nó bỏ đường dẫn file Windows trong dump gốc, giữ các bảng, view, index và stored procedure. Nó từ chối ghi đè nếu file đầu ra đã tồn tại.

Mở SSMS, kết nối bằng Windows Authentication. Ghi lại đúng **Server name** bạn dùng, chẳng hạn `localhost`, `.\SQLEXPRESS` hoặc tên máy/instance khác. Tạo database mới:

```sql
CREATE DATABASE [QLSCTBDT];
```

Nếu đã có database cùng tên chứa dữ liệu cần giữ, chọn tên mới: dùng `--database QLSCTBDT_Local` khi xuất schema, tạo database tên đó, rồi dùng đúng tên trong connection string. Không nhập lại schema vào database đang có dữ liệu.

Trong SSMS chọn **File → Open → File**, mở `.local/schema.sql` trong thư mục dự án, nhấn **Execute/F5** để chạy toàn bộ file. Chỉ tiếp tục khi không có lỗi và các bảng đã xuất hiện trong database.

## Cấu hình và chạy API

Trong **cùng cửa sổ PowerShell** tại thư mục dự án, chạy:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = 'http://localhost:5107'
$env:ConnectionStrings__DefaultConnection = 'Server=localhost;Database=QLSCTBDT;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True'

$bytes = New-Object byte[] 48
$rng = [Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
$rng.Dispose()
$env:JwtSettings__SecretKey = [Convert]::ToBase64String($bytes)

$password = Read-Host 'Dat mat khau admin moi (12-100 ky tu)' -AsSecureString
$env:Bootstrap__AdminPassword = [Net.NetworkCredential]::new('', $password).Password

dotnet restore RepairSystem.sln
dotnet run --project RepairSystem/RepairSystem.csproj --no-launch-profile
```

Thay `Server=localhost` bằng đúng Server name trong SSMS. Ví dụ SQL Express thường là `Server=.\SQLEXPRESS`. `Trusted_Connection=True` dùng tài khoản Windows đang chạy PowerShell; tài khoản đó cần có quyền trên database. Cấu hình tin cậy chứng chỉ tự ký trong ví dụ này dành cho phát triển cục bộ.

Mật khẩu admin không hiển thị khi nhập. Giữ mật khẩu để đăng nhập; bootstrap chỉ tạo tài khoản `admin` khi chưa tồn tại và không đặt lại mật khẩu cũ. Biến môi trường chỉ tồn tại trong cửa sổ PowerShell hiện tại; file `.env` không được ASP.NET tự nạp.

## Dùng Swagger

Khi terminal hiển thị `Now listening on: http://localhost:5107`, mở địa chỉ `http://localhost:5107/swagger` trong trình duyệt trên máy của bạn.

1. Mở `POST /api/auth/login` → **Try it out**.
2. Nhập `username` là `admin`, `password` là mật khẩu bạn vừa đặt, rồi **Execute**.
3. Sao chép `data.token.accessToken` trong kết quả.
4. Bấm **Authorize**, dán access token (không thêm chữ `Bearer`) để dùng các API quản trị.
5. Tạo nhân viên `LE_TAN`, `THU_KHO`, `KY_THUAT_VIEN` qua `/api/nhan-vien`. Các thao tác tiếp nhận, nhập/xuất kho và lập hóa đơn cần đăng nhập bằng nhân viên đúng chức vụ như mô tả trong README.

Đây là backend API; Swagger giúp thử chức năng trước khi kết nối frontend. Dừng API bằng **Ctrl+C**. Nếu còn giữ cửa sổ PowerShell, chạy lại lệnh `dotnet run` để khởi động lại. Trên cùng database, dùng lại mật khẩu admin đã tạo.

## Nếu gặp lỗi

- Không nhận `dotnet`: cài .NET SDK 8 rồi mở lại terminal.
- Không kết nối database: kiểm tra dịch vụ SQL Server, đúng instance/Server name, tên database và quyền Windows Authentication.
- `/health/ready` trả 503: kiểm tra lỗi nhập schema và connection string; tiến trình API đang chạy chưa đảm bảo database sẵn sàng.
- Admin đăng nhập không được: xác nhận đã dùng schema trống và đã cung cấp mật khẩu bootstrap **trước** lần chạy đầu tiên. Dump dữ liệu mẫu không cung cấp mật khẩu admin dùng chung.
- Cổng 5107 bận: đổi `ASPNETCORE_URLS` sang cổng trống và mở Swagger tại cổng mới.
