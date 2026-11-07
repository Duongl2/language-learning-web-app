# Quy ước Thiết kế REST API (API Conventions)

## 1. Tiền tố & Phiên bản (Prefix & Versioning)
Mọi API đều có tiền tố: `/api/v1`

## 2. Phương thức HTTP (HTTP Methods)
- `GET`: Lấy thông tin tài nguyên (Idempotent, Safe).
- `POST`: Tạo mới tài nguyên hoặc kích hoạt hành động nghiệp vụ.
- `PATCH`: Cập nhật một phần tài nguyên.
- `DELETE`: Xóa tài nguyên (ưu tiên Soft Delete hoặc Archive nếu có liên kết).

## 3. Xác thực & Phân quyền (Authentication & Authorization)
- Yêu cầu Header: `Authorization: Bearer <jwt_token>`
- Token chứa thông tin Claims: `sub` (User ID dạng GUID), `email`, `role` (`Learner` hoặc `Admin`).

## 4. Định dạng Phản hồi Lỗi (RFC 7807 Problem Details)
Khi có lỗi, API trả về mã lỗi HTTP kèm JSON theo chuẩn `application/problem+json`:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "Email đã được đăng ký trên hệ thống.",
  "errors": {
    "Email": ["Email đã tồn tại."]
  }
}
```

## 5. Quy chuẩn Phân trang (Pagination)
Các endpoint danh sách sử dụng Query params:
- `page`: Số trang (bắt đầu từ 1, mặc định 1).
- `pageSize`: Số mục mỗi trang (mặc định 10, tối đa 50).
```json
{
  "items": [],
  "page": 1,
  "pageSize": 10,
  "totalItems": 45,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

## 6. Cập nhật Flashcard và kiểm soát phiên bản

### Endpoint

`PATCH /api/v1/decks/{deckId}/cards/{cardId}`

Yêu cầu đăng nhập và quyền sở hữu Deck.

### Request Body

```json
{
  "baseVersion": 1,
  "front": "Hello",
  "back": "Xin chào"
}
```

- `baseVersion`: Bắt buộc, là số nguyên từ 1 trở lên.
- `front`: Nội dung mặt trước, tùy chọn.
- `back`: Nội dung mặt sau, tùy chọn.

Client phải lấy `version` hiện tại từ thông tin Flashcard và gửi lại dưới tên `baseVersion` khi cập nhật.

### HTTP Response

| Status | Ý nghĩa |
|---|---|
| 200 OK | Cập nhật thành công, trả về Flashcard với version mới |
| 400 Bad Request | Thiếu hoặc không hợp lệ `baseVersion` |
| 404 Not Found | Flashcard hoặc Deck không tồn tại hoặc không có quyền truy cập |
| 409 Conflict | `baseVersion` không khớp với phiên bản hiện tại trên Server |

### Xử lý xung đột

Khi nhận HTTP 409, Client không được tự động ghi đè dữ liệu mới hơn.

Frontend Flashcard Editor (Issue #23) cần tải lại Flashcard mới nhất, thông báo xung đột và cho người dùng quyết định cách chỉnh sửa tiếp.
