# RepairSystem Backend

Backend ASP.NET Core cho hệ thống quản lý sửa chữa thiết bị. Dự án dùng .NET 8 và SQL Server. Mỗi thành viên có thể tạo CSDL `QLSCTBDT_V3` riêng trên máy mình; không cần sao chép CSDL từ máy người khác.

## Chạy lần đầu trên Windows

1. Cài .NET SDK 8, SQL Server và SSMS.
2. Trong SSMS, chạy toàn bộ [database/TAO_MOI_QLSCTBDT_V3.sql](database/TAO_MOI_QLSCTBDT_V3.sql). Script tạo CSDL mới và dừng nếu tên CSDL đã tồn tại.
3. Cấu hình SQL Server instance. Mặc định của profile Development là `Server=.`. Nếu máy dùng SQL Server Express, trong **cùng cửa sổ PowerShell** chạy:

   ```powershell
   $env:ConnectionStrings__DefaultConnection = 'Server=.\SQLEXPRESS;Database=QLSCTBDT_V3;Trusted_Connection=True;TrustServerCertificate=True'
   ```

4. Từ thư mục gốc repo, chạy:

   ```powershell
   dotnet restore RepairSystem.sln
   dotnet build RepairSystem.sln --no-restore -m:1
   dotnet run --project RepairSystem/RepairSystem.csproj --launch-profile http
   ```

5. Mở `http://localhost:5107/swagger`. CSDL mới được tạo admin Development ở lần khởi động đầu: `admin` / `Admin@123`. Đổi mật khẩu này nếu dùng dữ liệu thật.

ASP.NET Core đọc `appsettings.json`, `appsettings.Development.json` và các biến môi trường. File `.env.example` chỉ là mẫu tên biến, ứng dụng không tự đọc `.env`. Ngoài Development, cần đặt riêng `ConnectionStrings__DefaultConnection` và `JwtSettings__SecretKey` (ít nhất 32 byte); không đưa giá trị thật vào Git.

Xem [SETUP_DATABASE_BACKEND.md](SETUP_DATABASE_BACKEND.md) để biết chi tiết, lỗi thường gặp và các bước khởi tạo nghiệp vụ. FE hiện chuyển tiếp `/api` tới cổng `5107` khi chạy Vite.
