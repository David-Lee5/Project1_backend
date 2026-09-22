# API Routes Documentation

## Authentication Routes

### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "string",
  "password": "string"
}

Response: 200 OK
{
  "success": true,
  "message": "Login successful",
  "token": {
    "accessToken": "string",
    "refreshToken": "string",
    "expiresIn": "2024-01-15T10:15:00Z"
  },
  "user": {
    "taiKhoanId": 1,
    "username": "string",
    "email": "string",
    "roleId": 1,
    "roleName": "string",
    "permissions": ["string"],
    "isActive": true
  }
}
```

### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "string",
  "email": "string",
  "password": "string",
  "confirmPassword": "string",
  "roleId": 1
}

Response: 200 OK
{
  "success": true,
  "message": "User registered successfully",
  "data": true
}
```

### Refresh Token
```http
POST /api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "string"
}

Response: 200 OK
{
  "success": true,
  "message": "Token refreshed successfully",
  "data": {
    "accessToken": "string",
    "refreshToken": "string",
    "expiresIn": "2024-01-15T10:15:00Z"
  }
}
```

### Logout
```http
POST /api/auth/logout
Authorization: Bearer {accessToken}

Response: 200 OK
{
  "success": true,
  "message": "Logout successful",
  "data": true
}
```

### Get Current User
```http
GET /api/auth/me
Authorization: Bearer {accessToken}

Response: 200 OK
{
  "success": true,
  "message": "Success",
  "data": {
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

### Change Password
```http
POST /api/auth/change-password
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "oldPassword": "string",
  "newPassword": "string",
  "confirmPassword": "string"
}

Response: 200 OK
{
  "success": true,
  "message": "Password changed successfully",
  "data": true
}
```

---

## User Management Routes

### Get All Users
```http
GET /api/users
Authorization: Bearer {accessToken}
Required Permission: ViewUsers

Response: 200 OK
{
  "success": true,
  "message": "Success",
  "data": [
    {
      "taiKhoanId": 1,
      "username": "admin",
      "email": "admin@repairsystem.com",
      "roleId": 1,
      "roleName": "Admin",
      "permissions": ["string"],
      "isActive": true
    }
  ]
}
```

### Get User by ID
```http
GET /api/users/{id}
Authorization: Bearer {accessToken}
Required Permission: ViewUsers

Response: 200 OK
{
  "success": true,
  "message": "Success",
  "data": {
    "taiKhoanId": 1,
    "username": "string",
    "email": "string",
    "roleId": 1,
    "roleName": "string",
    "permissions": ["string"],
    "isActive": true
  }
}
```

### Get User by Username
```http
GET /api/users/username/{username}
Authorization: Bearer {accessToken}
Required Permission: ViewUsers

Response: 200 OK
{
  "success": true,
  "message": "Success",
  "data": { ... }
}
```

### Create User
```http
POST /api/users
Authorization: Bearer {accessToken}
Required Permission: CreateUser
Content-Type: application/json

{
  "username": "string",
  "email": "string",
  "password": "string",
  "roleId": 1
}

Response: 201 Created
{
  "success": true,
  "message": "User created successfully",
  "data": { ... }
}
```

### Update User
```http
PUT /api/users/{id}
Authorization: Bearer {accessToken}
Required Permission: EditUser
Content-Type: application/json

{
  "username": "string",
  "email": "string",
  "roleId": 1
}

Response: 200 OK
{
  "success": true,
  "message": "User updated successfully",
  "data": { ... }
}
```

### Delete User
```http
DELETE /api/users/{id}
Authorization: Bearer {accessToken}
Required Permission: DeleteUser

Response: 200 OK
{
  "success": true,
  "message": "User deleted successfully",
  "data": true
}
```

### Assign Role to User
```http
POST /api/users/{id}/role/{roleId}
Authorization: Bearer {accessToken}
Required Permission: EditUser

Response: 200 OK
{
  "success": true,
  "message": "Role assigned successfully",
  "data": true
}
```

### Update User Status
```http
PUT /api/users/{id}/status
Authorization: Bearer {accessToken}
Required Permission: EditUser
Content-Type: application/json

{
  "isActive": true
}

Response: 200 OK
{
  "success": true,
  "message": "User status updated successfully",
  "data": true
}
```

---

## Role Management Routes

### Get All Roles
```http
GET /api/roles
Authorization: Bearer {accessToken}
Required Permission: ViewRoles

Response: 200 OK
{
  "success": true,
  "message": "Success",
  "data": [
    {
      "id": 1,
      "name": "Admin",
      "description": "string",
      "isActive": true,
      "permissions": [
        {
          "id": 1,
          "name": "string",
          "description": "string",
          "module": "string",
          "isActive": true
        }
      ]
    }
  ]
}
```

### Get Role by ID
```http
GET /api/roles/{id}
Authorization: Bearer {accessToken}
Required Permission: ViewRoles

