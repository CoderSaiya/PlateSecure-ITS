# API Quản lý Bãi Đỗ Xe

Repository này chứa một API .NET để quản lý các sự kiện phát hiện phương tiện trong bãi đỗ xe, bao gồm theo dõi ra/vào, xử lý thanh toán và ghi nhật ký.

## Các Endpoint API

API cung cấp các endpoint sau để quản lý các sự kiện phát hiện trong bãi đỗ xe:

### 1. Xử lý Sự kiện Vào Bãi

Xử lý sự kiện phát hiện phương tiện vào hoặc ra tại cổng.

```
POST /api/detection/entry
```

**Kiểu Nội dung:** `multipart/form-data`

**Tham số Yêu cầu:**
- `Image` (file, bắt buộc): Hình ảnh phương tiện được chụp tại cổng
- `ConfidenceScore` (số, bắt buộc): Điểm tin cậy của việc phát hiện
- `LicensePlate` (chuỗi, tùy chọn): Biển số xe được phát hiện
- `GateIn` (chuỗi, điều kiện): Định danh cổng cho sự kiện vào (bắt buộc nếu GateOut là null)
- `GateOut` (chuỗi, điều kiện): Định danh cổng cho sự kiện ra (bắt buộc nếu GateIn là null)
- `MetadataJson` (chuỗi, tùy chọn): Metadata bổ sung dưới dạng JSON

**Quy tắc Xác thực:**
- Chỉ được cung cấp một trong hai tham số `GateIn` hoặc `GateOut`

**Phản hồi:**
- `200 OK`: Trả về kết quả xử lý phát hiện
- `400 Bad Request`: Nếu cả hai hoặc không có tham số `GateIn` và `GateOut` được cung cấp

### 2. Xử lý Sự kiện Ra Khỏi Bãi

Xử lý sự kiện phương tiện ra khỏi bãi.

```
POST /api/detection/exit
```

**Kiểu Nội dung:** `multipart/form-data`

**Tham số Yêu cầu:**
Các tham số tương tự với xử lý vào bãi, và có thêm một tham mới
- `Fee` (double, bắt buộc): Mức phí gửi xe tại bãi (tùy loại phương tiện)

**Phản hồi:**
- `200 OK`: Trả về thông tin thanh toán thành công
- `400 Bad Request`: Nếu thao tác không hợp lệ, kèm theo thông báo lỗi

### 3. Cập nhật Thanh toán

Cập nhật thông tin thanh toán cho một sự kiện đỗ xe cụ thể.

```
PUT /api/detection/{id}/payment
```

**Tham số Đường dẫn:**
- `id` (chuỗi, bắt buộc): Định danh của sự kiện đỗ xe

**Nội dung Yêu cầu:** Đối tượng JSON tuân thủ `PaymentUpdateDto`

**Phản hồi:**
- `200 OK`: Trả về thông tin sự kiện đỗ xe đã được cập nhật
- `400 Bad Request`: Nếu các tham số không hợp lệ, kèm theo thông báo lỗi
- `404 Not Found`: Nếu không tìm thấy sự kiện đỗ xe với ID đã chỉ định

### 4. Lấy Nhật ký

Truy xuất tất cả các bản ghi nhật ký từ hệ thống.

```
GET /api/detection/logs
```

**Phản hồi:**
- `200 OK`: Trả về danh sách các bản ghi nhật ký

**Ví dụ Yêu cầu:**
```bash
curl -X GET https://your-api.com/api/detection/logs
```

### 5. Lấy Sự kiện Đỗ xe

Truy xuất tất cả các sự kiện đỗ xe từ hệ thống.

```
GET /api/detection/event
```

**Phản hồi:**
- `200 OK`: Trả về danh sách các sự kiện đỗ xe
