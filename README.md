# RepairSystem Backend

API quản lý sửa chữa thiết bị bằng ASP.NET Core 8, Entity Framework Core 8 và SQL Server 2022. Solution gồm API (`RepairSystem`), nghiệp vụ/DTO (`RepairSystem.Service`), truy cập dữ liệu (`RepairSystem.Data`), model/database mapping (`RepairSystem.Model`) và kiểm thử (`RepairSystem.Tests`).

## Cài đặt và chạy

Yêu cầu .NET SDK 8 và SQL Server 2022 có database `QLSCTBDT`. Trong môi trường đám mây đã thiết lập, SDK nằm tại `/workspace/.dotnet`; cần thêm đường dẫn này vào `PATH` nếu shell chưa nhận `dotnet`.

```sh
cd /workspace/Project1_backend
dotnet restore RepairSystem.sln
dotnet build RepairSystem.sln --no-restore
dotnet test RepairSystem.sln --no-build
```

Cấu hình qua biến môi trường do môi trường chạy cung cấp, không ghi mật khẩu hoặc khóa vào repository:

| Biến | Ý nghĩa |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | Connection string SQL Server trỏ tới `QLSCTBDT`; dùng tài khoản có quyền truy cập schema và thực thi các stored procedure |
| `JwtSettings__SecretKey` | Khóa ký JWT riêng của môi trường, tối thiểu **32 byte UTF-8** |
| `JwtSettings__Issuer` | Issuer; mặc định `RepairSystemAPI` |
| `JwtSettings__Audience` | Audience; mặc định `RepairSystemUsers` |
| `JwtSettings__ExpirationMinutes` | Số phút hiệu lực access token, số nguyên dương; mặc định 15 |
| `ASPNETCORE_ENVIRONMENT` | `Development` để bật Swagger |
| `ASPNETCORE_URLS` | Địa chỉ lắng nghe; ví dụ `http://127.0.0.1:5107` khi phát triển cục bộ |
| `Cors__AllowedOrigins__0` | Origin frontend được phép, gồm scheme/host/port; thêm `__1`, `__2` cho các origin khác |
| `Bootstrap__AdminPassword` | Tùy chọn: mật khẩu **12–100 ký tự**, chỉ dùng tạo `admin` khi tài khoản này chưa tồn tại |

`ConnectionStrings:DefaultConnection` và `JwtSettings:SecretKey` trong appsettings được để trống có chủ đích. Ứng dụng dùng cấu hình ASP.NET với dấu `__`; file `.env.example` chỉ mô tả tên biến; file `.env` không được ứng dụng tự động nạp. Không đưa file `.env` chứa bí mật lên Git.

Sau khi cung cấp cấu hình:

```sh
dotnet run --project RepairSystem/RepairSystem.csproj --no-launch-profile
```

Swagger nằm ở `/swagger` trong Development; OpenAPI JSON ở `/swagger/v1/swagger.json`. Dùng nút Authorize để nhập access token. Khi tích hợp frontend, gửi header `Authorization: Bearer <accessToken>`. Production sử dụng HTTPS; cấu hình proxy/TLS phù hợp trước khi triển khai.

## Chuẩn bị database

`data/data_v2.sql` là bản xuất **UTF-16**, chứa schema, dữ liệu mẫu, view và stored procedure. Phần `CREATE DATABASE` ở đầu dùng đường dẫn file tuyệt đối của Windows. **Không chạy nguyên bản trên Linux.**

Cách khởi tạo database trống trên Linux với Docker (cần Python 3 và Docker Compose):

```sh
# Cung cấp MSSQL_SA_PASSWORD an toàn qua môi trường hoặc .env cục bộ đã được ignore.
docker compose up -d --wait
# SQLCMDPASSWORD dùng cùng mật khẩu SQL Server, không truyền mật khẩu bằng tham số dòng lệnh.
export SQLCMDPASSWORD="$MSSQL_SA_PASSWORD"
python3 scripts/init-database.py
```

Script mặc định chỉ nhập schema, giữ nguyên ba stored procedure và không nhập tài khoản mẫu. Cấu hình `Bootstrap__AdminPassword` trước lần chạy API đầu tiên để tạo admin; bỏ biến bootstrap sau khi tạo. Script từ chối database không trống và không xóa dữ liệu. `--database` chọn database mới; `--container` chọn container cục bộ; `--sample-data` nhập thêm dữ liệu minh họa và cần lưu ý hash mật khẩu mẫu phía dưới. SQL Server Developer chỉ dùng phát triển/kiểm thử.

Nếu nhập thủ công vào một database phát triển mới trên Linux:

