# Cấu trúc backend

Mở `RepairSystem.sln` trong Visual Studio để xem bốn project. Thư mục con chỉ nhóm mã nguồn; chúng không đổi URL API, namespace hay kết nối CSDL.

Đồng nghiệp mới clone repo: bắt đầu từ [SETUP_DATABASE_BACKEND.md](SETUP_DATABASE_BACKEND.md).

```text
Project1_backend/
├─ RepairSystem/                 API, cấu hình, Program.cs
│  └─ Controllers/
│     ├─ Auth/                   đăng nhập, tài khoản, xác định nhân viên V3
│     ├─ Billing/                báo giá, hóa đơn, thanh toán, bàn giao
│     ├─ Catalog/                dịch vụ, linh kiện, loại/mẫu linh kiện, nhà cung cấp
│     ├─ Inventory/              kho, nhập kho, yêu cầu và trả vật tư
│     ├─ Repair/                 tiếp nhận, sửa chữa, dịch vụ của phiếu
│     ├─ Staff/                  nhân viên
│     └─ Warranty/               bảo hành khách và nhà cung cấp
├─ RepairSystem.Service/         DTO, interface, service, helper
├─ RepairSystem.Model/           entity và AppDbContext hiện có
│  └─ ScaffoldV3/               entity và AppDbContextV3 sinh từ CSDL V3
├─ RepairSystem.Data/            repository và unit of work
├─ database/                     script tạo mới CSDL V3 cho người clone repo
├─ data/                         SQL tham khảo cũ
└─ archive/                      mã mẫu và file .bak cũ, không được build
```

Không cần thư mục `Controllers/V3`: các controller mới đã nằm cùng nghiệp vụ với controller hiện có. Tên lớp và route `/api/v3/...` vẫn giữ để frontend không phải đổi đường dẫn; namespace hiện tại cũng được giữ để tránh thay đổi tham chiếu mã. `ScaffoldV3` trong **Model** có mục đích khác: đây là entity và context sinh tự động từ CSDL V3. Không nên sửa tay các file sinh này vì scaffold lại sẽ ghi đè.

Các file `.bak` được chuyển vào `archive/legacy-auth`; `WeatherForecast` mẫu nằm trong `archive/template`. Chúng còn để đối chiếu nhưng không tham gia project. Hai khai báo thư mục rỗng `Controllers/NewFolder` và `DTOs/Common` đã bỏ khỏi `.csproj`.

Kiểm tra sau khi di chuyển:

```powershell
dotnet build Project1_backend/RepairSystem.sln --no-restore -m:1
```

Nếu Visual Studio đang mở Folder View, dùng **Refresh** hoặc mở lại `RepairSystem.sln` để thấy cây thư mục mới. Khi tạo controller mới, chọn thư mục theo nghiệp vụ; dùng route tường minh (`[Route(...)]`) để đường dẫn API không phụ thuộc vị trí file.
