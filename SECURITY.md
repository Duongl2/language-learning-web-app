# Chính sách Bảo mật (SECURITY POLICY)

## 1. Phạm vi Phiên bản Hỗ trợ

| Phiên bản | Hỗ trợ bảo mật | Ghi chú |
|---|---|---|
| `v1.0.x` | Được hỗ trợ | Bản phát hành MVP chính thức |
| `< v1.0.0` | Không còn hỗ trợ | Các bản tiền phát hành trong giai đoạn phát triển |

## 2. Quy trình Báo cáo Lỗ hổng Bảo mật

Nếu phát hiện bất kỳ lỗ hổng bảo mật nào liên quan đến dự án (xác thực JWT, leo thang đặc quyền, SQL/CSV Injection, rò rỉ dữ liệu người dùng):
1. **Không mở Issue công khai trên GitHub.**
2. Gửi báo cáo trực tiếp cho nhóm quản trị dự án (Leader / Maintainer) qua kênh liên lạc nội bộ hoặc tính năng *Report a vulnerability* trên tab Security của GitHub Repository.
3. Nội dung báo cáo cần đính kèm:
   - Mô tả chi tiết lỗ hổng và endpoint/component bị ảnh hưởng.
   - Các bước tái hiện (Proof of Concept).
   - Đánh giá mức độ ảnh hưởng và đề xuất hướng khắc phục (nếu có).

## 3. Các Cơ chế Bảo mật Đã Triển khai trong v1.0.0

- **Quản lý Bí mật (Secrets Management):** Không lưu trữ `.env`, mật khẩu cơ sở dữ liệu thật hoặc khóa ký JWT trong mã nguồn; tích hợp GitGuardian quét tự động trên mọi Pull Request.
- **Xác thực & Mật khẩu:** Mật khẩu người dùng được băm bằng thuật toán PBKDF2 kèm salt ngẫu nhiên; Refresh Token chỉ lưu dưới dạng mã băm SHA-256 trong cơ sở dữ liệu và hỗ trợ thu hồi (Revocation) ngay khi đăng xuất.
- **Phân quyền & Cô lập Tài nguyên (RBAC & Ownership):** Mọi endpoint `/api/v1/admin/*` bắt buộc chính sách `AdminOnly`; mọi thao tác đọc/ghi trên bộ thẻ riêng tư, thẻ học, tiến độ và đồng bộ đều kiểm tra chặt chẽ `OwnerId == CurrentUserId` từ JWT claims.
- **An toàn Dữ liệu CSV & Quiz:** Dữ liệu xuất CSV tự động trung hòa các ký tự công thức (`=`, `+`, `-`, `@`) chống CSV Formula Injection (CWE-1236); API câu hỏi Quiz loại bỏ hoàn toàn đáp án đúng trước khi gửi về trình duyệt.
- **An toàn Ngoại tuyến & Đồng bộ:** Không lưu mật khẩu trong IndexedDB; sổ cái `sync_operations` kết hợp mã băm SHA-256 của canonical payload để ngăn chặn tấn công phát lại (replay/duplicate) và phát hiện xung đột dữ liệu.