# RepairSystem Web API

## Gi?i thi?u

RepairSystem là m?t REST API ???c xây d?ng v?i .NET 8 cho h? th?ng qu?n lý s?a ch?a thi?t b?. API cung c?p các tính n?ng qu?n lý ng??i dùng, phân quy?n, qu?n lý kho, d?ch v? s?a ch?a, và qu?n lý khách hàng.

## Ki?n trúc

D? án ???c chia thành 4 projects chính:

- **RepairSystem**: Main API project (ASP.NET Core)
- **RepairSystem.Model**: Entity models và DTOs
- **RepairSystem.Data**: Data access layer (EF Core, DbContext, Repositories)
- **RepairSystem.Service**: Business logic layer (Services, Helpers, Interfaces)

## Tính n?ng chính

### 1. Authentication & Authorization
- JWT Token-based authentication
- Refresh token mechanism
- Role-based access control (RBAC)
- Permission-based authorization
- Password hashing v?i SHA256

### 2. User Management
- User registration/login
- User profile management
- Change password
- User status management

### 3. Role & Permission Management
- Create, read, update, delete roles
- Create, read, update, delete permissions
- Assign permissions to roles
- Module-based permissions (Users, Roles, Permissions, Inventory, Repair, Customer)

### 4. Repair Service Management
- Create repair requests
- Track repair status
- Manage repair details
- Invoice generation

### 5. Inventory Management
- Manage spare parts
- Track inventory levels
- Import/Export tracking

### 6. Customer Management
- Customer registration
- Customer information management
- Device management per customer

## C?u trúc Th? m?c

```
RepairSystem/
??? Controllers/
?   ??? AuthController.cs          # Authentication endpoints
?   ??? UsersController.cs         # User management endpoints
?   ??? RolesController.cs         # Role management endpoints
?   ??? PermissionsController.cs   # Permission management endpoints
??? Program.cs                      # Application startup configuration
??? appsettings.json               # Configuration settings

RepairSystem.Data/
??? AppDbContext.cs                # Entity Framework DbContext
??? Repository/
?   ??? GenericRepository.cs       # Generic repository implementation
?   ??? IRepository/
?   ?   ??? IGenericRepository.cs # Generic repository interface
?   ??? UnitOfWork/
?       ??? IUnitOfWork.cs        # Unit of Work interface
?       ??? UnitOfWork.cs         # Unit of Work implementation

RepairSystem.Model/
??? TaiKhoan.cs                    # User account model
??? Role.cs                        # Role model
??? Permission.cs                  # Permission model
??? ThietBi.cs                     # Device model
??? PhieuSuaChua.cs               # Repair request model
??? LinhKien.cs                    # Spare part model
??? KhachHang.cs                   # Customer model
??? HoaDon.cs                      # Invoice model
??? TonKho.cs                      # Inventory model
??? ... (other models)

RepairSystem.Service/
??? IServices/
?   ??? IAuthService.cs           # Authentication service interface
?   ??? IUserService.cs           # User service interface
?   ??? IRoleService.cs           # Role service interface
?   ??? IPermissionService.cs     # Permission service interface
?   ??? ITokenService.cs          # Token service interface
??? Services/
?   ??? AuthService.cs            # Authentication service implementation
?   ??? UserService.cs            # User service implementation
?   ??? RoleService.cs            # Role service implementation
?   ??? PermissionService.cs      # Permission service implementation
?   ??? GenericService.cs         # Generic service implementation
??? DTOs/
?   ??? Auth/
?   ?   ??? LoginRequestDto.cs
?   ?   ??? LoginResponseDto.cs
?   ?   ??? TokenDto.cs
?   ?   ??? RegisterRequestDto.cs
?   ?   ??? RefreshTokenRequestDto.cs
?   ?   ??? ChangePasswordRequestDto.cs
?   ??? Model/
?   ?   ??? ThietBiDto.cs
?   ?   ??? PhieuSuaChuaDto.cs
?   ?   ??? LinhKienDto.cs
?   ?   ??? KhachHangDto.cs
?   ?   ??? HoaDonDto.cs
?   ?   ??? TonKhoDto.cs
?   ??? UserDto.cs
?   ??? RoleDto.cs
?   ??? PermissionDto.cs
?   ??? CreateUserRequestDto.cs
?   ??? UpdateUserStatusRequestDto.cs
?   ??? ApiResponse.cs
??? Helpers/
?   ??? PasswordHelper.cs         # Password hashing and verification
?   ??? TokenHelper.cs            # JWT token generation and validation
?   ??? RequirePermissionAttribute.cs  # Permission check attribute
?   ??? SeedData.cs               # Database seed data initialization
??? Profiles/
    ??? MappingProfile.cs         # AutoMapper configuration
```

## C?u hình

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=...;Database=RepairsSystem;..."
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-must-be-at-least-32-characters-long",
    "Issuer": "RepairSystemAPI",
    "Audience": "RepairSystemUsers",
    "ExpirationMinutes": 15
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### JWT Configuration
- **SecretKey**: C?n ???c thay ??i trong production (t?i thi?u 32 ký t?)
- **Issuer**: Ng??i phát hành token
- **Audience**: Ng??i nh?n token
- **ExpirationMinutes**: Th?i gian h?t h?n c?a access token (phút)

## Authentication Flow

