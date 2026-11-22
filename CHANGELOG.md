# Nhật ký Thay đổi Phiên bản (CHANGELOG)

Tất cả những thay đổi đáng chú ý của dự án được ghi nhận tại tài liệu này theo chuẩn [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) và tuân thủ [Semantic Versioning](https://semver.org/).

---

## [v1.0.0] - 2026-11-22

### Added
- **Authentication & Profile (M01):** Đăng ký, đăng nhập, băm mật khẩu PBKDF2, JWT Access/Refresh token rotation, thu hồi phiên đăng xuất và quản lý hồ sơ múi giờ cá nhân (#10, #11, #12, #13).
- **Courses, Lessons & Vocabulary (M02, M03, M16):** Hệ thống quản trị nội dung phân cấp `Language -> Course -> Unit -> Lesson -> Vocabulary`, kiểm soát xuất bản `Draft/Published`, danh mục khóa học, trình học bài `Lesson Viewer` kèm từ vựng và điều hướng bài trước/sau (#14, #15, #16, #17, #18).
- **Decks, Flashcards & Optimistic Concurrency (M04, M05):** CRUD bộ thẻ, thẻ học, gắn nhãn tags, giao diện lật thẻ phím tắt và kiểm soát phiên bản `BaseVersion` chống ghi đè dữ liệu cũ giữa nhiều thiết bị (#20, #21, #22, #23, #48, #50).
- **Spaced Repetition System - SRS v1 (M06):** Thuật toán lặp lại ngắt quãng `srs-v1`, truy vấn thẻ đến hạn, giao diện phiên ôn tập và xử lý nộp kết quả ôn tập bất biến, idempotent qua `ClientEventId` (#24, #25, #26, #27).
- **Quiz & Learning Progress (M07, M08):** API và giao diện làm bài Quiz (trắc nghiệm, điền từ, nối cặp) chấm điểm phía Server, theo dõi tiến độ khóa học và nút tiếp tục học bài gần nhất (#28, #29, #30, #31, #32).
- **CSV Import & Export (M09):** Tải mẫu CSV chuẩn UTF-8 BOM, xem trước và kiểm tra lỗi theo dòng, nhập thẻ an toàn trong Transaction và xuất CSV chống Formula Injection (#33, #34).
- **Statistics, Heatmap & Timezone Streak (M11, M12):** Bảng điều khiển thống kê học tập, biểu đồ Heatmap 365 ngày, mục tiêu học hằng ngày và tính chuỗi ngày học liên tục (Streak) theo múi giờ người dùng (#37, #38, #39, #40).
- **Community Sharing & Moderation (M13, M16):** Thư viện bộ thẻ công khai, xem trước 5 thẻ mẫu, sao chép (Clone) bộ thẻ độc lập và tính năng Admin ẩn bộ thẻ vi phạm có ghi nhận `audit_logs` (#41, #42, #43).
- **Offline PWA & Two-Way Synchronization (M14, M15):** Cấu hình PWA Service Worker, tải trước bộ thẻ vào IndexedDB, hàng đợi ôn tập ngoại tuyến `syncQueue`, đồng bộ `/api/v1/sync/push` với sổ cái Idempotency `sync_operations`, đồng bộ `/api/v1/sync/pull` theo cursor và tự động retry exponential backoff (#44, #45, #46, #47, #48, #49).
- **Quality Assurance, Security & Operations:** Bộ kiểm thử Unit, MySQL Integration, Vitest Component, Playwright E2E, kiểm thử phân quyền bảo mật, endpoint `/health` và bộ công cụ sao lưu/phục hồi MySQL (#53, #54, #55, #56, #58, #59).

---

## [v0.1.0-alpha] - 2026-10-13

### Added
- Khởi tạo cấu trúc dự án Monorepo (React PWA + ASP.NET Core Web API + MySQL).
- Thiết lập quy định mã nguồn mở: `README.md`, `LICENSE`, `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `SECURITY.md`.
- Thiết lập cấu hình môi trường `.editorconfig`, `.gitignore`, `.env.example`, `docker-compose.yml`.
- Thiết lập quy trình GitHub: GitHub Actions CI workflows, Issue Templates, Pull Request Template, `CODEOWNERS`.