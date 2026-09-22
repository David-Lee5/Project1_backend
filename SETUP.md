# Setup Guide - RepairSystem Web API

## Yêu c?u h? th?ng

- .NET 8 SDK
- SQL Server 2019 ho?c cao h?n
- Visual Studio 2022 ho?c Visual Studio Code
- Git

## Các b??c cài ??t

### 1. Clone Repository
```bash
git clone <repository-url>
cd RepairSystem
```

### 2. Restore NuGet Packages
```bash
dotnet restore
```

### 3. C?u hình Database Connection

M? file `appsettings.json` trong th? m?c `RepairSystem` và c?p nh?t connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=YOUR_SERVER\\YOUR_INSTANCE;Database=RepairsSystem;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

**Ví d?:**
- Local: `Data Source=(LocalDB)\\mssqllocaldb;Database=RepairsSystem;Trusted_Connection=True;TrustServerCertificate=True;`
- Named Instance: `Data Source=YOUR_COMPUTER\\SQLEXPRESS;Database=RepairsSystem;Trusted_Connection=True;TrustServerCertificate=True;`

### 4. T?o Database

Database s? t? ??ng ???c t?o khi ?ng d?ng kh?i ??ng l?n ??u tiên. N?u mu?n t?o th? công:

```bash
# T?o migration ban ??u (n?u ch?a có)
dotnet ef migrations add InitialCreate --project RepairSystem.Data --startup-project RepairSystem

# C?p nh?t database
dotnet ef database update --project RepairSystem.Data --startup-project RepairSystem
```

### 5. C?u hình JWT Settings

C?p nh?t file `appsettings.json` - ph?n `JwtSettings`:

```json
"JwtSettings": {
  "SecretKey": "your-super-secret-key-that-must-be-at-least-32-characters-long",
  "Issuer": "RepairSystemAPI",
  "Audience": "RepairSystemUsers",
  "ExpirationMinutes": 15
}
```

**L?u ý:** 
- `SecretKey` ph?i có ít nh?t 32 ký t?
- Thay ??i SecretKey trong production
- Gi? bí m?t SecretKey

### 6. Build Solution
```bash
dotnet build
```

### 7. Ch?y ?ng d?ng
```bash
dotnet run --project RepairSystem
```

?ng d?ng s? ch?y t?i: `https://localhost:5001` ho?c `http://localhost:5000`

### 8. Truy c?p API Documentation

M? trình duy?t và truy c?p:
```
https://localhost:5001/swagger/index.html
```

## Default Admin Credentials

Sau khi ?ng d?ng kh?i ??ng l?n ??u tiên, m?t admin user s? ???c t?o:

- **Username:** `admin`
- **Password:** `Admin@123`
- **Email:** `admin@repairsystem.com`
- **Role:** Admin (toàn b? quy?n)

**Thay ??i m?t kh?u ngay sau khi ??ng nh?p:**
```
POST /api/auth/change-password
{
  "oldPassword": "Admin@123",
  "newPassword": "YourNewPassword123!",
  "confirmPassword": "YourNewPassword123!"
}
```

## Ki?m tra cài ??t

### 1. ??ng nh?p
```bash
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

N?u thành công, b?n s? nh?n ???c JWT token.

### 2. Ki?m tra API
```bash
GET /api/auth/me
Authorization: Bearer {your-access-token}
```

## C?u trúc th? m?c Project

```
RepairSystem/
??? RepairSystem/                 # Main API Project
?   ??? Controllers/
?   ??? Program.cs
?   ??? appsettings.json
??? RepairSystem.Model/           # Data Models
??? RepairSystem.Data/            # Data Access Layer
??? RepairSystem.Service/         # Business Logic Layer
```

## Các l?nh h?u ích

### Entity Framework Core

```bash
# T?o migration m?i
dotnet ef migrations add {MigrationName} --project RepairSystem.Data --startup-project RepairSystem

# Xóa migration cu?i cùng
dotnet ef migrations remove --project RepairSystem.Data --startup-project RepairSystem

# C?p nh?t database
dotnet ef database update --project RepairSystem.Data --startup-project RepairSystem

# Xem t?t c? migrations
dotnet ef migrations list --project RepairSystem.Data --startup-project RepairSystem

# Rollback migration
dotnet ef database update {PreviousMigrationName} --project RepairSystem.Data --startup-project RepairSystem
```

### Build & Run

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run (development)
dotnet run --project RepairSystem

# Run (release)
dotnet run --configuration Release --project RepairSystem

# Watch mode (t? ??ng reload khi code thay ??i)
dotnet watch run --project RepairSystem
```

## Troubleshooting

### L?i: "Cannot connect to SQL Server"
- Ki?m tra SQL Server ?ang ch?y
- Ki?m tra connection string trong appsettings.json
- Ki?m tra firewall settings

### L?i: "The type or namespace name could not be found"
- Run `dotnet restore` ?? cài ??t dependencies
- Rebuild solution
- Ki?m tra package references

### L?i: Database migration failed
- Xóa database và t?o l?i t? migrations
- Ki?m tra t?t c? migrations có valid
- Run migrations step by step

### L?i: JWT Token không h?p l?
- Ki?m tra SecretKey trong appsettings.json
- Ki?m tra token expiration time
- Ki?m tra Authorization header format: `Bearer {token}`

## Environment Configuration

### Development
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### Production
```json
{
  "JwtSettings": {
    "SecretKey": "CHANGE_THIS_IN_PRODUCTION_USE_ENVIRONMENT_VARIABLES"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Error"
    }
  }
}
```

## Security Checklist

- [ ] Thay ??i default admin password
- [ ] Thay ??i JWT SecretKey
- [ ] Enable HTTPS
- [ ] Configure CORS appropriately
- [ ] Set up database backups
- [ ] Configure firewall rules
- [ ] Use environment variables for sensitive data
- [ ] Enable API rate limiting
- [ ] Set up monitoring and logging
- [ ] Regular security updates

## Documentation

- API Documentation: Swagger UI t?i `/swagger`
- README.md: T?ng quan v? d? án
- SETUP.md: File này

## Support & Troubleshooting

N?u g?p v?n ??:
1. Ki?m tra t?p log
2. Xem l?i trong output window
3. T?o issue trên repository
4. Ki?m tra tài li?u Microsoft:
   - https://docs.microsoft.com/en-us/dotnet/
   - https://docs.microsoft.com/en-us/ef/core/

## Ti?p theo

Sau khi setup thành công:
1. T?o thêm roles tùy theo nhu c?u
2. T?o users test
3. C?u hình permissions
4. Phát tri?n thêm modules
5. Setup CI/CD pipeline
