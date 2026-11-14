# Hướng dẫn đồng bộ, cài đặt và kiểm thử QLTBSC V3

Tài liệu cho đồng nghiệp đang giữ bản mã từ lúc mới tạo V3. Áp dụng cho **bản mã trong workspace ngày 08/11/2026**. Các bước dưới đây dùng máy Windows, SQL Server và PowerShell. Hãy làm trên CSDL phát triển/kiểm thử của mình; các ca kiểm thử sẽ tạo và thay đổi dữ liệu.

## 1. Lấy đúng bản mã trước khi cài

Workspace hiện gồm ba thư mục ngang cấp:

```text
BTL/
├── Project1_backend/   # ASP.NET Core, script tạo CSDL, seed và hai migration 08/11
├── RepairFrontend/     # React/Vite
└── database_v3/        # hai migration 06–07/11 cho CSDL V3 đã có
```

`Project1_backend` và `RepairFrontend` là **hai Git repository riêng**; `BTL` không phải repository. Các thay đổi ngày 08/11 hiện còn ở working tree của cả hai repo (gồm file mới chưa commit), nên chỉ `git pull` từ remote có thể **chưa lấy được bản đang mô tả ở đây**. Người chia sẻ mã cần commit/push đủ hai repo và gửi kèm thư mục `database_v3`, hoặc bàn giao một bản sao đầy đủ của cả ba thư mục. Đối chiếu các file sau trước khi làm tiếp:

- [`RepairSystem/Infrastructure/DatabaseSchemaValidator.cs`](RepairSystem/Infrastructure/DatabaseSchemaValidator.cs) phải kiểm tra `V3_20261108_RULES` và `V3_20261108_WORKFLOW`.
- [`database/BO_SUNG_QUY_TAC_KIEM_KE_20261108.sql`](database/BO_SUNG_QUY_TAC_KIEM_KE_20261108.sql) và [`database/BO_SUNG_CONG_VIEC_THANH_LY_TAI_CHINH_20261108.sql`](database/BO_SUNG_CONG_VIEC_THANH_LY_TAI_CHINH_20261108.sql) phải có trong backend.
- [`../RepairFrontend/src/App.jsx`](../RepairFrontend/src/App.jsx) phải có route `/finance`; [`../RepairFrontend/src/features/inventory/StockControlTab.jsx`](../RepairFrontend/src/features/inventory/StockControlTab.jsx) phải có trong frontend.

Không dùng giao diện ở `RepairFrontend/archive/legacy-ui`: đó là bản lưu trữ, không được route hiện tại sử dụng.

## 2. Chuẩn bị môi trường

1. Cài SQL Server, SSMS, .NET SDK 8 và Node.js tương thích với Vite 8 (`20.19+` hoặc `22.12+`; bản `21.x` không nằm trong dải `engines` của Vite hiện có). `npm` đi kèm Node.js. Tài khoản kết nối SQL cần quyền tạo CSDL nếu dùng nhánh tạo mới.
2. Kiểm tra trong PowerShell:

   ```powershell
   dotnet --list-sdks
   dotnet --list-runtimes
   node --version
   npm --version
   ```

   Runtime cần có `Microsoft.NETCore.App 8.x` và `Microsoft.AspNetCore.App 8.x` để chạy `net8.0`.
3. Ghi đúng **Server name** đã dùng trong SSMS: ví dụ `.` (instance mặc định), `.\SQLEXPRESS`, hoặc `(localdb)\MSSQLLocalDB`. Các lệnh bên dưới giả sử ba thư mục nằm trong cùng `BTL`.

## 3. Cài hoặc cập nhật CSDL `QLSCTBDT_V3`

### A. Máy chưa có `QLSCTBDT_V3`

