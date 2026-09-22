# ?? RepairSystem Web API

> A complete, production-ready REST API for device repair management system built with .NET 8, featuring JWT authentication, role-based access control, and comprehensive permission management.

![Build Status](https://img.shields.io/badge/Build-?%20Passing-brightgreen)
![Version](https://img.shields.io/badge/Version-1.0-blue)
![Framework](https://img.shields.io/badge/.NET-8.0-purple)
![License](https://img.shields.io/badge/License-MIT-green)

---

## ?? Documentation Quick Links

?? **[QUICK START](QUICK_START.md)** - 5 minute setup  
?? **[DOCUMENTATION INDEX](DOCUMENTATION_INDEX.md)** - Complete documentation guide  
?? **[API ROUTES](API_ROUTES.md)** - All 27 endpoints documented  
?? **[SETUP GUIDE](SETUP.md)** - Detailed setup instructions  
?? **[PROJECT STATUS](PROJECT_COMPLETION.md)** - What's included & roadmap  

---

## ? Features

### ?? Authentication & Authorization
- JWT Token-based authentication
- Access tokens (15 minutes) + Refresh tokens (7 days)
- Role-based access control (RBAC)
- Permission-based authorization
- Secure password hashing (SHA256 + Salt)
- Custom permission attributes

### ?? User Management
- User registration & login
- User profile management
- Change password
- User status management (active/inactive)
- Assign roles to users

### ?? Role Management
- Create/Read/Update/Delete roles
- Assign permissions to roles
- 3 predefined roles (Admin, Technician, Customer)

### ?? Permission Management
- 20+ predefined permissions
- Organized in 6 modules (Users, Roles, Permissions, Inventory, Repair, Customer)
- Granular access control
- Module-based permission queries

### ?? API Endpoints
- **27 Total Endpoints**
  - 6 Authentication endpoints
  - 8 User management endpoints
  - 6 Role management endpoints
  - 7 Permission management endpoints

### ?? Data Layer
- Generic Repository pattern
- Unit of Work pattern
- EF Core with SQL Server
- Database migrations support
- Seed data initialization

---

## ?? Quick Start

### Prerequisites
- .NET 8 SDK
- SQL Server
- Visual Studio 2022 or VS Code

### Installation (5 minutes)

```bash
# 1. Clone repository
git clone <repository-url>
cd RepairSystem

# 2. Restore dependencies
dotnet restore

# 3. Configure database
# Edit RepairSystem/appsettings.json
# Set your connection string in "ConnectionStrings.DefaultConnection"

# 4. Run application
dotnet run --project RepairSystem
```

### Access the API

- **Swagger UI**: https://localhost:5001/swagger
- **API Base URL**: https://localhost:5001/api

### Default Admin Account
```
Username: admin
Password: Admin@123
```

---

## ?? API Endpoints Overview

### Authentication (6 endpoints)
```http
POST   /api/auth/login
POST   /api/auth/register
POST   /api/auth/refresh-token
POST   /api/auth/logout
GET    /api/auth/me
POST   /api/auth/change-password
```

### User Management (8 endpoints)
```http
GET    /api/users
GET    /api/users/{id}
GET    /api/users/username/{username}
POST   /api/users
PUT    /api/users/{id}
DELETE /api/users/{id}
POST   /api/users/{id}/role/{roleId}
PUT    /api/users/{id}/status
```

### Role Management (6 endpoints)
```http
GET    /api/roles
GET    /api/roles/{id}
POST   /api/roles
PUT    /api/roles/{id}
DELETE /api/roles/{id}
POST   /api/roles/{id}/permissions
```

### Permission Management (7 endpoints)
```http
GET    /api/permissions
GET    /api/permissions/{id}
GET    /api/permissions/module/{module}
GET    /api/permissions/role/{roleId}
POST   /api/permissions
PUT    /api/permissions/{id}
DELETE /api/permissions/{id}
```

---

## ??? Architecture

### Layered Architecture
```
Presentation Layer (Controllers)
         ?
Business Logic Layer (Services)
         ?
Data Access Layer (Repositories)
         ?
Database (SQL Server)
```

### Projects
- **RepairSystem** - Web API (ASP.NET Core)
- **RepairSystem.Service** - Business logic & Services
- **RepairSystem.Data** - Data access & EF Core
- **RepairSystem.Model** - Domain entities

---

## ?? Authentication Example

### Login
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin@123"
  }'
```

### Response
```json
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

### Use Token
```bash
curl -X GET "https://localhost:5001/api/users" \
  -H "Authorization: Bearer {accessToken}"
```

---

## ?? Default Roles & Permissions

### Roles
| Role | Access | Purpose |
|------|--------|---------|
| Admin | Full | Complete system access |
| Technician | Partial | Repair, Inventory, Customer modules |
| Customer | Limited | View/Create repairs only |

### Permission Modules
- **Users** - User account management
- **Roles** - Role management
- **Permissions** - Permission management
- **Inventory** - Spare parts & stock management
- **Repair** - Repair service management
- **Customer** - Customer information management

---

## ?? Configuration

### Database Connection
Edit `RepairSystem/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=YOUR_SERVER\\INSTANCE;Database=RepairsSystem;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### JWT Settings
```json
"JwtSettings": {
  "SecretKey": "your-secret-key-must-be-at-least-32-characters-long",
  "Issuer": "RepairSystemAPI",
  "Audience": "RepairSystemUsers",
  "ExpirationMinutes": 15
}
```

**?? In production, change the JWT SecretKey to a secure value!**

---

## ??? Technology Stack

- **Framework**: .NET 8
- **Web API**: ASP.NET Core
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT (System.IdentityModel.Tokens.Jwt)
- **ORM**: Entity Framework Core 8.0.14
- **API Documentation**: Swagger/Swashbuckle
- **Mapping**: AutoMapper

---

## ?? Documentation

| File | Purpose | Audience |
|------|---------|----------|
| [QUICK_START.md](QUICK_START.md) | 5-minute setup | Everyone |
| [README.md](README.md) | Project overview | Developers |
| [SETUP.md](SETUP.md) | Detailed setup | DevOps/Developers |
| [API_ROUTES.md](API_ROUTES.md) | All endpoints | API users |
| [CODEBASE_SUMMARY.md](CODEBASE_SUMMARY.md) | Technical details | Developers |
| [PROJECT_COMPLETION.md](PROJECT_COMPLETION.md) | Status & roadmap | Managers/Team |
| [DOCUMENTATION_INDEX.md](DOCUMENTATION_INDEX.md) | Documentation guide | Everyone |

---

## ?? Getting Started

### 1. First Time Setup
```bash
dotnet restore
# Edit appsettings.json
dotnet run --project RepairSystem
```

### 2. Login
```bash
# Open Swagger UI
https://localhost:5001/swagger

# Login with:
# Username: admin
# Password: Admin@123
```

### 3. Explore API
- Test endpoints in Swagger UI
- Read [API_ROUTES.md](API_ROUTES.md) for detailed documentation
- Check [QUICK_START.md](QUICK_START.md) for examples

### 4. Develop
- Create new controllers
- Extend services
- Add business logic

---

## ?? Security Features

? JWT Token authentication  
? Secure password hashing with salt  
? Role-based access control  
? Permission-based authorization  
? Token validation on every request  
? User status management  
? Custom authorization attributes  

**Security Checklist** (Before Production):
- [ ] Change admin password
- [ ] Update JWT SecretKey
- [ ] Configure HTTPS
- [ ] Setup database backups
- [ ] Configure firewall
- [ ] Enable logging
- [ ] Setup monitoring

---

## ?? Project Statistics

| Metric | Count |
|--------|-------|
| Total Endpoints | 27 |
| Controllers | 4 |
| Services | 5 |
| DTOs | 15+ |
| Models | 18+ |
| Permissions | 20+ |
| Roles | 3 |
| Documentation Files | 8+ |

---

## ?? What's Included

? Complete REST API with authentication  
? Role-based access control  
? 27 working API endpoints  
? Comprehensive documentation  
? Database migrations support  
? Seed data initialization  
? Error handling  
? Swagger/OpenAPI documentation  
? Best practices implementation  
? Production-ready code  

---

## ?? Build & Test

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests (when available)
dotnet test

# Run application
dotnet run --project RepairSystem

# Watch mode (auto-reload)
dotnet watch run --project RepairSystem
```

---

## ?? Support & Help

### Quick Help
- **Setup Issues?** ? See [SETUP.md](SETUP.md#troubleshooting)
- **How to use?** ? See [QUICK_START.md](QUICK_START.md)
- **API Details?** ? See [API_ROUTES.md](API_ROUTES.md)
- **Architecture?** ? See [CODEBASE_SUMMARY.md](CODEBASE_SUMMARY.md)

### Common Commands
```bash
# Setup database
dotnet ef database update --project RepairSystem.Data --startup-project RepairSystem

# Create migration
dotnet ef migrations add {Name} --project RepairSystem.Data --startup-project RepairSystem

# View logs
# Check Visual Studio Output window

# Test API
# Open https://localhost:5001/swagger
```

---

## ?? Deployment

### Prerequisites
- SQL Server configured
- .NET 8 installed
- Environment variables set
- Security configured (HTTPS, etc.)

### Deployment Steps
See [PROJECT_COMPLETION.md - Deployment Checklist](PROJECT_COMPLETION.md#-deployment-checklist)

---

## ?? Learning Resources

The codebase includes examples of:
- ? Complete service implementations
- ? Repository pattern usage
- ? JWT token generation & validation
- ? Authorization & permission checking
- ? Error handling patterns
- ? API documentation with Swagger
- ? Database migrations
- ? Dependency injection

---

## ?? Contributing

1. Review existing code
2. Follow code style
3. Implement features
4. Test thoroughly
5. Document changes
6. Submit pull request

---

## ?? Release Notes

### Version 1.0 (Current)
- ? Initial release
- ? Complete authentication system
- ? Role & permission management
- ? User management
- ? 27 API endpoints
- ? Comprehensive documentation

---

## ?? License

MIT License - Feel free to use this project for personal or commercial purposes.

---

## ?? Acknowledgments

Built with .NET 8 and best practices for:
- Clean Architecture
- SOLID Principles
- Security First
- Developer Experience

---

## ?? Contact

For issues, questions, or suggestions:
1. Review the documentation
2. Check existing code examples
3. Review git history
4. Create GitHub issues

---

## ?? Ready to Start?

1. **Quick Setup** ? [QUICK_START.md](QUICK_START.md)
2. **Full Setup** ? [SETUP.md](SETUP.md)
3. **API Reference** ? [API_ROUTES.md](API_ROUTES.md)
4. **All Docs** ? [DOCUMENTATION_INDEX.md](DOCUMENTATION_INDEX.md)

---

## ? Status

- **Build**: ? Passing
- **Version**: 1.0
- **Framework**: .NET 8
- **Status**: Production Ready
- **Last Updated**: 2024

---

**Happy Coding! ??**

For detailed instructions, see [DOCUMENTATION_INDEX.md](DOCUMENTATION_INDEX.md)
