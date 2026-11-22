# Release Notes - v1.0.0 MVP Release

**Ngày phát hành:** 22/11/2026  
**Phiên bản:** `v1.0.0`  
**Trạng thái:** Bản phát hành chính thức (General Availability - MVP 16/16 Modules)

---

## 1. Điểm nhấn Phiên bản v1.0.0
Phiên bản `v1.0.0` hoàn thiện toàn bộ **16/16 Module bắt buộc** của hệ thống ứng dụng web học ngoại ngữ mã nguồn mở theo kiến trúc **Clean Architecture** (ASP.NET Core 8 + EF Core + MySQL 8.0) và **Feature-First PWA** (React 19 + TypeScript + Tailwind CSS + Dexie IndexedDB).

## 2. Danh mục Tính năng Hoàn thiện (16 Modules)
- **M01 - Authentication & Profile:** Đăng ký, đăng nhập, PBKDF2 password hashing, JWT Access/Refresh token rotation, thu hồi token khi đăng xuất và cài đặt múi giờ cá nhân.
- **M02 - Course Catalog & Learning Path:** Duyệt khóa học theo ngôn ngữ và trình độ (A1–C2), xem lộ trình Units/Lessons và trình đọc bài học (`Lesson Viewer`) có điều hướng xuyên chương.
- **M03 - Vocabulary Management:** Quản lý từ vựng theo bài học với phiên âm IPA, giải nghĩa, ví dụ, âm thanh phát âm và hình ảnh minh họa, hỗ trợ tìm kiếm và phân trang.
- **M04 & M05 - Decks & Flashcards:** CRUD bộ thẻ và thẻ học cá nhân, phân loại bằng tags, kiểm soát xung đột phiên bản đa thiết bị bằng Optimistic Concurrency (`BaseVersion` / HTTP 409).
- **M06 - Spaced Repetition (SRS v1):** Lập lịch ôn tập tự động theo 4 mức đánh giá (`Again`, `Hard`, `Good`, `Easy`), truy vấn thẻ đến hạn và ghi nhận `ReviewEvent` bất biến (append-only) kèm khóa chống trùng `ClientEventId`.
- **M07 - Interactive Quiz:** Hỗ trợ 3 dạng câu hỏi (Multiple-Choice, Fill-in-the-blank, Matching), ẩn đáp án tuyệt đối trước khi nộp và chấm điểm idempotent phía Server.
- **M08 - Learning Progress:** Ghi danh khóa học tự động, cập nhật tiến độ bài học một chiều (`NotStarted -> InProgress -> Completed`) và tính năng "Tiếp tục học bài gần nhất".
- **M09 - CSV Import & Export:** Tải mẫu CSV chuẩn UTF-8 BOM, xem trước dữ liệu và báo lỗi theo dòng, nhập thẻ trong Transaction và xuất CSV chống Formula Injection.
- **M10 - Multimedia Support:** Tích hợp trình phát âm thanh HTML5 và hình ảnh minh họa trên bài học và từ vựng.
- **M11 & M12 - Statistics & Timezone Streak:** Tổng quan chỉ số học tập, biểu đồ Heatmap 365 ngày, tính chuỗi ngày học liên tục (Streak) và mục tiêu ngày (Daily Goal) chính xác theo múi giờ địa phương.
- **M13 - Deck Sharing & Moderation:** Thư viện bộ thẻ cộng đồng, xem trước 5 thẻ mẫu, sao chép (Clone) độc lập về tài khoản cá nhân và kiểm duyệt phía Admin có lưu `audit_logs`.
- **M14 & M15 - Offline PWA & Data Sync:** Tải trước bộ thẻ vào IndexedDB để ôn tập khi mất mạng; tự động đồng bộ qua `/api/v1/sync/push` (kèm sổ cái Idempotency `sync_operations` và SHA-256 payload hash) và `/api/v1/sync/pull` (phân trang cursor và tombstone xóa mềm) với cơ chế exponential backoff.
- **M16 - Admin Content & Operations:** Quản trị phân cấp `Language -> Course -> Unit -> Lesson -> Vocabulary`, kiểm soát trạng thái `Draft/Published`, endpoint `/health` và bộ công cụ sao lưu/phục hồi MySQL.