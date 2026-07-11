# FRS — Module 5: Quy Trình Thuê (Application)

> **Module:** M5  
> **Phiên bản:** 1.0  
> **Phụ thuộc:** M1, M2, M3  
> **Bảng DB:** `Applications`

---

## 1. Mô Tả Tổng Quan

Module quản lý toàn bộ quy trình từ lúc Gia sư apply vào Job của Học viên cho đến khi được chấp nhận và bắt đầu buổi học. Đây là module trung tâm kết nối M3 (Job) với M6 (Chat) và M7 (Booking).

---

## 2. Workflow Tổng Quan

```
[Tutor] Apply Job
       │
       ▼
Status: Pending ──▶ [Student] Xem Application
       │                    │
       │              ┌─────┴─────┐
       │              ▼           ▼
       │           Accept       Reject
       │              │
       │              ▼
       │        Status: Accepted
       │              │
       │        ┌─────┴──────────┐
       │        ▼                ▼
       │     [Chat bắt đầu]  [Book Lesson]
       │                         │
       │                         ▼
       │                   Status: Completed
       │
       └──▶ [Tutor] Cancel (khi còn Pending)
```

---

## 3. Actors & Quyền Hạn

| Actor | Quyền |
|---|---|
| **Tutor** | Apply job, Cancel application của mình |
| **Student** | Xem danh sách apply, Accept/Reject |
| **Admin** | Xem tất cả application, can thiệp khi có tranh chấp |

---

## 4. Danh Sách Chức Năng

### UC-M5-01: Tutor Apply Job

**Actor:** Tutor  
**Mô tả:** Gia sư nộp đơn apply vào job của học viên.

**Thông tin khi apply:**
- Thư giới thiệu (Cover Note) — tùy chọn, tối đa 500 ký tự
- Đề xuất học phí (ProposedRate) — tùy chọn

**Luồng chính:**
1. Tutor xem trang chi tiết Job
2. Nhấn nút "Apply Ngay"
3. Điền Cover Note và ProposedRate (nếu có)
4. Submit → Tạo Application với `Status = "Pending"`
5. Gửi thông báo cho Student (M11)
6. Redirect về trang chi tiết job, nút Apply đổi thành "Đã Apply"

**Business Rules:**
- Mỗi Tutor chỉ apply 1 lần cho 1 Job
- Tutor không apply được job của chính mình (không thể)
- Chỉ apply được job đang `Status = "Open"`
- Tutor phải có TutorProfile đã được duyệt

**Acceptance Criteria:**
- [ ] Tạo Application thành công trong DB
- [ ] Nút Apply đổi thành "Đã Apply" và disabled
- [ ] Student nhận notification
- [ ] Apply lần 2 vào cùng job → hiển thị lỗi

---

### UC-M5-02: Student Xem Danh Sách Apply

**Actor:** Student  
**Mô tả:** Học viên xem tất cả gia sư đã apply vào job của mình.

**Giao diện:**
- Hiển thị danh sách dạng card mỗi application
- Thông tin: Ảnh, Tên gia sư, Rating, Học phí đề xuất, Cover Note, Ngày apply
- Badge trạng thái: Pending / Accepted / Rejected

**Acceptance Criteria:**
- [ ] Hiển thị đúng danh sách apply cho job của Student
- [ ] Sắp xếp theo ngày apply mới nhất

---

### UC-M5-03: Student Chấp Nhận (Accept) Application

**Actor:** Student  
**Mô tả:** Học viên chấp nhận một gia sư apply vào job.

**Luồng chính:**
1. Student nhấn "Chấp nhận" trên Application của Tutor
2. Application `Status → "Accepted"`
3. Job `Status → "Closed"` (không nhận apply thêm)
4. Các Application còn `Pending` của job đó → tự động `Rejected`
5. Gửi thông báo cho Tutor được chọn
6. Gửi thông báo từ chối cho các Tutor còn lại
7. Hệ thống tự động tạo Conversation (M6) giữa Student và Tutor

**Business Rules:**
- Chỉ accept 1 application cho mỗi job
- Sau khi accept → job đóng lại tự động

**Acceptance Criteria:**
- [ ] Application được accept → status cập nhật
- [ ] Job tự động Closed
- [ ] Các application còn lại tự động Rejected
- [ ] Conversation được tạo giữa 2 người

---

### UC-M5-04: Student Từ Chối (Reject) Application

**Actor:** Student  
**Mô tả:** Học viên từ chối đơn apply của một gia sư.

**Business Rules:**
- Có thể reject trước khi accept ai đó
- Sau khi reject, Tutor không thể apply lại job đó

**Acceptance Criteria:**
- [ ] Application `Status → "Rejected"`
- [ ] Tutor nhận notification bị từ chối
- [ ] Tutor không thể apply lại job đó

---

### UC-M5-05: Tutor Hủy Application (Cancel)

**Actor:** Tutor  
**Mô tả:** Gia sư rút đơn apply trước khi được xem xét.

**Business Rules:**
- Chỉ cancel được khi `Status = "Pending"`
- Không cancel được khi đã `Accepted`

**Acceptance Criteria:**
- [ ] Application `Status → "Cancelled"`
- [ ] Nút Apply xuất hiện lại trên trang job (có thể apply lại)

---

### UC-M5-06: Theo Dõi Trạng Thái Application

**Actor:** Tutor  
**Mô tả:** Gia sư xem danh sách tất cả application mình đã nộp và trạng thái.

**Trang `/Tutor/MyApplications`:**
- Danh sách job đã apply
- Trạng thái từng application
- Nút Cancel (nếu Pending)
- Nút Chat (nếu Accepted)

**Tab lọc:** Tất cả / Đang chờ / Được chấp nhận / Bị từ chối / Đã hủy

**Acceptance Criteria:**
- [ ] Hiển thị đúng danh sách theo Tutor đang đăng nhập
- [ ] Filter theo tab hoạt động

---

## 5. State Machine — Application Status

```
                    ┌──────────┐
                    │ Pending  │
                    └────┬─────┘
          ┌──────────────┼───────────────┐
          ▼              ▼               ▼
     Accepted         Rejected        Cancelled
          │
          ▼
      Completed (sau khi booking hoàn thành)
```

---

## 6. Out of Scope

- Đàm phán học phí qua nhiều vòng
- Counter-offer từ Tutor
- Apply với portfolio đính kèm

---

## 7. Task Breakdown

| Task ID | Tên Task | Ước tính |
|---|---|---|
| M5-T1 | Application Model + Migration | 30 phút |
| M5-T2 | Tutor Apply Job (form + logic) | 60 phút |
| M5-T3 | Student xem danh sách Application | 45 phút |
| M5-T4 | Accept Application + đóng Job + reject others | 60 phút |
| M5-T5 | Reject & Cancel Application | 30 phút |
| M5-T6 | Trang quản lý Application của Tutor | 45 phút |
| M5-T7 | Tự động tạo Conversation sau Accept | 30 phút |