1. Trong SSMS, kết nối đúng SQL Server instance bằng Windows Authentication. Mở và **Execute toàn bộ** [`database/TAO_MOI_QLSCTBDT_V3.sql`](database/TAO_MOI_QLSCTBDT_V3.sql). Script tự tạo `QLSCTBDT_V3`; nó dừng nếu tên này đã tồn tại. File tạo mới đã gồm migration API 06/11 và KTV 07/11, nên **không chạy lại** hai file đó.
2. Mở cửa sổ query mới, chọn database `QLSCTBDT_V3` ở thanh chọn database. Chạy `SELECT DB_NAME();` và chỉ tiếp tục khi kết quả đúng là `QLSCTBDT_V3`. Execute toàn bộ [`database/BO_SUNG_QUY_TAC_KIEM_KE_20261108.sql`](database/BO_SUNG_QUY_TAC_KIEM_KE_20261108.sql).
3. Vẫn trên đúng `QLSCTBDT_V3`, Execute toàn bộ [`database/BO_SUNG_CONG_VIEC_THANH_LY_TAI_CHINH_20261108.sql`](database/BO_SUNG_CONG_VIEC_THANH_LY_TAI_CHINH_20261108.sql).

### B. Máy đã có V3 từ đợt đầu

1. **Dừng backend, sao lưu CSDL bằng SSMS** trước khi thay schema. Không chạy lại file `TAO_MOI_QLSCTBDT_V3.sql`; không dùng script `NANG_CAP_QLSCTBDT_V3.sql` cho CSDL đã là V3. Nếu CSDL đang là `QLSCTBDT` đời cũ, hãy dùng nhánh A để tạo V3 riêng rồi kiểm tra phương án chuyển dữ liệu, không đổi tên CSDL cũ để chạy thử.
2. Trên đúng `QLSCTBDT_V3`, kiểm tra migration:

   ```sql
   USE [QLSCTBDT_V3];
   SELECT DB_NAME() AS database_dang_chon;
   SELECT version FROM dbo.v3_schema_migration
   WHERE version IN ('V3_20261106_API', 'V3_20261107_KTV',
                     'V3_20261108_RULES', 'V3_20261108_WORKFLOW')
   ORDER BY version;
   ```

3. Chỉ chạy các migration **còn thiếu**, theo đúng thứ tự bảng dưới. Mỗi script được thiết kế để từ chối chạy lần hai. Hai script 08/11 **không tự `USE QLSCTBDT_V3`**, vì vậy phải chọn đúng database trong SSMS trước khi Execute.

   | Thứ tự | Dấu phiên bản | File cần chạy |
   |---|---|---|
   | 1 | `V3_20261106_API` | [`../database_v3/BO_SUNG_API_20261106.sql`](../database_v3/BO_SUNG_API_20261106.sql) |
   | 2 | `V3_20261107_KTV` | [`../database_v3/BO_SUNG_QUY_TRINH_KTV_20261107.sql`](../database_v3/BO_SUNG_QUY_TRINH_KTV_20261107.sql) |
   | 3 | `V3_20261108_RULES` | [`database/BO_SUNG_QUY_TAC_KIEM_KE_20261108.sql`](database/BO_SUNG_QUY_TAC_KIEM_KE_20261108.sql) |
   | 4 | `V3_20261108_WORKFLOW` | [`database/BO_SUNG_CONG_VIEC_THANH_LY_TAI_CHINH_20261108.sql`](database/BO_SUNG_CONG_VIEC_THANH_LY_TAI_CHINH_20261108.sql) |

Nếu thiếu bảng `dbo.v3_schema_migration`, dừng và xác định lại CSDL/schema ban đầu trước khi chạy migration bổ sung. Không thêm thủ công dòng phiên bản để bỏ qua kiểm tra.

### Kiểm tra kết quả và nạp dữ liệu mẫu

Chạy truy vấn sau trong SSMS sau khi hoàn tất nhánh A hoặc B:

```sql
USE [QLSCTBDT_V3];
SELECT version FROM dbo.v3_schema_migration
WHERE version IN ('V3_20261106_API', 'V3_20261107_KTV',
                  'V3_20261108_RULES', 'V3_20261108_WORKFLOW')
ORDER BY version;
SELECT OBJECT_ID(N'dbo.kiem_ke_kho_v3', N'U') AS bang_kiem_ke,
       OBJECT_ID(N'dbo.cong_viec_sua_chua_v3', N'U') AS bang_cong_viec,
       OBJECT_ID(N'dbo.hoa_don_dien_tu_v3', N'U') AS bang_chung_tu;
```

Kỳ vọng có đủ **4 phiên bản**, và cả ba `OBJECT_ID` khác `NULL`. Backend cũng kiểm tra schema này khi khởi động.

