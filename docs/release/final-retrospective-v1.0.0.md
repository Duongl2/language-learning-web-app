# Báo cáo Tổng kết Dự án & Phát hành v1.0.0 (Final Retrospective)

**Dự án:** Language Learning Web Application  
**Môn học:** Phát triển Phần mềm Mã nguồn mở (PTPMMNM)  
**Phiên bản phát hành:** `v1.0.0`  
**Repository:** `https://github.com/NguyenLuuKienn/language-learning-web-app`

---

## 1. Tổng quan Kết quả Đạt được
- Hoàn thành **16/16 Modules MVP** theo đúng kiến trúc Clean Architecture (Backend ASP.NET Core 8 + EF Core + MySQL) và Feature-First PWA (Frontend React + TypeScript + Tailwind CSS + Dexie IndexedDB).
- Tuân thủ nghiêm ngặt quy trình cộng tác mã nguồn mở trên GitHub: **62 Issues** được phân bổ rõ ràng qua **4 Milestones** (`v0.1.0 Foundation`, `v0.2.0 Learning Engine`, `v0.3.0 Extended Learning`, `v1.0.0 MVP Release`).
- Toàn bộ thay đổi đều đi qua nhánh tính năng (`feature/<issue-number>-<short-description>`), tạo Pull Request về `develop`, vượt qua các bài kiểm tra tự động của GitHub Actions CI (Build 0 Warning, Unit/Integration Tests, Lint, GitGuardian Security Check) và thực hiện Code Review chéo trước khi merge.

---

## 2. Báo cáo Tổng kết Đóng góp của 4 Thành viên

| Thành viên (GitHub) | Vai trò | Phân hệ Phụ trách Chính | Đóng góp Tiêu biểu (Issues & PRs) |
|---|---|---|---|
| **NguyenLuuKienn** | **Leader** | Foundation, Auth & Profile (M01), Security, CI/CD, Deployment & Release | Khởi tạo kiến trúc Monorepo, GitHub Projects, 62 Issues, шаблоны PR/Issue, Docker Compose, OpenAPI conventions (#1–#9); triển khai hệ thống Auth JWT/Refresh Token, User Profile & Timezone (#10–#13); kiểm thử phân quyền bảo mật (#55); Health Checks, Staging/Production Backup & Restore Drill (#58, #59); điều phối Code Review toàn dự án và hoàn thiện tài liệu phát hành v1.0.0 (#60–#62). |
| **MaitPhap** | **Maintainer** | Courses (M02), Vocabulary (M03), Admin Content Management (M16) | Xây dựng hệ thống CRUD quản trị nội dung phân cấp `Language -> Course -> Unit -> Lesson` (#14); kiểm soát xuất bản `Draft/Published` và thứ tự bài học (#15); giao diện danh mục khóa học và lộ trình học (#16, PR #91); màn hình học chi tiết `Lesson Viewer` tích hợp từ vựng, âm thanh và điều hướng xuyên chương (#17, PR #96); API quản lý và tìm kiếm từ vựng phân trang (#18, PR #117). |
| **nguyenhqbinh** | **Core Developer** | Decks (M04), Flashcards (M05), SRS Engine (M06), Sync Server & Conflict Resolution (M15) | Thiết kế MySQL Schema & EF Core Migrations (#5, #8); triển khai CRUD Decks, Cards, Flashcard Editor và thuật toán lặp lại ngắt quãng `srs-v1` (#20–#27); xây dựng hệ thống đồng bộ `/api/v1/sync/push` với sổ cái Idempotency `sync_operations` (#47, PR #92), `/api/v1/sync/pull` phân trang cursor và xử lý xung đột `BaseVersion` cho Deck/Card (#48, #49, #50, PR #94, #95, #97); thực hiện báo cáo kiểm thử nghiệm thu TC-01..TC-20 (#56, PR #98). |
| **Duongl2** | **Contributor** | Quiz (M07), Progress (M08), CSV (M09), Statistics (M11), Streak (M12), Sharing (M13), Offline PWA (M14) & Test Suites | Xây dựng PWA Offline App Shell, IndexedDB predownload và hàng đợi `syncQueue` (#44, #45, #46); triển khai API & UI cho Quiz trắc nghiệm/điền từ/nối cặp (#28, #29, #30, PR #100–#102), Tiến độ học tập & Resume (#31, #32, PR #103–#104), CSV Preview/Import/Export chống Formula Injection (#33, #34, PR #105–#106), Thống kê & Heatmap (#37, #38, PR #107–#108), Timezone Streak (#39, #40, PR #109–#110), Thư viện cộng đồng & Clone Deck (#41, #42, #43, PR #111–#113) cùng bộ test tích hợp Backend/Frontend E2E (#53, #54, PR #114–#115). |

---

## 3. Bài học Kinh nghiệm (Lessons Learned)
1. **Kiến trúc & Định danh GUID (ADR-001):** Việc thống nhất khóa chính `Guid` (`CHAR(36)`) ngay từ đầu giúp giải quyết triệt để bài toán sinh sự kiện ôn tập ngoại tuyến trên IndexedDB mà không xảy ra xung đột khóa chính khi đồng bộ lên MySQL.
2. **Idempotency & Optimistic Concurrency:** Sự kết hợp giữa sổ cái `sync_operations` (băm SHA-256 payload) cho các sự kiện bất biến và `BaseVersion` (Concurrency Token) cho các thực thể có trạng thái (Deck, Card, LessonProgress) giúp hệ thống chịu lỗi tốt trước điều kiện mạng chập chờn hoặc đa thiết bị.
3. **Kỷ luật Code Review & Chia nhỏ PR:** Việc chia nhỏ từng Issue thành các PR độc lập, tách biệt rõ tầng Backend API và Frontend UI, kết hợp quy tắc kiểm tra xung đột trước khi merge giúp duy trì nhánh `develop` luôn xanh trên GitHub Actions CI.

---

## 4. Quy trình Đóng gói & Gắn nhãn Phát hành `v1.0.0`
Sử dụng script `scripts/release-v1.0.0.ps1` sau khi hợp nhất nhánh `develop` vào `main`:
```powershell
pwsh ./scripts/release-v1.0.0.ps1
```