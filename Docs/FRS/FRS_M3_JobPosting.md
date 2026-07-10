# FRS — Module 3: Quản Lý Yêu Cầu Thuê Gia Sư

> **Module:** M3  
> **Phiên bản:** 1.0  
> **Phụ thuộc:** M1 (Auth), M2 (Subjects)  
> **Bảng DB:** `JobPostings`, `Subjects`

---

## 1. Mô Tả Tổng Quan

Học viên đăng yêu cầu tìm gia sư (Job Posting) với đầy đủ thông tin về môn học, trình độ mong muốn, ngân sách và hình thức học. Gia sư và AI có thể tìm kiếm, lọc và apply vào các job phù hợp.

---

## 2. Actors & Quyền Hạn

| Actor | Quyền |
|---|---|
| **Guest** | Xem danh sách job công khai |
| **Student** | CRUD job của mình |
| **Tutor** | Xem danh sách job, apply (module M5) |
| **Admin** | Xem/ẩn/xóa job vi phạm |

---

## 3. Danh Sách Chức Năng

### UC-M3-01: Đăng Yêu Cầu Thuê Gia Sư

**Actor:** Student  
**Mô tả:** Học viên tạo một Job Posting tìm gia sư.

**Thông tin Job:**

| Field | Bắt buộc | Mô tả |
|---|---|---|
| Tiêu đề | ✅ | VD: "Cần gia sư IELTS 7.5+" |
| Mô tả chi tiết | ✅ | Tối thiểu 50 ký tự |
| Môn học | ✅ | Chọn từ danh sách |
| Trình độ mong muốn | ❌ | VD: IELTS 7.5+, THPT lớp 12... |
| Hình thức học | ✅ | Online / Offline / Cả hai |
| Địa điểm | ❌ | Bắt buộc nếu Offline |
| Ngân sách tối thiểu | ❌ | VNĐ/giờ |
| Ngân sách tối đa | ❌ | VNĐ/giờ |
| Số buổi/tuần | ❌ | Số nguyên ≥ 1 |
| Thời lượng mỗi buổi | ❌ | Phút |
| Deadline | ❌ | Ngày hết hạn nhận apply |

**Luồng chính:**
1. Student vào `/Jobs/Create`
2. Điền thông tin
3. Submit → hệ thống validate
4. Lưu với `Status = "Open"`
5. Hiển thị thành công và redirect về trang chi tiết job

**Business Rules:**
- Mỗi Student có thể có tối đa 5 job đang `Open` cùng lúc
- Deadline nếu có phải là ngày trong tương lai
- Job tự động chuyển sang `Expired` khi qua deadline

**Acceptance Criteria:**
- [ ] Form đầy đủ, validation hoạt động
- [ ] Job được tạo với `Status = "Open"`
- [ ] Job hiển thị trong danh sách công khai

---

### UC-M3-02: Xem Danh Sách Job

**Actor:** Guest, Student, Tutor  
**Mô tả:** Xem danh sách tất cả job đang `Open`.

**Giao diện:**
- Hiển thị dạng card: Tiêu đề, Môn học, Ngân sách, Hình thức, Deadline
- Phân trang: 10 job/trang
- Bộ lọc và tìm kiếm (xem UC-M3-05)

**Acceptance Criteria:**
- [ ] Chỉ hiển thị job `Status = "Open"`
- [ ] Hiển thị đúng thông tin trên card
- [ ] Phân trang hoạt động

---

### UC-M3-03: Xem Chi Tiết Job

**Actor:** Guest, Student, Tutor  
**Mô tả:** Xem toàn bộ thông tin một Job Posting.

**Trang chi tiết hiển thị:**
- Toàn bộ thông tin job
- Thông tin Student đăng job (tên, ảnh, rating trung bình)
- Số lượng gia sư đã apply
- Nút "Apply ngay" (chỉ hiện với Tutor, khi job còn Open)