1. Tạo `QLSCTBDT` bằng đường dẫn mặc định của SQL Server trên máy đích.
2. Đọc dump bằng UTF-16 và tạo bản nhập tạm từ lần xuất hiện đầu tiên của `USE [QLSCTBDT]`, bỏ phần tạo database bằng đường dẫn Windows. Giữ nguyên file gốc trong repository.
3. Chạy bản nhập bằng công cụ hỗ trợ batch `GO`, chẳng hạn `sqlcmd -I`, và dừng ngay khi có lỗi (`-b`); `-I` bật QUOTED_IDENTIFIER cần cho computed/filtered index.
4. Xác minh các bảng, dữ liệu và ba thủ tục `usp_xac_nhan_nhap_kho`, `usp_xuat_kho`, `usp_lap_hoa_don` đã tồn tại trước khi dùng API nghiệp vụ.

Không nhập lại dump vào database đang có dữ liệu. Dự án dùng schema SQL có sẵn; `EnsureCreated` hoặc chỉ tạo bảng EF sẽ không tạo đủ stored procedure cho nhập kho, xuất kho và hóa đơn.

Dump có sẵn tài khoản `admin`, nên `Bootstrap__AdminPassword` không đổi mật khẩu tài khoản đã tồn tại. Một số hash mật khẩu dữ liệu mẫu có thể chỉ là placeholder, không phải hash BCrypt đăng nhập được. Không giả định mật khẩu mặc định; hãy dùng database phát triển được quản lý, đặt hash BCrypt hợp lệ theo quy trình quản trị riêng, hoặc đăng ký khách hàng mới qua API. Bootstrap không ghi mật khẩu ra log.

## Phân quyền

Vai trò được lưu trong cột `tai_khoan.vai_tro`: `ADMIN`, `LE_TAN`, `KY_THUAT_VIEN`, `THU_KHO`, `KHACH_HANG`. Nhân viên cần có hồ sơ đang hoạt động tương ứng; lễ tân, kỹ thuật viên và thủ kho còn có bảng hồ sơ chức vụ riêng.

- Khách vãng lai chỉ xem danh mục dịch vụ/linh kiện và gọi đăng ký, đăng nhập, refresh. Tồn kho không xuất hiện trong dữ liệu danh mục công khai.
- Khách hàng chỉ đọc/sửa hồ sơ và thiết bị của mình; chỉ xem phiếu sửa chữa và hóa đơn thuộc mình. Không thể tự gán tài khoản, vai trò hoặc chuyển thiết bị sang chủ khác bằng API hồ sơ.
- Nhân viên được đọc hồ sơ khách hàng/thiết bị; chỉ `ADMIN`/`LE_TAN` được sửa trên toàn bộ hồ sơ và thiết bị. Kỹ thuật viên/thủ kho không được tạo, sửa hoặc xóa thiết bị của khách hàng khác.
- Kỹ thuật viên chỉ xem và cập nhật nghiệp vụ trên phiếu được phân công. Lễ tân/quản trị viên tiếp nhận, phân công và điều phối phiếu.
- Thủ kho lập/xác nhận phiếu nhập, xuất linh kiện và đặt mức tồn tối thiểu. Các thao tác này lấy người thực hiện từ tài khoản đăng nhập.
- Lễ tân lập hóa đơn; lễ tân/quản trị viên thanh toán hoặc hủy hóa đơn chưa thanh toán. Tài khoản `ADMIN` không tự thay thế hồ sơ lễ tân/thủ kho trong các thao tác yêu cầu đúng chức vụ.
- Quản trị viên quản lý tài khoản, nhân viên, nhà cung cấp và kho. `ADMIN`/`THU_KHO` quản lý danh mục linh kiện; `ADMIN` quản lý danh mục dịch vụ.

Đăng nhập sai 5 lần sẽ khóa tạm 15 phút. Mật khẩu được băm bằng BCrypt; refresh token lưu dưới dạng hash, có hiệu lực 7 ngày và được xoay khi refresh. Đăng xuất/đổi mật khẩu/vô hiệu hóa tài khoản thu hồi refresh token. Middleware kiểm tra trạng thái tài khoản và vai trò hiện tại khi xác thực access token.

## Các API

Các đường dẫn dưới đây có tiền tố `/api`; `{id}` là ID `long`. Swagger là nguồn mô tả schema request/response chi tiết.