Muốn có dữ liệu để thử giao diện, Execute [`database/DU_LIEU_MAU_V3.sql`](database/DU_LIEU_MAU_V3.sql) **sau khi đủ migration**. Script dùng cố định tên `QLSCTBDT_V3`, chỉ dành cho máy phát triển; nó in bốn tài khoản thử nghiệm, mã phiếu và tồn kho. Nếu phát hiện bộ dữ liệu bị nạp dở hoặc trùng dữ liệu, script dừng để kiểm tra thủ công. Bộ dữ liệu này không tạo sẵn dữ liệu cho mọi luồng ngày 08/11; các ca kiểm thử bên dưới chỉ rõ khi nào phải tạo bản ghi mới.

| Tài khoản thử nghiệm | Vai trò | Mật khẩu trên CSDL phát triển mới |
|---|---|---|
| `nguyenthimai` | Lễ tân | `Demo@123456` |
| `tranvanbinh` | Thủ kho | `Demo@123456` |
| `phamquocan` | Kỹ thuật viên | `Demo@123456` |
| `nguyenminhanh` | Khách hàng | `Demo@123456` |

Bộ dữ liệu giả định có 5 phiếu: A mới tiếp nhận; B chờ khách duyệt báo giá; C đang sửa, có yêu cầu vật tư chờ duyệt; D hoàn thành, hóa đơn 150.000đ đã thu 50.000đ; E đã bàn giao, có bảo hành. Dựa vào **mã phiếu và tên khách do script in ra**, không đoán ID cố định. Các phiếu này không bật quy tắc xác nhận tiếp nhận mới; phải tạo **phiếu mới trên giao diện** để thử quy tắc đó. Nếu đã nạp bộ `[DEMO]` trước đây, sao lưu rồi chạy [script chuẩn hóa](database/CHUAN_HOA_DU_LIEU_V3_20261114.sql).

## 4. Cấu hình và chạy backend

Mở PowerShell tại `BTL/Project1_backend`. Profile `http` chạy môi trường Development ở `http://localhost:5107`, kết nối mặc định `Server=.;Database=QLSCTBDT_V3;Trusted_Connection=True;TrustServerCertificate=True`.

Nếu SSMS dùng instance khác `.`, đặt biến trong **chính cửa sổ PowerShell chạy API**. Ví dụ SQL Server Express:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=.\SQLEXPRESS;Database=QLSCTBDT_V3;Trusted_Connection=True;TrustServerCertificate=True'
dotnet restore RepairSystem.sln
dotnet build RepairSystem.sln --no-restore -m:1
dotnet run --project RepairSystem/RepairSystem.csproj --launch-profile http
```

Với instance mặc định `.`, bỏ dòng đặt biến môi trường. Đợi `Now listening on: http://localhost:5107`, rồi mở [Swagger local](http://localhost:5107/swagger). Nếu API dừng với thông báo thiếu migration, kiểm tra lại bước 3 và Server name. Không cần `dotnet ef database update` hoặc scaffold: schema được tạo bằng SQL script. File `.env.example` chỉ là mẫu; ASP.NET Core **không tự đọc `.env`**.

Trên CSDL mới, backend Development tạo `admin` / `Admin@123` ở lần khởi động đầu **nếu chưa có admin**. Nếu CSDL đã có admin, khởi động lại không đặt lại mật khẩu. Tài khoản này chỉ để thử local; nếu dùng dữ liệu thật, đổi mật khẩu. Chạy ngoài Development cần đặt riêng `ConnectionStrings__DefaultConnection`, `JwtSettings__SecretKey` (ít nhất 32 byte) và mật khẩu khởi tạo admin theo [`README.md`](README.md).

## 5. Chạy frontend

Mở **PowerShell thứ hai** tại `BTL/RepairFrontend`:

```powershell
npm ci
npm run build
npm run dev
```

Mở `http://localhost:3000/login`. [`vite.config.js`](../RepairFrontend/vite.config.js) đặt cổng **3000** và chuyển `/api` tới `http://localhost:5107`. Nếu đổi cổng backend, sửa proxy tại đây rồi khởi động lại Vite. `npm ci` dùng đúng `package-lock.json`; khi sửa dependencies mới dùng `npm install`.

Thứ tự chạy thử nhanh: (1) Swagger mở được; (2) `admin` đăng nhập; (3) vào **Tổng quan hệ thống**, **Lễ tân & Thu ngân**, **Quản lý kho vật tư** không có lỗi tải dữ liệu; (4) đăng xuất, đăng nhập lần lượt bốn tài khoản demo và kiểm tra menu theo vai trò.

