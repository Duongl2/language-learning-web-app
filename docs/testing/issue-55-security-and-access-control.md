# Issue #55 - Security and Access-Control Tests

## 1. Mục tiêu
Kiểm thử bảo mật, xác thực (Authentication), phân quyền theo vai trò (Role-Based Access Control - RBAC), cô lập quyền sở hữu tài nguyên (Resource Ownership Isolation) và chống rò rỉ dữ liệu nhạy cảm trên toàn bộ hệ thống REST API.

## 2. Ma trận Kiểm thử Bảo mật (Security Test Matrix)

| Nhóm kiểm thử | Kịch bản kiểm thử | Endpoint đại diện | Kỳ vọng | Kết quả |
|---|---|---|---|---|
| **RBAC / Admin Guard** | Người dùng chưa đăng nhập gọi API Admin | `GET /api/v1/admin/*` | `401 Unauthorized` | **Pass** |
| **RBAC / Admin Guard** | Tài khoản `Learner` gọi API quản trị nội dung hoặc moderation | `GET /api/v1/admin/courses`, `POST /api/v1/admin/decks/{id}/hide` | `403 Forbidden` | **Pass** |
| **RBAC / Admin Guard** | Tài khoản `Admin` gọi API quản trị | `GET /api/v1/admin/access` | `204 No Content` | **Pass** |
| **Resource Ownership** | `Learner B` xem/sửa/xóa bộ thẻ Private của `Learner A` | `GET/PATCH/DELETE /api/v1/decks/{id}` | `404 Not Found` | **Pass** |
| **Resource Ownership** | `Learner B` sửa bộ thẻ Public hoặc thêm/sửa/xóa thẻ của `Learner A` | `POST/PATCH/DELETE /api/v1/decks/{id}/cards` | `404 Not Found` | **Pass** |
| **Resource Ownership** | `Learner B` cố tình clone bộ thẻ Private của `Learner A` | `POST /api/v1/public/decks/{id}/clone` | `404 Not Found` | **Pass** |
| **Data Leak Prevention** | Kiểm tra response của `register`, `login`, `me` | `/api/v1/auth/*`, `/api/v1/me` | Không chứa `password`, `passwordHash`, `tokenHash` | **Pass** |
| **Quiz Anti-Cheat** | Kiểm tra response đề bài Quiz trước khi nộp bài | `GET /api/v1/lessons/{id}/quiz` | Không chứa `isCorrect`, `explanation` | **Pass** |

## 3. Lệnh chạy kiểm thử
```bash
dotnet test backend/LanguageLearning.sln --filter "FullyQualifiedName~SecurityAccessControlTests"
```