| Nhóm | Route và thao tác |
| --- | --- |
| Xác thực | `POST /auth/register`, `/auth/login`, `/auth/refresh-token`, `/auth/logout`, `/auth/change-password`; `GET /auth/me` |
| Tài khoản | `GET /users`, `/users/{id}`, `/users/by-username/{username}`; `PUT /users/{id}/status`; `DELETE /users/{id}` |
| Khách hàng | `GET/POST /khach-hang`; `GET/PUT/DELETE /khach-hang/{id}` |
| Thiết bị | `GET/POST /thiet-bi`; `GET/PUT/DELETE /thiet-bi/{id}` |
| Nhân viên | `GET/POST /nhan-vien`; `GET/PUT/DELETE /nhan-vien/{id}` |
| Dịch vụ | `GET/POST /dich-vu`; `GET/PUT/DELETE /dich-vu/{id}` |
| Linh kiện | `GET/POST /linh-kien`; `GET/PUT/DELETE /linh-kien/{id}`; `GET/POST /{id}/anh`, `PUT /{id}/anh/{imageId}/chinh`, `DELETE /{id}/anh/{imageId}` |
| Nhà cung cấp | `GET/POST /nha-cung-cap`; `GET/PUT/DELETE /nha-cung-cap/{id}` |
| Phiếu sửa chữa | `GET/POST /phieu-sua-chua`; `GET/PUT /phieu-sua-chua/{id}`; `PUT /{id}/phan-cong`, `/{id}/trang-thai`; `PUT/DELETE /{id}/dich-vu/{dichVuId}`; `POST /{id}/hinh-anh`; `DELETE /{id}/hinh-anh/{photoId}` |
| Kho | `GET/POST /kho`; `GET/PUT/DELETE /kho/{id}` |
| Tồn kho | `GET /ton-kho`, `/ton-kho/bien-dong`; `PUT /ton-kho/{khoId}/{linhKienId}/muc-ton-toi-thieu` |
| Phiếu nhập | `GET/POST /phieu-nhap`; `GET/PUT /phieu-nhap/{id}`; `POST /phieu-nhap/{id}/xac-nhan`, `/phieu-nhap/{id}/huy` |
| Xuất kho | `GET/POST /xuat-kho`; `GET /xuat-kho/{id}` |
| Hóa đơn | `GET/POST /hoa-don`; `GET /hoa-don/{id}`; `POST /hoa-don/{id}/thanh-toan`, `/hoa-don/{id}/huy` |

Trong hàng phiếu sửa chữa, các route `/{id}/...` tiếp tục sau `/phieu-sua-chua`. Ảnh được cung cấp bằng URL; API không nhận file upload trực tiếp hoặc yêu cầu Cloudinary cho quy trình hiện tại.

Khách hàng/thiết bị dùng `search`, `page`, `pageSize` (1–100); thiết bị có thêm `khachHangId`. Phiếu sửa chữa có bộ lọc theo trạng thái/tìm kiếm/phân công trong Swagger. Tồn kho và các chứng từ kho dùng `skip`, `take`; `lowOnly=true` lọc tồn thấp. Hóa đơn dùng `status`, `page`, `pageSize`. Kiểm tra quyền vẫn áp dụng sau khi lọc.

Các danh mục hỗ trợ xóa mềm. Khách hàng/thiết bị có phiếu chưa kết thúc không thể bị xóa; kho còn tồn hoặc chứng từ chưa xử lý và nhân viên còn công việc đang thực hiện cũng được bảo vệ. Xóa hồ sơ khách hàng vô hiệu hóa tài khoản liên kết và thu hồi refresh token.

## Quy trình sửa chữa

1. Đăng ký khách hàng hoặc để lễ tân tạo hồ sơ, sau đó tạo thiết bị thuộc khách hàng đó.
2. Lễ tân tiếp nhận thiết bị bằng `POST /api/phieu-sua-chua`; chỉ một phiếu chưa kết thúc được mở cho cùng thiết bị. Phân công kỹ thuật viên trước khi bắt đầu sửa.
3. Chuyển `TIEP_NHAN → DANG_SUA`. Thêm dịch vụ với số lượng/đơn giá thực tế. Khi thiếu linh kiện, chuyển `DANG_SUA → CHO_LINH_KIEN → DANG_SUA`.
4. Thủ kho tạo phiếu nhập nháp với các dòng linh kiện, xác nhận để cộng tồn; xuất linh kiện cho phiếu đang `DANG_SUA` hoặc `CHO_LINH_KIEN`. Giá xuất và bảo hành lấy từ dữ liệu linh kiện, không nhận tổng tiền tùy ý từ client.
5. Ghi kết quả sửa chữa và chuyển `DANG_SUA → HOAN_THANH`. Sau hoàn thành, các chi phí bị khóa. Lịch sử trạng thái, dịch vụ, linh kiện và ảnh nằm trong chi tiết phiếu.
6. Lễ tân lập hóa đơn từ phiếu hoàn thành. Database chốt tổng dịch vụ + linh kiện − giảm giá; giảm giá không vượt tổng chi phí. Thanh toán bằng `TIEN_MAT`, `CHUYEN_KHOAN`, `THE` hoặc `VI_DIEN_TU`.
7. Chỉ sau khi có hóa đơn `DA_THANH_TOAN`, chuyển phiếu `HOAN_THANH → DA_TRA` để bàn giao thiết bị.