## 6. Chuẩn bị kiểm thử nghiệp vụ

- Thử trên CSDL riêng, dùng tên/ghi chú có tiền tố `[TEST]`. Ghi mã phiếu, ID kho, ID chứng từ và số tồn trước mỗi ca. Các ca thay đổi trạng thái không có nút quay lại; khi cần kiểm thử nhánh khác, tạo bản ghi mới.
- Cần **hai tài khoản Admin khác nhau** cho các ca yêu cầu người duyệt khác người tạo/đề xuất. Admin thứ hai có thể tạo tại **Quản trị nhân sự**. Cần ít nhất một tài khoản lễ tân, một thủ kho, một KTV và một khách; bộ demo cung cấp sẵn bốn vai trò sau.
- Tạo ít nhất một phiếu sửa **mới** qua **Lễ tân & Thu ngân → Tiếp nhận máy** để kiểm tra checklist, serial/lý do thiếu serial, ảnh và xác nhận tiếp nhận. Sau đó dùng cùng mã phiếu theo luồng: xác nhận → KTV nhận máy/kiểm tra → báo giá → khách duyệt → sửa/yêu cầu vật tư/xuất kho → hoàn thành → hóa đơn/thu tiền → bàn giao → bảo hành.
- Với hàng theo serial, chuẩn bị serial thử duy nhất; với kiểm kê, ghi lại **toàn bộ** dòng tồn của kho, kể cả dòng 0. Chuẩn bị ảnh JPEG/PNG/WebP nhỏ hơn 5 MB cho ảnh tiếp nhận và ảnh JPEG/PNG cùng biên bản PDF nhỏ hơn 10 MB cho hồ sơ thanh lý.
- Sau mỗi thao tác quan trọng, bấm **Làm mới**, tải lại trang và đăng nhập tài khoản liên quan để kiểm tra dữ liệu đã lưu ở API/CSDL, không chỉ thông báo thành công trên giao diện.

## 7. Ma trận kiểm thử trên trang

Đánh dấu từng ca `PASS`/`FAIL`/`CHƯA THỬ`, ghi mã bản ghi và ảnh chụp hoặc lỗi API. Trong các ca âm, kỳ vọng thao tác bị từ chối **và dữ liệu/tồn tiền/tồn kho không đổi**. Danh sách endpoint và quy tắc chi tiết nằm ở [`API_V3_COMPLETION.md`](API_V3_COMPLETION.md), [`API_BO_SUNG_20261106.md`](API_BO_SUNG_20261106.md), [`QUY_TAC_VA_KIEM_KE_20261108.md`](QUY_TAC_VA_KIEM_KE_20261108.md) và [`QUY_TRINH_CONG_VIEC_THANH_LY_TAI_CHINH_20261108.md`](QUY_TRINH_CONG_VIEC_THANH_LY_TAI_CHINH_20261108.md).

### A. Đăng nhập, phân quyền, tổng quan và danh mục

