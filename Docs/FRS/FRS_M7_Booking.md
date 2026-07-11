# FRS — Module 7: Lịch Học (Booking / Calendar)

> **Module:** M7  
> **Phiên bản:** 1.0  
> **Phụ thuộc:** M1, M5 (Application accepted)  
> **Bảng DB:** `Bookings`

---

## 1. Mô Tả Tổng Quan

Module quản lý lịch học giữa Student và Tutor sau khi Application được chấp nhận. Giao diện dạng Google Calendar. Hỗ trợ đặt lịch, hủy, đổi lịch, xác nhận buổi học, check-in/check-out và reminder.

---

## 2. Actors & Quyền Hạn

| Actor | Quyền |
|---|---|
| **Student** | Đặt lịch, hủy, đổi lịch, check-in |
| **Tutor** | Xác nhận/từ chối lịch, check-in/check-out |
| **System** | Gửi reminder, tự động đổi trạng thái |
| **Admin** | Xem tất cả booking, xử lý tranh chấp |

---

## 3. Danh Sách Chức Năng

### UC-M7-01: Đặt Lịch Học

**Actor:** Student  
**Mô tả:** Student đặt lịch buổi học với Tutor đã được accept.

**Thông tin booking:**
- Ngày và giờ bắt đầu
- Thời lượng (phút)
- Meeting URL (Google Meet/Zoom — tùy chọn)
- Ghi chú

**Luồng chính:**
1. Student vào `/Booking/Create?applicationId=X`
2. Chọn ngày, giờ (dựa trên lịch rảnh của Tutor từ M2)
3. Submit → tạo Booking `Status = "Pending"`
4. Gửi notification cho Tutor
5. Tutor xác nhận → `Status = "Confirmed"`

**Business Rules:**
- Chỉ đặt lịch với Application `Status = "Accepted"`
- Không đặt lịch trùng với booking đã Confirmed
- Thời gian đặt phải trong lịch rảnh của Tutor
- Đặt trước tối thiểu 2 giờ

**Acceptance Criteria:**
- [ ] Booking được tạo và lưu DB
- [ ] Tutor nhận notification
- [ ] Lịch rảnh được highlight trên calendar

---

### UC-M7-02: Tutor Xác Nhận Lịch

**Actor:** Tutor  
**Mô tả:** Tutor xác nhận hoặc từ chối buổi học được đề xuất.

**Acceptance Criteria:**
- [ ] Xác nhận → `Status = "Confirmed"`, Student nhận notification
- [ ] Từ chối → `Status = "Cancelled"`, Student nhận notification kèm lý do

---

### UC-M7-03: Hủy Lịch

**Actor:** Student, Tutor  
**Mô tả:** Một trong hai bên hủy buổi học.

**Business Rules:**
- Hủy trước 2 giờ → không bị phạt
- Hủy trong vòng 2 giờ trước giờ học → ghi chú vi phạm (ảnh hưởng rating)
- Sau khi hủy → Booking `Status = "Cancelled"`

**Acceptance Criteria:**
- [ ] Hủy thành công → status cập nhật, bên còn lại nhận notification
- [ ] Ghi chú vi phạm nếu hủy muộn

---

### UC-M7-04: Đổi Lịch

**Actor:** Student, Tutor  
**Mô tả:** Đề xuất đổi sang thời gian khác.

**Luồng:**
1. Một bên đề xuất thời gian mới
2. Bên kia nhận notification, confirm hoặc từ chối
3. Confirm → booking cũ hủy, booking mới Confirmed

**Acceptance Criteria:**
- [ ] Flow đổi lịch hoàn chỉnh, cả 2 bên đồng ý mới đổi

---

### UC-M7-05: Calendar View

**Mô tả:** Hiển thị lịch học dạng tháng/tuần/ngày.

**Giao diện:**
- Dạng calendar tương tự Google Calendar
- Màu sắc theo trạng thái: Pending (vàng), Confirmed (xanh), Cancelled (đỏ), Completed (xám)
- Click vào event → xem chi tiết booking

**Acceptance Criteria:**
- [ ] Calendar hiển thị đúng booking theo tháng/tuần/ngày
- [ ] Màu sắc đúng theo trạng thái
- [ ] Responsive trên mobile

---

### UC-M7-06: Check-in

**Actor:** Student, Tutor  
**Mô tả:** Xác nhận buổi học đã bắt đầu.

**Business Rules:**
- Check-in được trong khoảng ±15 phút so với giờ bắt đầu
- Cả 2 phải check-in để buổi học tính là đã diễn ra

**Acceptance Criteria:**
- [ ] Check-in thành công → `CheckInAt` ghi nhận
- [ ] Nếu chỉ 1 bên check-in sau 30 phút → tự động xử lý

---

### UC-M7-07: Check-out & Hoàn Thành

**Actor:** Student, Tutor  
**Mô tả:** Xác nhận buổi học kết thúc.

**Business Rules:**
- Check-out → `CheckOutAt` ghi nhận
- Cả 2 check-out hoặc qua giờ kết thúc → `Status = "Completed"`
- Sau Completed → trigger review (M10) và thanh toán (M9)

**Acceptance Criteria:**
- [ ] `Status = "Completed"` sau khi hoàn thành
- [ ] Trigger notification nhắc đánh giá

---

### UC-M7-08: Reminder

**Mô tả:** Hệ thống gửi nhắc nhở trước buổi học.

**Business Rules:**
- Reminder 24 giờ trước
- Reminder 1 giờ trước
- Qua email + notification trong app (M11)

**Acceptance Criteria:**
- [ ] Email reminder gửi đúng thời điểm
- [ ] Notification in-app hiển thị

---

## 4. Out of Scope

- Tích hợp Google Calendar API
- Tích hợp Zoom/Meet API
- Học nhóm (nhiều student)

---

## 5. Task Breakdown

| Task ID | Tên Task | Ước tính |
|---|---|---|
| M7-T1 | Booking Model + Migration | 30 phút |
| M7-T2 | Form đặt lịch + chọn thời gian | 60 phút |
| M7-T3 | Tutor xác nhận/từ chối lịch | 45 phút |
| M7-T4 | Hủy lịch + Đổi lịch | 60 phút |
| M7-T5 | Calendar view (tháng/tuần/ngày) | 90 phút |
| M7-T6 | Check-in / Check-out | 45 phút |
| M7-T7 | Reminder (background job) | 45 phút |
| M7-T8 | Tự động Completed sau check-out | 30 phút |
