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

Nếu dùng CSDL V3 đã tạo trước ngày 06/11/2026, chạy migration
[`database_v3/BO_SUNG_API_20261106.sql`](../database_v3/BO_SUNG_API_20261106.sql)
trên đúng CSDL trước khi chạy backend mới. Danh sách endpoint và mẫu payload nằm tại
[API_BO_SUNG_20261106.md](API_BO_SUNG_20261106.md).
Nếu CSDL V3 đã có migration trên, chạy tiếp
[`database_v3/BO_SUNG_QUY_TRINH_KTV_20261107.sql`](../database_v3/BO_SUNG_QUY_TRINH_KTV_20261107.sql)
để bật bước KTV xác nhận nhận máy và ghi kết quả kiểm tra. File tạo mới V3 đã gồm cả hai phần bổ sung.
Xem [QUY_TRINH_KTV_20261107.md](QUY_TRINH_KTV_20261107.md) để biết thứ tự thao tác.
Backend kiểm tra migration `V3_20261106_API`, `V3_20261107_KTV` và các bảng/cột bắt buộc khi khởi động.
CSDL đã tạo bằng script cũ còn bảng hậu tố `_v3`: làm theo [hướng dẫn đồng bộ tên bảng cho đồng nghiệp](HUONG_DAN_DONG_BO_TEN_BANG_V3.md) sau các migration còn thiếu và trước khi chạy backend mới. Script đổi tên bảng ngay trên CSDL đang dùng; tên CSDL `QLSCTBDT_V3` vẫn giữ nguyên. Bản tạo mới hiện tại đã dùng tên bảng không có `_v3`.
Các API chỉnh sửa danh mục V3, đặt lại mật khẩu qua Admin và kiểm tra sức chứa lịch hẹn được mô tả tại [API_V3_COMPLETION.md](API_V3_COMPLETION.md).
Để nạp dữ liệu thử, chạy [database/DU_LIEU_MAU_V3.sql](database/DU_LIEU_MAU_V3.sql) bằng SSMS trên CSDL `QLSCTBDT_V3` sau khi áp dụng đủ migration. Script tạo tài khoản `demo_letan`, `demo_kho`, `demo_ktv`, `demo_khach` (mật khẩu chung `Demo@123456`) và dữ liệu cho các luồng tiếp nhận, báo giá, kho, thanh toán, lịch hẹn, bảo hành. Chạy lại sẽ không tạo trùng.
Nếu kết nối nhầm CSDL hoặc thiếu migration, ứng dụng dừng với thông báo chỉ rõ CSDL,
SQL Server và thành phần còn thiếu thay vì phục vụ API rồi trả lỗi SQL 500.
