# ?? TROUBLESHOOTING - H??ng D?n Fix L?i Ch?y

## ?? V?n ?? Chính

N?u ?ng d?ng không ch?y ???c, có th? do các lý do sau:

---

## ?? KI?M TRA VÀ FIX T?NG B??C

### **B??c 1: Ki?m tra SQL Server**

? **L?i**: "Cannot connect to database"

? **Gi?i pháp**:

#### Windows
```bash
# M? Services
Win + R
services.msc

# Tìm SQL Server (SQLEXPRESS ho?c tên khác)
# Ki?m tra status: Running

# Ho?c dùng SQL Server Management Studio
# K?t n?i v?i (LocalDB)\mssqllocaldb ho?c .\SQLEXPRESS
```

#### Ho?c dùng PowerShell
```powershell
# Ki?m tra SQL Server ?ang ch?y không
Get-Service -Name MSSQL* | Select-Object Name, Status

# Kh?i ??ng n?u d?ng
Start-Service -Name "MSSQL$SQLEXPRESS"
```

---

### **B??c 2: C?p nh?t Connection String**

? **L?i**: "Connection refused" ho?c "Cannot open database"

? **Gi?i pháp**:

M? file: `RepairSystem/appsettings.json`

C?p nh?t connection string theo server c?a b?n:

#### Tùy ch?n 1: Local Default (LocalDB)
```json
"DefaultConnection": "Data Source=(LocalDB)\\mssqllocaldb;Database=RepairsSystem;Integrated Security=true;TrustServerCertificate=True;"
```

#### Tùy ch?n 2: SQL Server Express (SQLEXPRESS)
```json
"DefaultConnection": "Data Source=.\\SQLEXPRESS;Database=RepairsSystem;Integrated Security=true;TrustServerCertificate=True;"
```

#### Tùy ch?n 3: Specific Server & Port
```json
"DefaultConnection": "Data Source=YOUR_SERVER:1433;Database=RepairsSystem;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
```

#### Tùy ch?n 4: Named Instance
```json
"DefaultConnection": "Data Source=YOUR_COMPUTER\\INSTANCE_NAME;Database=RepairsSystem;Integrated Security=true;TrustServerCertificate=True;"
```

---

### **B??c 3: Ki?m tra Server Name**

? **Không bi?t server name c?a mình**

? **Gi?i pháp**:

#### Cách 1: SQL Server Management Studio
- M? SSMS
- Nhìn "Server name" ? connection dialog
- Copy server name ?ó vào connection string

#### Cách 2: PowerShell
```powershell
# L?y computer name
$env:COMPUTERNAME

# L?y SQL Server instances
$instances = (Get-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server' -Name InstalledInstances).InstalledInstances
Write-Host "SQL Server instances: $instances"
```

#### Cách 3: Dùng (LocalDB)
N?u không ch?c, s? d?ng LocalDB (??n gi?n nh?t):
```json
"DefaultConnection": "Data Source=(LocalDB)\\mssqllocaldb;Database=RepairsSystem;Integrated Security=true;TrustServerCertificate=True;"
```

---

### **B??c 4: Xóa Database C? (N?u C?n)**

? **L?i**: "Database already exists but corrupted"

? **Gi?i pháp**:

#### SQL Server Management Studio
```sql
-- Xóa database c?
USE master;
GO
DROP DATABASE IF EXISTS RepairsSystem;
GO
```

#### Ho?c Command Line
```bash
cd RepairSystem
# Xóa migration folder n?u có
rm -r Migrations

# Database s? ???c t?o l?i khi ch?y
```

---

### **B??c 5: Clear & Rebuild**

```bash
cd D:\code\API\project1\RepairSystem

# Clean all
dotnet clean

# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run --project RepairSystem
```

---

## ?? GI?I PHÁP NHANH NH?T

### **N?u b?n không bi?t gì:**

**1. Ki?m tra SQL Server ch?y không:**
```bash
# Windows Command Prompt
sc query MSSQL$SQLEXPRESS
```

