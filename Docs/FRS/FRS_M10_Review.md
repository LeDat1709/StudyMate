# FRS — Module 10: Đánh Giá (Review & Rating)

> **Module:** M10  
> **Phiên bản:** 1.0  
> **Phụ thuộc:** M1, M7 (Booking Completed)  
> **Bảng DB:** `Reviews`

---

## 1. Mô Tả Tổng Quan

Sau mỗi buổi học hoàn thành, Student đánh giá Gia sư. Gia sư có thể phản hồi đánh giá. AI phát hiện đánh giá spam hoặc giả mạo.

---

## 2. Actors & Quyền Hạn

| Actor | Quyền |
|---|---|
| **Student** | Viết đánh giá sau buổi học |
| **Tutor** | Phản hồi đánh giá |
| **Admin** | Xóa đánh giá vi phạm, xem AI spam log |

---

## 3. Danh Sách Chức Năng

### UC-M10-01: Viết Đánh Giá

**Actor:** Student  
**Trigger:** Booking `Status = "Completed"`

**Thông tin:**
- Rating: 1–5 sao (bắt buộc)
- Comment: văn bản (tùy chọn, tối đa 500 ký tự)

**Business Rules:**
- Mỗi Booking chỉ được đánh giá 1 lần
- Có thể đánh giá trong vòng 7 ngày sau khi Completed

**Acceptance Criteria:**
- [ ] Gửi đánh giá thành công
- [ ] AverageRating của Tutor cập nhật ngay
- [ ] AI quét spam sau khi submit

---

### UC-M10-02: Hiển Thị Đánh Giá

**Mô tả:** Đánh giá hiển thị trên trang hồ sơ công khai của Gia sư.

**Giao diện:**
- Rating trung bình + số sao + tổng số đánh giá
- Danh sách đánh giá: ảnh Student, tên, sao, comment, ngày
- Phản hồi của Tutor (nếu có)

**Acceptance Criteria:**
- [ ] Hiển thị đúng, mới nhất lên trên
- [ ] Rating trung bình hiển thị làm tròn 1 chữ số thập phân

---

### UC-M10-03: Tutor Phản Hồi

**Actor:** Tutor  
**Mô tả:** Gia sư viết phản hồi cho đánh giá.

**Business Rules:**
- Chỉ phản hồi 1 lần mỗi đánh giá
- Tối đa 300 ký tự

**Acceptance Criteria:**
- [ ] Phản hồi hiển thị dưới đánh giá

---

### UC-M10-04: AI Phát Hiện Spam

**Mô tả:** AI tự động kiểm tra đánh giá có phải spam không.

**Dấu hiệu spam:**
- Nội dung không liên quan đến buổi học
- Từ ngữ xúc phạm
- Đánh giá sao không khớp với nội dung comment

**Business Rules:**
- Flag: `IsSpam = true`, ghi `AiSpamNote`
- Đánh giá bị flag vẫn hiển thị nhưng Admin được thông báo

**Acceptance Criteria:**
- [ ] AI flag đúng đánh giá spam
- [ ] Admin nhận notification khi có đánh giá bị flag

---

### UC-M10-05: Báo Cáo Đánh Giá

**Actor:** Tutor  
**Mô tả:** Gia sư báo cáo đánh giá không trung thực.

**Acceptance Criteria:**
- [ ] Báo cáo được gửi đến Admin
- [ ] Admin xem xét và xử lý

---

## 4. Out of Scope

- Student đánh giá nhiều lần
- Đánh giá ẩn danh
- Điểm uy tín (karma)

---

## 5. Task Breakdown

| Task ID | Tên Task | Ước tính |
|---|---|---|
| M10-T1 | Review Model + Migration | 30 phút |
| M10-T2 | Form viết đánh giá + UI sao | 45 phút |
| M10-T3 | Hiển thị đánh giá trên trang hồ sơ | 45 phút |
| M10-T4 | Tutor phản hồi đánh giá | 30 phút |
| M10-T5 | AI phát hiện spam | 45 phút |