| ID | Vai trò / trang | Thao tác | Kết quả mong đợi |
|---|---|---|---|
| A01 | Khách chưa đăng nhập `/` | Mở trang chủ, xem dịch vụ và tra serial demo | Dữ liệu công khai tải được; không hiện phiếu của người khác. |
| A02 | `/login` | Đăng nhập đúng từng tài khoản demo, sau đó đăng xuất | Đúng vai trò/menu; đăng xuất quay về trạng thái chưa đăng nhập. |
| A03 | `/login` | Đăng nhập sai mật khẩu và thử tài khoản bị khóa | Có thông báo lỗi; không vào trang được bảo vệ. |
| A04 | `/login` | Đăng ký khách mới, thử trùng tên đăng nhập/số điện thoại | Bản hợp lệ đăng nhập được; bản trùng bị chặn, không tạo thêm tài khoản. |
| A05 | Phân quyền | Truy cập trực tiếp `/hr`, `/finance`, `/warehouse`, `/technician`, `/reception` bằng tài khoản không có quyền | Bị điều hướng/chặn; thử API tương ứng bằng Swagger cũng bị từ chối. |
| A06 | Tổng quan `/dashboard` | So số phiếu mở, hoàn thành, bảo hành và cảnh báo tồn với trang chi tiết | Số liệu và phiếu gần đây khớp dữ liệu đang có; KTV chỉ thấy phiếu được giao. |
| A07 | Admin `/hr` | Tạo nhân viên/tài khoản lễ tân, kho, KTV, Admin thứ hai | Từng tài khoản đăng nhập đúng vai trò; hồ sơ hiện trong danh sách. |
| A08 | Admin `/hr` | Khóa/mở tài khoản thử, đặt lại mật khẩu | Tài khoản khóa không đăng nhập; mật khẩu mới dùng được, mật khẩu cũ không còn dùng được. |
| A09 | Admin `/pricing` | Thêm, sửa, ngừng dùng dịch vụ; dùng lễ tân để xem | Giá mới hiện sau tải lại; dịch vụ ngừng dùng biến khỏi danh sách mặc định; lễ tân chỉ được xem. |
| A10 | Admin `/warehouse` | Tạo kho, nhà cung cấp, loại/thuộc tính và mẫu linh kiện; sửa cấu hình mẫu | Danh mục hiện ở phiếu nhập; trường thông số bắt buộc và chính sách serial được kiểm tra. |

### B. Khách hàng, lịch hẹn và tiếp nhận

| ID | Vai trò / trang | Thao tác | Kết quả mong đợi |
|---|---|---|---|
| B01 | Khách `/` → Đặt lịch | Chọn ngày tương lai, khung giờ còn chỗ, nhập thiết bị/lỗi, gửi lịch | Lịch vào `CHO_XAC_NHAN`, xuất hiện ở **Lịch hẹn của tôi** và trang lễ tân. |
| B02 | Khách/lễ tân | Đặt tới hết sức chứa một khung; thử đặt thêm hoặc giờ quá khứ | Khung hết chỗ bị vô hiệu; API không nhận lịch vượt sức chứa/quá khứ. |
| B03 | Lễ tân `/reception` | Xác nhận một lịch, từ chối một lịch khác, gắn lịch đã xác nhận với phiếu mới | Trạng thái/ghi chú cập nhật; lịch gắn đúng mã phiếu. |
| B04 | Khách `/` | Hủy lịch tương lai còn cho phép hủy; thử hủy lại | Lịch chuyển `HUY`, chỗ trống được trả lại; không hủy lặp. |
| B05 | Lễ tân `/reception` | Tiếp nhận máy mới với đủ lỗi, ngoại quan, phụ kiện và 5 mục checklist; thử thiếu serial nhưng có lý do | Phiếu tạo thành công, dữ liệu hiển thị đúng; thiếu cả serial lẫn lý do bị chặn. |
| B06 | Lễ tân → phiếu mới → Điều phối · ảnh | Tải chưa đủ ảnh tiếp nhận, thử xác nhận; sau đó tải đủ ảnh và ảnh biên nhận đã ký, nhập tên người ký | Lần đầu bị chặn; lần sau ghi nhận người ký, ID ảnh; ảnh tiếp nhận bị khóa sau xác nhận. |
| B07 | KTV `/technician` | Thử nhận máy trước B06, rồi nhận sau B06 và lưu kết quả kiểm tra | Trước xác nhận bị chặn; sau đó có người/thời điểm nhận và nội dung kiểm tra trong phiếu. |
| B08 | Lễ tân `/reception` | Đổi KTV của phiếu còn tiếp nhận | KTV cũ mất quyền xử lý; xác nhận nhận máy và kết quả kiểm tra cũ được xóa, KTV mới phải làm lại. |
| B09 | Lễ tân + khách | Cấp mã liên kết hồ sơ cũ tại quầy; đăng ký tài khoản khách với mã đó | Khách thấy đúng phiếu/hồ sơ đã liên kết; mã sai/hết hạn/đã dùng bị chặn. |
| B10 | Khách `/` → Phiếu của tôi | Xem tiến độ, lịch sử, báo giá/chi tiết và chỉ phiếu của mình | Dữ liệu khớp phiếu; không truy cập được phiếu của khách khác bằng URL/API. |

### C. Báo giá, sửa chữa, vật tư và công việc