**2. Update connection string (s? d?ng LocalDB):**

File: `RepairSystem/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(LocalDB)\\mssqllocaldb;Database=RepairsSystem;Integrated Security=true;TrustServerCertificate=True;"
  },
  "JwtSettings": { ... }
}
```

**3. Ch?y ?ng d?ng:**
```bash
cd D:\code\API\project1\RepairSystem
dotnet run --project RepairSystem
```

---

## ?? CÁC L?I C? TH? VÀ FIX

### **L?i 1: "Cannot open database"**

```
Error: Cannot open database "RepairsSystem" requested by the login
```

**Nguyên nhân**: Database không t?n t?i  
**Fix**:
1. Database s? t? t?o khi ch?y l?n ??u
2. N?u v?n l?i, xóa database c?:
```sql
DROP DATABASE IF EXISTS RepairsSystem;
GO
```

---

### **L?i 2: "Login failed"**

```
Error: Login failed for user 'sa'
```

**Nguyên nhân**: Sai username/password  
**Fix**:
- Dùng Integrated Security thay vì sa account:
```json
"DefaultConnection": "Data Source=.\\SQLEXPRESS;Database=RepairsSystem;Integrated Security=true;TrustServerCertificate=True;"
```

---

### **L?i 3: "Cannot connect"**

```
Error: A network-related or instance-specific error occurred
```

**Nguyên nhân**: SQL Server không ch?y ho?c server name sai  
**Fix**:
1. Kh?i ??ng SQL Server
2. Ki?m tra server name trong SSMS
3. Update connection string

---

### **L?i 4: "Port 5001 already in use"**

```
Error: Failed to bind to address
```

**Nguyên nhân**: Port 5001 ?ang b? s? d?ng  
**Fix**:
```bash
# Tìm process dùng port 5001
netstat -ano | findstr :5001

# Kill process (thay PID b?ng ID tìm ???c)
taskkill /PID YOUR_PID /F

# Ho?c change port trong Program.cs
```

---

### **L?i 5: "Assembly or type not found"**

```
Error: CS0246: The type or namespace name could not be found
```

**Nguyên nhân**: Missing using statements ho?c reference  
**Fix**:
```bash
dotnet clean
dotnet restore
dotnet build
```

---

## ? KI?M TRA THÀNH CÔNG

N?u b?n th?y output này, ?ng d?ng ch?y thành công:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to exit.
```

? Truy c?p Swagger UI:
```
https://localhost:5001/swagger
```

---

## ?? QUICK DEBUG CHECKLIST

- [ ] SQL Server ?ang ch?y? (Services ho?c SSMS)
- [ ] Connection string ?úng? (appsettings.json)
- [ ] Server name ?úng? (dùng (LocalDB)\\mssqllocaldb n?u không ch?c)
- [ ] Firewall cho phép SQL Server?
- [ ] .NET 8 SDK cài ??t? (dotnet --version)
- [ ] Port 5001 không b? dùng?
- [ ] Rebuild solution? (dotnet clean && dotnet build)

---

## ?? LIÊN H? H? TR?

N?u v?n l?i:

1. **Ch?y l?nh này và ghi l?i error message**:
```bash
dotnet run --project RepairSystem 2>&1
```

2. **Ki?m tra file này** - cung c?p thông tin chi ti?t:
```
D:\code\API\project1\RepairSystem\RepairSystem\appsettings.json
```

3. **G?i error message ??y ??** cho developer

---

## ?? Cách Ki?m tra Connection String

### SQL Server Management Studio
1. M? SSMS
2. K?t n?i t?i server
3. Xem "Server name" ? Object Explorer
4. Copy vào connection string

### PowerShell
```powershell
# Ki?m tra SQL Server running
Get-Service -Name "MSSQL*" | Select Name, Status

# List instances
Invoke-Sqlcmd -Query "SELECT @@SERVERNAME" -ServerInstance "YOUR_SERVER"
```

---

**Sau khi fix, ?ng d?ng s? ch?y bình th??ng! ??**
