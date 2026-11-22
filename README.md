# 🌐 Language Learning Web Application

[![Build & Test CI](https://github.com/NguyenLuuKienn/language-learning-web-app/actions/workflows/backend-ci.yml/badge.svg)](https://github.com/NguyenLuuKienn/language-learning-web-app/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![.NET Version](https://img.shields.io/badge/.NET-8.0%2F9.0-purple.svg)](https://dotnet.microsoft.com/)
[![React Version](https://img.shields.io/badge/React-18%2B-blue.svg)](https://react.dev/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED.svg)](https://www.docker.com/)

Ứng dụng web học ngoại ngữ hiện đại tích hợp **Flashcards** và thuật toán lặp lại ngắt quãng **Spaced Repetition (SRS)**, hỗ trợ học theo khóa học có sẵn, làm Quiz đánh giá năng lực, PWA học ngoại tuyến (Offline) và tự động đồng bộ dữ liệu hai chiều khi có kết nối mạng.

> 🎓 **Đề tài:** Phát triển Phần mềm Mã nguồn mở (PTPMMNM)  
> 👥 **Nhóm thực hiện:** 4 thành viên (Leader, Maintainer, Core Developer, Contributor)  
> 📦 **Phiên bản hiện tại:** `v0.1.0-alpha` (Đang phát triển MVP v1.0.0)

---

## 📌 Tính năng cốt lõi (16 Modules MVP)

- **M01 - Authentication & Profile:** Xác thực JWT, Refresh token an toàn, phân quyền Learner/Admin, thiết lập múi giờ cá nhân.
- **M02 - Course Management:** Cấu trúc phân cấp Course → Unit → Lesson; duyệt khóa học trực quan.
- **M03 - Vocabulary:** Quản lý ngân hàng từ vựng theo bài học, phiên âm IPA, giải nghĩa, câu ví dụ, hỗ trợ âm thanh và hình ảnh.
- **M04 - Deck Management:** Tự do tạo bộ thẻ cá nhân, gắn thẻ phân loại (tags), chế độ công khai/riêng tư.
- **M05 - Flashcards:** Lật thẻ hai mặt mượt mà, hỗ trợ phím tắt bàn phím (Space, 1-4).
- **M06 - Spaced Repetition (SRS):** Lên lịch ôn tập dựa trên phản hồi (Again / Hard / Good / Easy), lọc thẻ đến hạn (Due cards).
- **M07 - Quiz & Practice:** Đa dạng dạng bài: Trắc nghiệm (MCQ), điền từ vào chỗ trống, ghép cặp thẻ. Chấm điểm server.
- **M08 - Learning Progress:** Theo dõi tiến độ học tập chi tiết, ghi nhận kết quả quiz, tính năng "Tiếp tục học bài gần nhất".
- **M09 - CSV Import/Export:** Nhập/xuất bộ từ vựng nhanh chóng qua CSV, xem trước dữ liệu, chống lỗi UTF-8 và formula injection.
- **M10 - Multimedia:** Đính kèm phát âm thanh và hình ảnh minh họa cho từ vựng và thẻ học.
- **M11 - Advanced Statistics:** Biểu đồ học tập trực quan, tỷ lệ ghi nhớ, Heatmap hoạt động theo phong cách GitHub.
- **M12 - Learning Streak:** Chuỗi ngày học liên tục (Streak), mục tiêu học hằng ngày (Daily Goal) tính theo múi giờ địa phương.
- **M13 - Deck Sharing & Cloning:** Khám phá thư viện thẻ cộng đồng, xem trước và sao chép bộ thẻ về tài khoản riêng.
- **M14 - Offline Learning (PWA):** Ứng dụng PWA hỗ trợ cài đặt trên Desktop/Mobile, lưu trữ thẻ và học bài hoàn toàn offline bằng IndexedDB.
- **M15 - Data Synchronization:** Cơ chế đẩy hàng đợi offline tự động khi online, xử lý idempotency chống trùng lặp dữ liệu và giải quyết xung đột (conflict resolution).
- **M16 - Admin Management:** Cổng quản trị viên quản lý tài khoản người dùng, phê duyệt/kiểm duyệt nội dung khóa học và bộ thẻ công khai.

---

## 🏗️ Kiến trúc & Công nghệ

### Frontend (PWA)
- **Framework:** React 18+ với TypeScript
- **Bundler:** Vite
- **Styling:** Tailwind CSS (Dark/Light Mode)
- **Data Fetching:** TanStack Query (React Query)
- **Local Storage / Offline:** Dexie.js (IndexedDB wrapper) & Service Worker Cache

### Backend
- **Nền tảng:** ASP.NET Core Web API (Clean Architecture: Domain, Application, Infrastructure, Api)
- **ORM:** Entity Framework Core với Pomelo MySQL Provider
- **Khóa chính:** Consistent UUID / GUID (`CHAR(36)`)
- **Tài liệu API:** OpenAPI / Swagger UI

### Database & Hạ tầng
- **Database:** MySQL 8.0
- **Containerization:** Docker & Docker Compose
- **CI/CD:** GitHub Actions

---

## 🚀 Hướng dẫn Chạy Nhanh (Quick Start)

### 1. Yêu cầu môi trường
- [Git](https://git-scm.com/)
- [Node.js](https://nodejs.org/) v18+ và npm/yarn
- [.NET SDK 8.0+](https://dotnet.microsoft.com/)
- [Docker Desktop](https://www.docker.com/) (Khuyến nghị) hoặc MySQL Server 8.0

### 2. Chạy toàn bộ hệ thống bằng Docker Compose (Khuyên dùng)
```bash
# Clone repository
git clone https://github.com/NguyenLuuKienn/language-learning-web-app.git
cd language-learning-web-app

# Thiết lập file biến môi trường
cp .env.example .env

# Khởi động toàn bộ MySQL, API và Web
docker compose up --build -d
```
Sau khi khởi động thành công:
- **Frontend App:** http://localhost:3000
- **Backend API Swagger:** http://localhost:5000/swagger
- **MySQL Database:** localhost:3306

---

## 📁 Cấu trúc Thư mục

```text
├── .github/              # GitHub Actions Workflows, Issue & PR Templates
├── backend/              # Solution ASP.NET Core Clean Architecture
│   ├── src/              # Api, Application, Domain, Infrastructure
│   └── tests/            # Unit & Integration Tests
├── frontend/             # Ứng dụng React PWA Vite
│   ├── public/           # Manifest, Assets tĩnh
│   └── src/              # Features, Components, Offline Dexie Store
├── docs/                 # Tài liệu kiến trúc, database ERD, API specs, ADRs
├── scripts/              # Scripts tự động hoá CI, nạp dữ liệu mẫu, tạo issue
├── docker-compose.yml    # Khởi chạy toàn bộ hệ thống
└── README.md
```

---

## 👥 Vai trò & Phân chia Công việc

| Thành viên | Vai trò GitHub | Module Phụ trách Chính |
|---|---|---|
| **Leader** | Quản trị dự án & Roadmap | M01 (Auth), M11 (Stats), M12 (Streak), Seed Data |
| **Maintainer** | Quản trị Code, CI/CD, Release | M02 (Courses), M03 (Vocabulary), M16 (Admin) |
| **Core Developer** | Kiến trúc & Backend cốt lõi | M04 (Decks), M05 (Cards), M06 (SRS Engine), M10 (Media) |
| **Contributor** | Trải nghiệm & Tương tác | M07 (Quiz), M08 (Progress), M09 (CSV), M13 (Share), M14-15 (Offline/Sync) |

---

## 🤝 Đóng góp mã nguồn

Dự án áp dụng quy trình phát triển GitHub tiêu chuẩn:
1. Đọc kỹ hướng dẫn tại [CONTRIBUTING.md](./CONTRIBUTING.md).
2. Quy tắc ứng xử cộng đồng tại [CODE_OF_CONDUCT.md](./CODE_OF_CONDUCT.md).
3. Tuân thủ định dạng commit: [Conventional Commits](https://www.conventionalcommits.org/).

## 📄 Bản quyền (License)

Dự án phát hành dưới giấy phép mã nguồn mở [MIT License](./LICENSE).