### 1. Login
```
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}

Response:
{
  "success": true,
  "message": "Login successful",
  "token": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "R2FtZm...",
    "expiresIn": "2024-01-15T10:15:00Z"
  },
  "user": {
    "taiKhoanId": 1,
    "username": "admin",
    "email": "admin@repairsystem.com",
    "roleId": 1,
    "roleName": "Admin",
    "permissions": ["ViewUsers", "CreateUser", ...],
    "isActive": true
  }
}
```

### 2. Authenticated Requests
```
GET /api/users
Authorization: Bearer {accessToken}
```

### 3. Refresh Token
```
POST /api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "R2FtZm..."
}
```

### 4. Logout
```
POST /api/auth/logout
Authorization: Bearer {accessToken}
```

## M?c ??nh Roles và Permissions

### Admin Role
- Toàn b? quy?n trên t?t c? modules

### Technician Role
- ViewRepairs, CreateRepair, EditRepair, ApproveRepair
- ViewInventory, ManageInventory
- ViewCustomers, ManageCustomers

### Customer Role
- ViewRepairs
- CreateRepair

## API Endpoints

### Authentication
- `POST /api/auth/login` - Login
- `POST /api/auth/register` - Register new user
- `POST /api/auth/refresh-token` - Refresh access token
- `POST /api/auth/logout` - Logout
- `GET /api/auth/me` - Get current user info
- `POST /api/auth/change-password` - Change password

### Users
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users/username/{username}` - Get user by username
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user
- `POST /api/users/{id}/role/{roleId}` - Assign role to user
- `PUT /api/users/{id}/status` - Update user status

### Roles
- `GET /api/roles` - Get all roles
- `GET /api/roles/{id}` - Get role by ID
- `POST /api/roles` - Create new role
- `PUT /api/roles/{id}` - Update role
- `DELETE /api/roles/{id}` - Delete role
- `POST /api/roles/{id}/permissions` - Assign permissions to role

### Permissions
- `GET /api/permissions` - Get all permissions
- `GET /api/permissions/{id}` - Get permission by ID
- `GET /api/permissions/module/{module}` - Get permissions by module
- `GET /api/permissions/role/{roleId}` - Get role permissions
- `POST /api/permissions` - Create new permission
- `PUT /api/permissions/{id}` - Update permission
- `DELETE /api/permissions/{id}` - Delete permission

## Seed Data

?ng d?ng t? ??ng kh?i t?o các d? li?u ban ??u khi kh?i ??ng:

- **3 Roles**: Admin, Technician, Customer
- **20 Permissions**: Theo 6 modules (Users, Roles, Permissions, Inventory, Repair, Customer)
- **1 Admin User**: 
  - Username: `admin`
  - Password: `Admin@123`
  - Email: `admin@repairsystem.com`

## L?i Handling

API tr? v? các HTTP status codes tiêu chu?n:
- `200 OK` - Request thành công
- `201 Created` - Resource ???c t?o thành công
- `400 Bad Request` - Input không h?p l?
- `401 Unauthorized` - Không ???c xác th?c
- `403 Forbidden` - Không có quy?n truy c?p
- `404 Not Found` - Resource không tìm th?y
- `500 Internal Server Error` - L?i server

## Security

### Password Security
- M?t kh?u ???c hash b?ng SHA256 v?i salt
- Salt ???c l?u tr? cùng v?i hash

### Token Security
- JWT tokens ???c ký b?ng secret key
- Access tokens h?t h?n sau 15 phút
- Refresh tokens h?t h?n sau 7 ngày
- Tokens ???c xác th?c trên m?i request

### Authorization
- Permission-based access control
- Custom `[RequirePermission]` attribute
- Role-based route authorization

## Phát tri?n ti?p theo

### Features c?n thêm
1. Email verification
2. Two-factor authentication (2FA)
3. Audit logging
4. API rate limiting
5. Caching layer (Redis)
6. Search functionality
7. Pagination support
8. Filter/sort capabilities
9. File upload/download
10. Webhook notifications

### Performance Optimization
1. Add database indexing
2. Implement caching
3. Add query optimization
4. Implement async operations
5. Add pagination

### Testing
1. Unit tests
2. Integration tests
3. API endpoint tests
4. Security tests

## Build & Run

```bash
# Clone repository
git clone <repository-url>

# Navigate to solution directory
cd RepairSystem

# Restore packages
dotnet restore

# Build solution
dotnet build

# Run migrations (if needed)
dotnet ef database update --project RepairSystem

# Run application
dotnet run --project RepairSystem
```

## Database Migrations

```bash
# Create migration
dotnet ef migrations add <MigrationName> --project RepairSystem.Data

# Update database
dotnet ef database update --project RepairSystem.Data

# Remove last migration
dotnet ef migrations remove --project RepairSystem.Data
```

## Dependencies

- Microsoft.EntityFrameworkCore 8.0.14
- Microsoft.EntityFrameworkCore.SqlServer 8.0.14
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0
- System.IdentityModel.Tokens.Jwt 7.1.2
- Microsoft.IdentityModel.Tokens 7.1.2
- AutoMapper 8.0.0
- Swashbuckle.AspNetCore 6.6.2
- CloudinaryDotNet 1.29.3

## License

MIT License

## Support

?? báo cáo l?i ho?c yêu c?u tính n?ng, vui lòng t?o issue trên repository.