| ID | Vai trò / trang | Thao tác | Kết quả mong đợi |
|---|---|---|---|
| C01 | Lễ tân → phiếu → Báo giá | Tạo báo giá có dịch vụ/linh kiện, gửi khách; thử sửa bản đã gửi | Tổng tiền đúng; bản gửi không sửa trực tiếp được, phải tạo phiên bản mới. |
| C02 | Khách `/` hoặc lễ tân | Khách duyệt/từ chối báo giá đang chờ; tạo phiên bản mới và thử duyệt bản cũ | Chỉ phản hồi đúng báo giá mới nhất; lịch sử phản hồi được lưu. |
| C03 | KTV → phiếu | Thử chuyển `TIEP_NHAN` → `DANG_SUA` khi thiếu nhận máy, kết quả kiểm tra hoặc báo giá được duyệt; bổ sung đủ rồi thử lại | Bị chặn ở từng điều kiện thiếu; khi đủ thì chuyển trạng thái thành công. |
| C04 | KTV → Yêu cầu vật tư | Tạo yêu cầu cho phiếu đang sửa, chọn linh kiện và số lượng | Yêu cầu hiện ở KTV và kho, gắn đúng phiếu. |
| C05 | Kho → Xuất kho cho Phiếu sửa chữa | Duyệt một phần/đủ yêu cầu của KTV; dùng Admin tạo yêu cầu thử và tự duyệt | Số lượng duyệt hiện đúng, không vượt yêu cầu; người tạo yêu cầu tự duyệt bị chặn kể cả Admin. |
| C06 | Kho → xuất vật tư | Xuất đúng yêu cầu đã duyệt; thử vượt số lượng, sai phiếu, hàng hết tồn hoặc serial không ở kho | Xuất hợp lệ giảm tồn đúng một lần và ghi serial/phiếu; các trường hợp sai không đổi tồn. |
| C07 | Kho/KTV | Trả vật tư tốt chưa dùng trước hóa đơn; thử trả quá số đã xuất hoặc sau hóa đơn | Trả hợp lệ cộng tồn, serial về kho và ghi biến động; trường hợp sai bị chặn. Nếu giao diện không có nút, dùng endpoint V3 trong Swagger. |
| C08 | Lễ tân → Công việc sửa chữa | Giao việc cho KTV, gắn hồ sơ bảo hành nếu có; thử hủy việc giao nhầm | Việc hiện đúng phiếu/KTV; việc hủy có audit và không chặn hoàn thành phiếu. |
| C09 | KTV → phiếu → Công việc | Bắt đầu, hoàn thành với kết quả và dòng vật tư đã xuất; thử hoàn thành phiếu khi còn việc chưa xong | Lưu thời điểm/kết quả/linh kiện; phiếu bị chặn khi còn việc `CHO_LAM`/`DANG_LAM`. |
| C10 | KTV/Admin | KTV đề xuất công; Admin khác người thực hiện duyệt; thử tự duyệt | Mức đề xuất/duyệt lưu đúng; tự duyệt bị chặn. |
| C11 | KTV → phiếu | Thêm/xem ảnh lúc sửa, chuyển `CHO_LINH_KIEN` rồi trở lại, hoàn thành có kết quả | Ảnh xem lại được; trạng thái và nhật ký cập nhật đúng, chỉ người có quyền sửa được. |

### D. Nhập kho, serial, kiểm kê và cách ly