Hủy phiếu cần lý do và tuân theo trạng thái/chi tiết phát sinh; phiếu đã hoàn thành hoặc đã trả không được mở lại bằng cập nhật trạng thái. Hóa đơn đã thanh toán không thể hủy bằng endpoint hủy hóa đơn. Phiếu nhập đã xác nhận không thể hủy như phiếu nháp.

Các stored procedure xử lý giao dịch và khóa dữ liệu ở SQL Server: xác nhận nhập chống cộng tồn lặp; xuất kho chống tồn âm và ghi sổ biến động; lập hóa đơn chống hóa đơn hiệu lực trùng trên cùng phiếu. Các kiểm thử dùng InMemory/SQLite không thay thế kiểm thử các thủ tục này trên SQL Server thật.

## Phản hồi và kiểm tra dịch vụ

Các API nghiệp vụ trả envelope JSON:

```json
{
  "success": false,
  "message": "Dữ liệu không hợp lệ.",
  "data": null,
  "errors": []
}
```

Mã thường gặp: `400` dữ liệu sai, `401` cần đăng nhập/token không hợp lệ, `403` thiếu quyền, `404` không tìm thấy hoặc ngoài phạm vi được đọc, `409` xung đột/trạng thái nghiệp vụ, `503` database chưa sẵn sàng. Lỗi ngoài dự kiến trả `500` với thông báo chung.

- `GET /health/live`: tiến trình API phản hồi, không kiểm tra database.
- `GET /health/ready`: truy vấn được bảng tài khoản, trả `200` hoặc `503`; cần kiểm tra riêng các luồng nghiệp vụ để xác nhận đủ schema/thủ tục.
- `GET /WeatherForecast`: endpoint mẫu, không xác nhận nghiệp vụ sửa chữa hoặc database.

`dotnet test` chạy các kiểm thử hồi quy của dự án; kiểm thử SQL Server và luồng HTTP thực tế cần database đã khởi tạo cùng tài khoản chức vụ phù hợp. Không dùng kết quả build hoặc health/live làm bằng chứng rằng toàn bộ quy trình kho/hóa đơn đã hoạt động.

## Kiểm thử tích hợp trên SQL Server

Sau khi API kết nối database cục bộ, cung cấp `SMOKE_ADMIN_USERNAME` và `SMOKE_ADMIN_PASSWORD` bằng môi trường (không ghi vào source), rồi chạy:

```sh
python3 scripts/integration-smoke.py
python3 scripts/concurrency-smoke.py
```

Các script chỉ chấp nhận địa chỉ HTTP loopback. Chúng tạo nhân viên, khách hàng và chứng từ riêng rồi để lại dữ liệu trong database phát triển để kiểm tra; không chạy vào database chứa dữ liệu thật. Script đầu kiểm tra trọn quy trình và các trường hợp từ chối; script sau kiểm tra hai request đồng thời cho nhập kho, xuất kho, lập hóa đơn và thanh toán. Mật khẩu, token và response body không được in ra.

CI trong `.github/workflows/dotnet.yml` chạy restore, build Release và toàn bộ test .NET khi push/pull request. Các kiểm thử SQL Server qua HTTP được chạy riêng trên database phát triển.

Đã xác thực trong môi trường phát triển hiện tại: 109 test .NET ở cấu hình Release, 31 kiểm tra HTTP nghiệp vụ và 11 kiểm tra đồng thời trên SQL Server đều đạt; không có test .NET bị bỏ qua. Việc khởi tạo schema trống và tạo admin bằng mật khẩu bootstrap cũng đã được kiểm tra trực tiếp.

Hướng dẫn khởi động riêng cho máy cloud hiện tại nằm trong [docs/cloud-environment-start.md](docs/cloud-environment-start.md). Các đường dẫn `.setup` trong tài liệu đó là trạng thái cục bộ của máy cloud; trên máy mới, dùng quy trình Docker Compose và biến môi trường được mô tả ở README này.
