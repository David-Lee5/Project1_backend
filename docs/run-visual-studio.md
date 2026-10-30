# Chạy bằng Visual Studio 2022

Không cần PowerShell hoặc Python cho quy trình bên dưới.

## Phần mềm

- **Visual Studio 2022 từ 17.8 trở lên**, cài workload **ASP.NET and web development** và **.NET 8 SDK**.
- **SQL Server 2022** Developer hoặc Express, có Database Engine đang chạy.
- **SQL Server Management Studio (SSMS)** để tạo database.

## 1. Mở solution

Giải nén ZIP, trong Visual Studio chọn **Open a project or solution** và mở `RepairSystem.sln`.

Trong **Solution Explorer**, nhấp phải project **RepairSystem** → **Set as Startup Project**. Đây là project Web API; các project `.Data`, `.Model`, `.Service` là thư viện.

## 2. Chuẩn bị database bằng SSMS

Kết nối SQL Server bằng **Windows Authentication**. Ghi lại **Server name** đã kết nối, ví dụ `localhost`, `.\SQLEXPRESS` hoặc tên máy/instance khác.

Chọn **New Query**, chạy:

```sql
CREATE DATABASE [QLSCTBDT];
```

Tiếp theo chọn **File → Open → File**, mở `data/schema.sql` trong thư mục mã nguồn và nhấn **Execute/F5** để chạy toàn bộ file. Đây là schema trống đã được xuất sẵn: không có tài khoản mẫu, có đầy đủ bảng, view và ba stored procedure nghiệp vụ. Không chạy `data/data_v2.sql` cho quy trình này vì dump đó có đường dẫn máy cũ và dữ liệu minh họa.

Nếu đã có `QLSCTBDT` chứa dữ liệu cần giữ, không xóa hoặc nhập lại schema vào đó. Dùng tên database mới; thay `[QLSCTBDT]` trong một bản sao file schema bằng tên mới và cấu hình tên tương ứng trong connection string. File schema không thực hiện CREATE DATABASE, nên database phải được tạo trước.

## 3. Lưu cấu hình riêng bằng Manage User Secrets

Quay lại Visual Studio, nhấp phải project **RepairSystem** → **Manage User Secrets**. File `secrets.json` mở ra ở thư mục riêng của tài khoản Windows, ngoài repository.

Dán cấu trúc sau rồi **điền hai giá trị đang trống** trước khi lưu:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=QLSCTBDT;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": ""
  },
  "Bootstrap": {
    "AdminPassword": ""
  }
}
```

- `DefaultConnection`: thay `localhost` bằng đúng **Server name** trong SSMS. Nếu server là `.\SQLEXPRESS`, JSON phải viết `Server=.\\SQLEXPRESS;...` vì dấu gạch chéo ngược cần được escape. `Trusted_Connection=True` sử dụng tài khoản Windows chạy Visual Studio; tài khoản đó cần có quyền trên database. Tin cậy chứng chỉ tự ký trong ví dụ này dành cho phát triển cục bộ.
- `JwtSettings.SecretKey`: tự đặt một khóa bí mật riêng, tối thiểu **32 ký tự**. Không để trống, không chia sẻ và không đưa vào Git.
- `Bootstrap.AdminPassword`: đặt mật khẩu admin **12–100 ký tự**, ghi nhớ để đăng nhập. Chỉ tạo `admin` khi tài khoản chưa tồn tại, không reset admin cũ. Sau khi tạo thành công có thể bỏ mục Bootstrap khỏi User Secrets.

Không chép các giá trị này vào `appsettings.json` hoặc `launchSettings.json`. Nếu trước đó đã đặt biến môi trường cùng tên, chúng có độ ưu tiên cao hơn User Secrets; kiểm tra chúng nếu thay secrets.json mà cấu hình không đổi.

## 4. Build và nhấn F5

Nếu NuGet chưa tự restore, nhấp phải solution → **Restore NuGet Packages**. Chọn **Build → Build Solution**.

Ở thanh công cụ cạnh nút chạy màu xanh, chọn profile **http** của project **RepairSystem**. Profile này dùng Development và cổng 5107. Nhấn **F5** (hoặc **Ctrl+F5** để chạy không debug).

Trình duyệt sẽ mở Swagger ở `http://localhost:5107/swagger`. Nếu chưa tự mở, gõ địa chỉ đó vào trình duyệt trên máy bạn. Giữ Visual Studio và tiến trình đang chạy; dừng bằng **Shift+F5**.

## 5. Đăng nhập và dùng API

Trong Swagger mở **POST /api/auth/login** → **Try it out**. Điền `username` là `admin`, `password` là mật khẩu đã đặt trong User Secrets. Nhấn **Execute**.

Trong response, sao chép `data.token.accessToken`. Bấm **Authorize**, dán access token (không thêm `Bearer`) rồi thử các API. Tạo nhân viên theo vai trò qua `/api/nhan-vien`; lễ tân, thủ kho và kỹ thuật viên cần đăng nhập bằng tài khoản riêng cho các thao tác tương ứng.

Đây là backend API; Swagger là công cụ thử trước khi kết nối frontend.

## Lỗi thường gặp

- **Không có .NET 8 hoặc profile http:** cập nhật Visual Studio/workload, xác nhận project khởi động là `RepairSystem`.
- **Lỗi SecretKey:** kiểm tra User Secrets của đúng project, giá trị đã điền, profile Development và biến môi trường có ghi đè không.
- **Không kết nối database:** kiểm tra đúng instance SQL Server, tên database và quyền Windows Authentication. SSMS cài riêng không có nghĩa Database Engine đã chạy.
- **Database chưa sẵn sàng:** chạy đủ `data/schema.sql` trong database trống; `/health/ready` phải phản hồi HTTP 200.
- **Đăng nhập admin thất bại:** xác nhận đã cấu hình Bootstrap trước lần chạy đầu và không nhập tài khoản mẫu. Bootstrap không thay mật khẩu của tài khoản admin có sẵn.
- **Cổng 5107 đã được dùng:** dừng phiên chạy cũ hoặc đổi `applicationUrl` trong profile http ở Properties/launchSettings.json rồi mở Swagger tại cổng tương ứng.
