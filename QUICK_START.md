# Quick Start Guide

## ?? 5 Minutes Setup

### Prerequisites
- .NET 8 SDK installed
- SQL Server running
- Visual Studio 2022 or VS Code

### Step 1: Clone & Setup
```bash
git clone <repository-url>
cd RepairSystem
dotnet restore
```

### Step 2: Configure Database
Edit `RepairSystem/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=YOUR_SERVER\\INSTANCE;Database=RepairsSystem;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### Step 3: Run Application
```bash
dotnet run --project RepairSystem
```

### Step 4: Access API
- Swagger UI: `https://localhost:5001/swagger`
- Admin Login:
  - Username: `admin`
  - Password: `Admin@123`

---

## ?? Logging In

### Using Swagger UI
1. Click "Authorize" button
2. Click "Try it out" on Login endpoint
3. Enter credentials:
   ```json
   {
     "username": "admin",
     "password": "Admin@123"
   }
   ```
4. Copy the `accessToken` from response
5. Paste in Authorization header (Bearer token)

### Using cURL
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin@123"
  }'
```

### Using Postman
1. Create POST request to `https://localhost:5001/api/auth/login`
2. Body (raw JSON):
```json
{
  "username": "admin",
  "password": "Admin@123"
}
```
3. Send request
4. Copy `accessToken` from response
5. Add Authorization header:
   - Type: Bearer Token
   - Token: `{accessToken}`

---

## ?? Creating Your First User

### Using Swagger
1. Get Access Token (see above)
2. Authorize with token
3. Find "POST /api/users" endpoint
4. Click "Try it out"
5. Enter:
```json
{
  "username": "technician1",
  "email": "tech@example.com",
  "password": "TechPassword123!",
  "roleId": 2
}
```
6. Send

### Using cURL
```bash
TOKEN="your-access-token"

curl -X POST "https://localhost:5001/api/users" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "technician1",
    "email": "tech@example.com",
    "password": "TechPassword123!",
    "roleId": 2
  }'
```

---

## ?? Available Roles

| Role ID | Role Name | Purpose |
|---------|-----------|---------|
| 1 | Admin | Full system access |
| 2 | Technician | Repair & inventory management |
| 3 | Customer | Submit & track repairs |

---

## ?? Important Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | User login |
| POST | `/api/auth/register` | Register new user |
| GET | `/api/auth/me` | Current user info |
| GET | `/api/users` | List all users |
| POST | `/api/users` | Create user |
| GET | `/api/roles` | List roles |
| GET | `/api/permissions` | List permissions |

---

## ?? Common Issues

### Issue: "Connection refused"
**Solution:** Check SQL Server is running
```bash
# SQL Server isn't running?
# Windows: Open Services and start SQL Server
# Or use SQL Server Management Studio
```

### Issue: "Invalid token"
**Solution:** Token may have expired
```bash
# Get a new token by logging in again
POST /api/auth/login
```

### Issue: "Permission denied"
**Solution:** User doesn't have required permission
```bash
# Admin user should have all permissions
# Check role assignments
```

### Issue: "Database not found"
**Solution:** Update connection string in appsettings.json
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=.;Database=RepairsSystem;..."
}
```

---

## ?? Tips

### Development Tips
- Use Swagger UI for testing endpoints
- Check logs in VS Output window for errors
- Use Postman for complex scenarios
- Use `dotnet watch run` for auto-reload

### API Testing
```bash
# Get all users
curl -X GET "https://localhost:5001/api/users" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Create role
curl -X POST "https://localhost:5001/api/roles" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Manager",
    "description": "Manager role",
    "isActive": true
  }'

# Assign permissions to role
curl -X POST "https://localhost:5001/api/roles/1/permissions" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '[1, 2, 3]'
```

---

## ?? Security Quick Tips

1. **Change admin password immediately**
   ```bash
   POST /api/auth/change-password
   {
     "oldPassword": "Admin@123",
     "newPassword": "NewSecurePassword123!",
     "confirmPassword": "NewSecurePassword123!"
   }
   ```

2. **Update JWT Secret Key**
   - Edit `appsettings.json`
   - Change `JwtSettings.SecretKey` to 32+ character key
   - Restart application

3. **Use environment variables in production**
   - Never commit sensitive data
   - Use Azure Key Vault or similar
   - Set environment variables for secrets

---

## ?? Deployment Checklist

- [ ] Change admin password
- [ ] Update JWT secret key
- [ ] Configure correct database connection
- [ ] Enable HTTPS
- [ ] Set environment to Production
- [ ] Configure CORS for your domain
- [ ] Setup database backups
- [ ] Enable logging
- [ ] Setup monitoring
- [ ] Configure firewall rules
- [ ] Test all endpoints
- [ ] Setup SSL certificates

---

## ?? Next Steps

After successful setup:

1. **Read Full Documentation**
   - See `README.md` for overview
   - See `API_ROUTES.md` for all endpoints
   - See `SETUP.md` for detailed setup

2. **Create Test Users**
   - Create users for each role
   - Test different permission levels
   - Verify role-based access control

3. **Explore API**
   - Try all endpoints in Swagger
   - Test with Postman
   - Test with your client application

4. **Customize**
   - Add your business logic
   - Extend DTOs as needed
   - Create additional services

5. **Deploy**
   - Follow deployment checklist
   - Monitor performance
   - Setup CI/CD pipeline

---

## ?? Quick Troubleshooting

```bash
# Clear build
dotnet clean

# Restore packages
dotnet restore

# Build solution
dotnet build

# Run with logs
dotnet run --project RepairSystem --verbosity detailed

# Reset database (WARNING: deletes all data)
# Remove migrations and delete database, then:
dotnet ef migrations add InitialCreate --project RepairSystem.Data --startup-project RepairSystem
dotnet ef database update --project RepairSystem.Data --startup-project RepairSystem
```

---

## ?? Success Indicators

You've successfully setup the project when:

? Application starts without errors  
? Swagger UI is accessible  
? Can login with admin credentials  
? Can access protected endpoints with token  
? Can create users, roles, permissions  
? Permission-based access control works  

---

## ?? Need Help?

1. Check `SETUP.md` for detailed instructions
2. Check `API_ROUTES.md` for endpoint details
3. Check application logs for errors
4. Review code comments in source files
5. Check GitHub issues/discussions

---

Happy coding! ??