Response: 200 OK
{
  "success": true,
  "message": "Success",
  "data": { ... }
}
```

### Create Role
```http
POST /api/roles
Authorization: Bearer {accessToken}
Required Permission: CreateRole
Content-Type: application/json

{
  "name": "string",
  "description": "string",
  "isActive": true
}

Response: 201 Created
{
  "success": true,
  "message": "Role created successfully",
  "data": { ... }
}
```

### Update Role
```http
PUT /api/roles/{id}
Authorization: Bearer {accessToken}
Required Permission: EditRole
Content-Type: application/json

{
  "name": "string",
  "description": "string",
  "isActive": true
}

Response: 200 OK
{
  "success": true,
  "message": "Role updated successfully",
  "data": { ... }
}
```

### Delete Role
```http
DELETE /api/roles/{id}
Authorization: Bearer {accessToken}
Required Permission: DeleteRole

Response: 200 OK
{
  "success": true,
  "message": "Role deleted successfully",
  "data": true
}
```

### Assign Permissions to Role
```http
POST /api/roles/{id}/permissions
Authorization: Bearer {accessToken}
Required Permission: EditRole
Content-Type: application/json

[1, 2, 3]  // Array of permission IDs

Response: 200 OK
{
  "success": true,
  "message": "Permissions assigned successfully",
  "data": true
}
```

---

## Permission Management Routes

### Get All Permissions
```http
GET /api/permissions
Authorization: Bearer {accessToken}
Required Permission: ViewPermissions

Response: 200 OK
{
  "success": true,
  "message": "Success",
  "data": [
    {
      "id": 1,
      "name": "ViewUsers",
      "description": "string",
      "module": "Users",
      "isActive": true
    }
  ]
}
```

### Get Permission by ID
```http
GET /api/permissions/{id}
Authorization: Bearer {accessToken}
Required Permission: ViewPermissions

Response: 200 OK
{
  "success": true,
  "message": "Success",
  "data": { ... }
}
```

### Get Permissions by Module
```http
GET /api/permissions/module/{module}
Authorization: Bearer {accessToken}
Required Permission: ViewPermissions

Response: 200 OK
{
  "success": true,
  "message": "Success",
  "data": [ ... ]
}

// Modules: Users, Roles, Permissions, Inventory, Repair, Customer
```

### Get Role Permissions
```http
GET /api/permissions/role/{roleId}
Authorization: Bearer {accessToken}
Required Permission: ViewPermissions

Response: 200 OK
{
  "success": true,
  "message": "Success",
  "data": [ ... ]
}
```

### Create Permission
```http
POST /api/permissions
Authorization: Bearer {accessToken}
Required Permission: CreatePermission
Content-Type: application/json

{
  "name": "string",
  "description": "string",
  "module": "Users",
  "isActive": true
}

Response: 201 Created
{
  "success": true,
  "message": "Permission created successfully",
  "data": { ... }
}
```

### Update Permission
```http
PUT /api/permissions/{id}
Authorization: Bearer {accessToken}
Required Permission: EditPermission
Content-Type: application/json

{
  "name": "string",
  "description": "string",
  "module": "string",
  "isActive": true
}

Response: 200 OK
{
  "success": true,
  "message": "Permission updated successfully",
  "data": { ... }
}
```

### Delete Permission
```http
DELETE /api/permissions/{id}
Authorization: Bearer {accessToken}
Required Permission: DeletePermission

Response: 200 OK
{
  "success": true,
  "message": "Permission deleted successfully",
  "data": true
}
```

---

## Error Responses

### Unauthorized (401)
```json
{
  "success": false,
  "message": "Invalid username or password",
  "data": null,
  "errors": []
}
```

### Forbidden (403)
```json
{
  "success": false,
  "message": "You don't have permission to access this resource",
  "data": null,
  "errors": []
}
```

### Not Found (404)
```json
{
  "success": false,
  "message": "User not found",
  "data": null,
  "errors": []
}
```

### Bad Request (400)
```json
{
  "success": false,
  "message": "Invalid input",
  "data": null,
  "errors": ["Username is required", "Email format is invalid"]
}
```

### Internal Server Error (500)
```json
{
  "success": false,
  "message": "An unexpected error occurred",
  "data": null,
  "errors": ["Error details..."]
}
```

---

## Status Codes

| Code | Meaning |
|------|---------|
| 200 | OK |
| 201 | Created |
| 400 | Bad Request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 500 | Internal Server Error |

---

## Authentication Header Format

```
Authorization: Bearer {accessToken}
```

Example:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## Modules & Permissions

### Users Module
- ViewUsers
- CreateUser
- EditUser
- DeleteUser

### Roles Module
- ViewRoles
- CreateRole
- EditRole
- DeleteRole

### Permissions Module
- ViewPermissions
- CreatePermission
- EditPermission
- DeletePermission

### Inventory Module
- ViewInventory
- ManageInventory

### Repair Module
- ViewRepairs
- CreateRepair
- EditRepair
- ApproveRepair

### Customer Module
- ViewCustomers
- ManageCustomers
