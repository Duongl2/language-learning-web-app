# Kịch bản Báo cáo & Demo Sản phẩm v1.0.0 (15 - 20 Phút)

## Phần 1: Demo Sản phẩm Thực tế (12 - 15 Phút)

| Bước | Thời lượng | Người thực hiện | Thao tác trên Giao diện | Điểm nhấn Kỹ thuật cần thuyết minh |
|---|---|---|---|---|
| **1. Đăng ký, Đăng nhập & Hồ sơ** | 2 phút | Leader | Đăng nhập tài khoản Learner, mở trang `/profile` cập nhật múi giờ `Asia/Ho_Chi_Minh`. | JWT Access Token + Refresh Token xoay vòng (Rotation), băm mật khẩu PBKDF2, cấu hình IANA Timezone. |
| **2. Khám phá Khóa học & Học bài** | 2 phút | Maintainer | Mở `/courses`, lọc theo ngôn ngữ và trình độ A1, xem chi tiết Units/Lessons, mở `Lesson Viewer` nghe phát âm từ vựng và chuyển bài. | Cấu trúc phân cấp `Course -> Unit -> Lesson -> Vocabulary`, chỉ hiển thị nội dung đã `Published`. |
| **3. Làm bài Quiz & Xem Tiến độ** | 2 phút | Contributor | Bấm "Làm Quiz" cuối bài học, trả lời câu hỏi trắc nghiệm/điền từ/ghép cặp, nộp bài và xem giải thích chi tiết. Mở trang `/learning` kiểm tra nút "Tiếp tục học". | API câu hỏi ẩn toàn bộ `isCorrect` trước khi nộp; chấm điểm Server-side có `ClientAttemptId` chống nộp trùng. |
| **4. Quản lý Bộ thẻ & Nhập/Xuất CSV** | 2 phút | Core Developer | Tạo bộ thẻ mới, tải file CSV mẫu, tải lên file CSV xem trước (Preview) dòng hợp lệ/lỗi, xác nhận Import và thử Export CSV. | Kiểm tra UTF-8, giới hạn kích thước, Transaction all-or-nothing và escape ký tự `=`, `+`, `-`, `@` chống CSV Formula Injection (TC-08, TC-09). |
| **5. Ôn tập Flashcard SRS & Thống kê/Streak** | 2 phút | Leader | Mở phiên ôn tập thẻ đến hạn, dùng phím tắt `Space` để lật thẻ, bấm phím `1-4` chọn `Again/Hard/Good/Easy`. Xem Dashboard Heatmap và huy hiệu Streak. | Thuật toán `srs-v1`, tính khoảng cách ôn tập tiếp theo, Heatmap 365 ngày và tính chuỗi Streak qua nửa đêm theo múi giờ người dùng. |
| **6. Thư viện Cộng đồng & Clone Bộ thẻ** | 1.5 phút | Contributor | Mở `/community`, tìm kiếm bộ thẻ công khai, bấm "Xem trước" 5 thẻ mẫu và bấm "Sao chép về bộ thẻ của tôi". | Bản sao độc lập hoàn toàn với GUID mới và quyền sở hữu riêng, không ảnh hưởng bộ thẻ gốc (TC-11). |
| **7. Học Ngoại tuyến (PWA) & Đồng bộ Idempotent** | 2.5 phút | Core Developer & Contributor | Bấm tải bộ thẻ về học offline, bật chế độ Offline trong DevTools, ôn tập 2 thẻ, F5 tải lại trang. Bật lại mạng Online để tự động đồng bộ. | PWA Service Worker cache App Shell, IndexedDB (Dexie) lưu `syncQueue`, tự động retry exponential backoff và đối chiếu sổ cái `sync_operations` chống tạo bản ghi trùng (TC-14, TC-15, TC-16). |
| **8. Cổng Quản trị viên (Admin)** | 1 phút | Maintainer | Đăng nhập tài khoản Admin, xuất bản khóa học và ẩn bộ thẻ công khai vi phạm. | RBAC `AdminOnly`, mọi thao tác kiểm duyệt đều tự động ghi vết vào bảng `audit_logs` (TC-02, TC-04, TC-20). |

---

## Phần 2: Demo Quy trình Cộng tác Mã nguồn mở trên GitHub (5 - 8 Phút)

1. **GitHub Projects & Milestones:** Trình bày bảng Kanban 62 Issues chia đều cho 4 Milestones (`v0.1.0 Foundation`, `v0.2.0 Learning Engine`, `v0.3.0 Extended Learning`, `v1.0.0 MVP Release`).
2. **Traceability (Issue -> Branch -> Commit -> PR -> Review):**
   - Mở Issue mẫu có đầy đủ Tiêu chí nghiệm thu (Acceptance Criteria).
   - Mở Pull Request liên kết `Closes #XX`, chỉ rõ lịch sử Code Review chéo giữa các thành viên, quá trình phản hồi và đẩy commit sửa lỗi.
3. **CI/CD & Kiểm thử tự động:** Trình bày GitHub Actions workflows (`backend-ci.yml`, `frontend-ci.yml`, GitGuardian Security Checks) kiểm tra build 0 warning và chạy bộ test Unit/Integration/E2E.
4. **Tài liệu & Phát hành:** Giới thiệu `README.md`, `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `SECURITY.md`, `CHANGELOG.md` và trang GitHub Release `v1.0.0`.