# Issue #58 - Staging Deployment, Smoke Test & Health Checks

## 1. Mục tiêu
Cung cấp cấu hình triển khai môi trường Staging qua Docker Compose, bổ sung endpoint kiểm tra sức khỏe hệ thống (`/health` và `/api/v1/health`) xác nhận kết nối trực tiếp tới MySQL Database, cùng kịch bản Smoke Test tự động.

## 2. Health Check Endpoints
- `GET /health` và `GET /api/v1/health` (`[AllowAnonymous]`)
- **HTTP 200 OK** khi API và Database hoạt động bình thường:
  ```json
  {
    "status": "Healthy",
    "api": "Healthy",
    "database": "Healthy",
    "timestampUtc": "2026-11-22T16:00:00Z"
  }
  ```
- **HTTP 503 Service Unavailable** khi Database mất kết nối.

## 3. Khởi chạy môi trường Staging & Smoke Test
```bash
# Khởi chạy Docker Compose với cấu hình Staging
docker compose -f docker-compose.yml -f docker-compose.staging.yml up --build -d

# Chạy kịch bản Smoke Test kiểm tra sức khỏe toàn hệ thống
pwsh ./scripts/smoke-test.ps1
```