| ID | Vai trò / trang | Thao tác | Kết quả mong đợi |
|---|---|---|---|
| D01 | Kho → Lịch sử Nhập hàng | Mở phiếu nháp `MP-2026-0419`, xác nhận; tạo một phiếu mới rồi hủy nháp | Xác nhận cộng tồn một lần; phiếu hủy không cộng tồn. |
| D02 | Kho → phiếu nhập | Nhập mẫu có theo dõi serial với danh sách đủ, duy nhất; thử thiếu/trùng serial | Chỉ bản hợp lệ được xác nhận; serial mới thấy trong Tra cứu & Quản lý Serial. |
| D03 | Kho → Danh mục & Tồn kho | Tìm/lọc kho, đặt ngưỡng tồn tối thiểu cao hơn tồn hiện tại | Cảnh báo thiếu hàng và chỉ số tổng quan cập nhật sau làm mới. |
| D04 | Kho → Tra cứu & Quản lý Serial | Tra serial còn kho, đã xuất và serial không tồn tại | Nguồn nhập, NCC, kho và trạng thái khớp chứng từ; serial lạ không lộ dữ liệu sai. |
| D05 | Kho → Kiểm kê & cách ly | Tạo phiếu kiểm kê đủ mọi dòng tồn, gồm dòng 0 và danh sách serial quét; thử bỏ một dòng/trùng serial | Phiếu hợp lệ chờ duyệt; dữ liệu thiếu/sai bị chặn. |
| D06 | Admin khác người kiểm | Duyệt/từ chối kiểm kê; thử tự duyệt hoặc thay đổi tồn sau khi đếm rồi mới duyệt | Chênh lệch hợp lệ cập nhật tồn/nhật ký serial; tự duyệt và dữ liệu đã lệch bị chặn. |
| D07 | Kho → Kiểm kê & cách ly | Cách ly một hàng/serial lỗi; thử xuất chính hàng/serial đó | Tồn khả dụng giảm ngay; không xuất được hàng cách ly. |
| D08 | Kho/Admin → hồ sơ cách ly | Đề xuất `NHAP_LAI`/`THANH_LY`/`TIEU_HUY`, tải ít nhất 1 ảnh và 1 biên bản, Admin khác duyệt rồi xử lý | Có lịch sử quyết định và chứng cứ xem được; nhập lại cộng tồn, thanh lý/tiêu hủy không cộng tồn. |
| D09 | Admin → Quản lý kho → Quy tắc vận hành | Đổi ngưỡng ảnh/duyệt bảo hành lặp, quy tắc công và serial; tải lại trang | Giá trị lưu và áp dụng cho bản ghi mới; tài khoản kho không được sửa. |

### E. Hóa đơn, công nợ, bảo hành và đối soát

| ID | Vai trò / trang | Thao tác | Kết quả mong đợi |
|---|---|---|---|
| E01 | Lễ tân → phiếu hoàn thành | Ghi dịch vụ đã duyệt, lập hóa đơn, kiểm tra từng dòng; thử lập lần hai | Tổng sau giảm đúng, chi tiết chốt không đổi; không có hóa đơn trùng. |
| E02 | Lễ tân → Hóa đơn · thu tiền | Với phiếu demo D, thu 100.000đ còn lại; thử thu vượt và hoàn vượt số đã thu | Công nợ về 0; thu/hoàn sai bị chặn, lịch sử giao dịch và số dư đúng. |
| E03 | Lễ tân → bàn giao | Thử bàn giao khi chưa có hóa đơn hoặc còn nợ; sau khi đủ điều kiện bàn giao | Không vượt điều kiện; phiếu chuyển `DA_TRA`, có người nhận/thời điểm và lịch sử. Trường hợp còn nợ cần người duyệt và lý do. |
| E04 | Lễ tân → Bảo hành khách | Tạo cam kết sau bàn giao, tiếp nhận khách quay lại, liên kết phiếu sửa cùng thiết bị | Cam kết/hạn bảo hành, lần quay lại và phiếu liên kết hiện đúng; khách thấy bảo hành của mình. |
| E05 | Admin/lễ tân → Bảo hành khách | Tạo đủ số lần quay lại đến ngưỡng cấu hình, thử chuyển trạng thái khi chưa duyệt; Admin khác duyệt/từ chối | Trước duyệt bị chặn; quyết định, nguyên nhân sơ bộ và lý do được lưu. |
| E06 | Lễ tân → Bảo hành khách | Ghi phân tích nguyên nhân, bên chịu chi phí, dự kiến/thực tế; thử hoàn thành khi thiếu chi phí thực tế | Hiện phân tích và số lần; thiếu chi phí thực tế bị chặn. |
| E07 | Kho → Quản lý Bảo hành NCC | Lập/gửi hồ sơ từ kho hoặc hàng khách, nhận kết quả, xử lý `NHAP_LAI`/`NHAP_THAY_THE`/`KHONG_NHAP` | Trừ/cộng tồn đúng nguồn; hàng thay thế phải qua phiếu nhập đã xác nhận, không cộng lần hai. |
| E08 | Kho → Hóa đơn đầu vào | Ghi hóa đơn cho phiếu nhập đã xác nhận; thử số tiền/mã số thuế khớp và lệch | Bản khớp được đối chiếu; bản lệch có trạng thái `CHENH_LECH` và ghi chú. |
| E09 | Admin → phiếu → Hóa đơn · thu tiền | Ghi số chứng từ điện tử **đã phát hành bên ngoài**; ghi điều chỉnh/thay thế/hủy đã làm bên ngoài | Thông tin, tổng và chênh lệch hiện trong đối soát; thao tác này không phát hành chứng từ thật. |
| E10 | Admin `/finance` | Xem phiếu nhập thiếu hóa đơn, hóa đơn ra thiếu chứng từ, công nợ, công đã duyệt chưa chi; ghi chi trả công | Chỉ số khớp bản ghi; chi trả có phương thức/mã chứng từ, không chi hai lần. |