**Acceptance Criteria:**
- [ ] Hiển thị đầy đủ thông tin
- [ ] Tutor thấy nút Apply, Student không thấy nút Apply job của người khác

---

### UC-M3-04: Chỉnh Sửa Job

**Actor:** Student (chủ job)  
**Mô tả:** Chỉnh sửa thông tin job đã đăng.

**Business Rules:**
- Chỉ sửa được khi job đang `Open`
- Không sửa được job đã `Closed` hoặc `Expired`
- Sau khi có Application, chỉ sửa được một số field không ảnh hưởng đến thỏa thuận

**Acceptance Criteria:**
- [ ] Sửa thành công → dữ liệu cập nhật
- [ ] Job đã `Closed` → nút sửa bị ẩn hoặc disabled

---

### UC-M3-05: Xóa / Đóng Job

**Actor:** Student (chủ job)  
**Mô tả:** Đóng job khi không cần tuyển thêm hoặc xóa hẳn.

**Business Rules:**
- Đóng job (`Status = "Closed"`): job không hiển thị công khai, gia sư không apply được nữa
- Xóa job: chỉ xóa được khi chưa có Application nào
- Nếu đã có Application → chỉ được Đóng, không được Xóa

**Acceptance Criteria:**
- [ ] Đóng job → biến mất khỏi danh sách công khai
- [ ] Xóa job có Application → hiển thị thông báo không thể xóa

---

### UC-M3-06: Tìm Kiếm & Lọc Job

**Actor:** Tutor, Guest, Student  
**Mô tả:** Tìm kiếm job theo tiêu chí.

**Bộ lọc:**
- Từ khóa (tìm trong tiêu đề, mô tả)
- Môn học
- Hình thức: Online / Offline / Cả hai
- Ngân sách (khoảng min-max)
- Địa điểm
- Còn hạn (deadline chưa qua)

**Sắp xếp:** Mới nhất / Ngân sách cao nhất / Sắp hết hạn

**Acceptance Criteria:**
- [ ] Kết quả trả về đúng với bộ lọc
- [ ] URL có query string, có thể share
- [ ] Không trả về job `Expired` hoặc `Closed`

---

### UC-M3-07: Xem Job Của Mình

**Actor:** Student  
**Mô tả:** Student xem danh sách tất cả job mình đã đăng.

**Tab lọc:**
- Tất cả
- Đang mở (Open)
- Đã đóng (Closed)
- Hết hạn (Expired)

**Acceptance Criteria:**
- [ ] Hiển thị đúng job của Student đang đăng nhập
- [ ] Filter theo tab hoạt động
- [ ] Mỗi job có nút Sửa/Đóng/Xóa tùy trạng thái

---

## 4. Business Rules Tổng Hợp

| Rule | Mô tả |
|---|---|
| Giới hạn job | Tối đa 5 job Open cùng lúc |
| Auto expire | Job tự chuyển Expired khi qua deadline |
| Không xóa có Apply | Job đã có Application không được xóa |
| Hiển thị | Chỉ show job Open trên trang công khai |

---

## 5. Out of Scope

- Job dạng đấu giá (bidding)
- Job nổi bật (featured/boost)
- Thanh toán phí đăng job

---

## 6. Task Breakdown

| Task ID | Tên Task | Ước tính |
|---|---|---|
| M3-T1 | JobPosting Model + Migration | 30 phút |
| M3-T2 | Form tạo Job + Validation | 60 phút |
| M3-T3 | Danh sách Job công khai + Phân trang | 60 phút |
| M3-T4 | Trang chi tiết Job | 45 phút |
| M3-T5 | Sửa/Đóng/Xóa Job | 45 phút |
| M3-T6 | Tìm kiếm & Lọc Job | 60 phút |
| M3-T7 | Trang quản lý Job của Student | 45 phút |
| M3-T8 | Background job tự động expire | 30 phút |
