# Issue #59 - Production Deployment & Backup/Restore Drill

## 1. Mục tiêu
Đóng gói cấu hình triển khai môi trường Production (`docker-compose.prod.yml`), cung cấp bộ script tự động sao lưu (`scripts/mysql-backup.ps1`) kèm kiểm tra toàn vẹn mã băm SHA-256 và phục hồi dữ liệu (`scripts/mysql-restore.ps1`), đồng thời diễn tập khôi phục dữ liệu 100% không mất mát.

## 2. Quy trình Sao lưu & Phục hồi (Backup & Restore)

### 2.1. Sao lưu tự động (Backup)
```powershell
pwsh ./scripts/mysql-backup.ps1 -ContainerName language_learning_mysql -Database language_learning_db -OutputDir ./backups
```
- Sử dụng `mysqldump --single-transaction --routines --triggers` đảm bảo tính nhất quán dữ liệu InnoDB ngay khi hệ thống đang hoạt động.
- Sinh file kiểm tra toàn vẹn `.sha256` đi kèm mỗi bản sao lưu.

### 2.2. Phục hồi dữ liệu (Restore)
```powershell
pwsh ./scripts/mysql-restore.ps1 -BackupFile ./backups/language_learning_db_backup_20261122-160000.sql
```
- Tự động đối chiếu mã băm SHA-256 trước khi nạp lại vào MySQL.

## 3. Kết quả Diễn tập Khôi phục (Backup/Restore Drill Report)
- **Kịch bản diễn tập:** Tạo người dùng, bộ thẻ, thẻ học và lịch sử ôn tập (`ReviewEvent`), thực hiện snapshot backup, mô phỏng sự cố mất dữ liệu, sau đó khôi phục lại từ bản backup.
- **Kiểm thử tự động xác nhận:** `BackupRestoreDrillTests` kiểm chứng toàn bộ định danh GUID, `Version` và lịch sử `ReviewEvent` được khôi phục nguyên vẹn 100% không mất dữ liệu.