### F. Kiểm tra chung sau mỗi nhóm

| ID | Thao tác | Kết quả mong đợi |
|---|---|---|
| F01 | Tải lại trang/đăng xuất rồi đăng nhập lại | Bản ghi, trạng thái, tiền và tồn vẫn đúng; không chỉ tồn tại trong bộ nhớ giao diện. |
| F02 | Mở Developer Tools → Network khi thao tác | Request `/api` trả mã thành công hợp lệ; không có lỗi 401/403/500 ngoài ca âm có chủ đích. |
| F03 | Bấm gửi/lưu hai lần gần nhau trên một thao tác không được lặp | Không tạo hai phiếu nhập, hóa đơn, giao dịch hoặc chi trả ngoài ý muốn. |
| F04 | Đăng nhập vai trò khác và mở trực tiếp URL/Swagger endpoint không được phép | Backend cũng từ chối, không chỉ ẩn nút trên frontend. |
| F05 | Xem lại ảnh tiếp nhận và tài liệu thanh lý sau khi khởi động lại API | File tải/xem được; dữ liệu file trong `RepairSystem/App_Data/repair-photos` và `RepairSystem/App_Data/thanh-ly` cần đi cùng bản sao lưu CSDL khi chuyển máy. |

## 8. Ghi kết quả và xử lý lỗi

Mẫu ghi cho mỗi ca: `ID | PASS/FAIL/CHƯA THỬ | tài khoản | mã phiếu/ID | dữ liệu trước → sau | HTTP/status/lỗi | ảnh chụp`. Một luồng được xem là đạt khi **giao diện, API và dữ liệu sau tải lại** cùng khớp kết quả mong đợi. `dotnet build` và `npm run build` chỉ xác nhận mã biên dịch; chúng không chứng minh nghiệp vụ chạy đúng.

| Hiện tượng | Kiểm tra nhanh |
|---|---|
| API báo không kết nối được SQL Server/CSDL | So Server name trong SSMS với `ConnectionStrings__DefaultConnection`; kiểm tra SQL Server đang chạy và quyền Windows Authentication. |
| API báo thiếu `V3_20261108_RULES` hoặc `WORKFLOW` | Xem lại 4 dòng migration ở bước 3; dừng API, chọn đúng database, chạy đúng file còn thiếu theo thứ tự. |
| Mở `/login` nhưng request `/api` lỗi proxy | API phải đang nghe cổng 5107; kiểm tra `vite.config.js` và khởi động lại Vite sau khi sửa. |
| Admin `Admin@123` không đăng nhập được | CSDL có thể đã có admin từ trước; backend không đặt lại mật khẩu. Kiểm tra đang kết nối đúng CSDL. |
| Trang không thấy nút/chức năng mới | Kiểm tra đúng bản mã của **cả hai repo** và các file chưa commit; kiểm tra vai trò đang đăng nhập. |
| Upload ảnh/tài liệu lỗi sau chuyển máy | Kiểm tra quyền ghi và sao lưu hai thư mục `App_Data` bên cạnh CSDL; file không nằm trong `.bak`. |

Các script cũ ở `database_v3/tests` dùng tên CSDL test cố định, có bước tạo/xóa CSDL và một số đường dẫn máy tác giả; chúng **không thay thế ma trận kiểm thử hiện tại**. Chỉ chạy chúng khi đã đọc toàn bộ script, biết rõ instance đích và được phép tạo/xóa CSDL test. Phần ghi chứng từ điện tử trong V3 là đối chiếu chứng từ phát hành ở hệ thống ngoài; hệ thống hiện không tự phát hành hóa đơn điện tử hoặc tự tính lương/thuế.
