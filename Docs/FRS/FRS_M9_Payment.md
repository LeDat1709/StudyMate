# FRS — Module 9: Thanh Toán

> **Module:** M9  
> **Phiên bản:** 1.0  
> **Phụ thuộc:** M1, M7 (Booking Completed)  
> **Bảng DB:** `Wallets`, `Transactions`

---

## 1. Mô Tả Tổng Quan

Module thanh toán quản lý ví điện tử của từng user, tích hợp cổng thanh toán VNPay/MoMo/Stripe, xử lý giao dịch nạp tiền, thanh toán buổi học, hoàn tiền và lịch sử giao dịch.

---

## 2. Actors & Quyền Hạn

| Actor | Quyền |
|---|---|
| **Student** | Nạp tiền, Thanh toán buổi học, Xem lịch sử |
| **Tutor** | Xem doanh thu, Rút tiền |
| **Admin** | Xem tất cả giao dịch, Xử lý hoàn tiền thủ công |

---

## 3. Danh Sách Chức Năng

### UC-M9-01: Ví Điện Tử

**Mô tả:** Mỗi user có 1 ví, có thể nạp/rút/thanh toán.

**Acceptance Criteria:**
- [ ] Mỗi user tạo tài khoản → tự động tạo Wallet `Balance = 0`
- [ ] Số dư hiển thị trên navbar

---

### UC-M9-02: Nạp Tiền

**Mô tả:** Student nạp tiền vào ví qua cổng thanh toán.

**Cổng hỗ trợ:** VNPay / MoMo / Stripe

**Luồng:**
1. Chọn số tiền nạp
2. Chọn cổng thanh toán
3. Redirect đến trang thanh toán của cổng
4. Callback → xác nhận giao dịch
5. Cập nhật Balance, tạo Transaction

**Acceptance Criteria:**
- [ ] Nạp thành công → Balance cập nhật ngay
- [ ] Giao dịch lưu vào `Transactions`

---

### UC-M9-03: Thanh Toán Buổi Học

**Mô tả:** Student thanh toán cho Tutor sau khi Booking `Completed`.

**Luồng:**
1. Booking `Completed`
2. Hệ thống trừ `Amount` từ Wallet Student
3. Cộng `Amount * (1 - platformFee%)` vào Wallet Tutor
4. Tạo 2 Transaction: Payment (Student) + Income (Tutor)

**Business Rules:**
- Platform fee: 10% (cấu hình được)
- Student phải có đủ số dư

**Acceptance Criteria:**
- [ ] Balance cả 2 bên cập nhật chính xác
- [ ] 2 transaction được ghi nhận

---

### UC-M9-04: Hoàn Tiền

**Mô tả:** Hoàn tiền khi buổi học bị hủy hoặc có tranh chấp.

**Business Rules:**
- Hủy trước 2 giờ → hoàn 100%
- Hủy trong 2 giờ → hoàn 50%
- Admin có thể override

**Acceptance Criteria:**
- [ ] Hoàn tiền vào Wallet Student
- [ ] Transaction ghi nhận loại `Refund`

---

### UC-M9-05: Lịch Sử Giao Dịch

**Mô tả:** Xem tất cả giao dịch của tài khoản.

**Filter:** Loại (Deposit/Payment/Refund) / Trạng thái / Khoảng thời gian

**Acceptance Criteria:**
- [ ] Hiển thị đúng lịch sử giao dịch
- [ ] Phân trang

---

### UC-M9-06: Doanh Thu Gia Sư

**Mô tả:** Tutor xem thống kê doanh thu theo tháng.

**Acceptance Criteria:**
- [ ] Biểu đồ doanh thu theo tháng
- [ ] Tổng doanh thu, số buổi dạy

---

### UC-M9-07: Rút Tiền

**Mô tả:** Tutor rút tiền từ ví về tài khoản ngân hàng.

**Business Rules:**
- Tối thiểu 100,000 VNĐ mỗi lần rút
- Admin xử lý thủ công hoặc tích hợp API ngân hàng

**Acceptance Criteria:**
- [ ] Yêu cầu rút tiền được ghi nhận
- [ ] Admin duyệt → balance trừ, transaction ghi nhận

---

## 4. Out of Scope

- Hóa đơn PDF tự động
- Tích hợp kế toán
- Multi-currency

---

## 5. Task Breakdown

| Task ID | Tên Task | Ước tính |
|---|---|---|
| M9-T1 | Wallet + Transaction Model + Migration | 30 phút |
| M9-T2 | Tạo Wallet tự động khi đăng ký | 30 phút |
| M9-T3 | Tích hợp VNPay | 90 phút |
| M9-T4 | Tích hợp MoMo | 90 phút |
| M9-T5 | Logic thanh toán sau Booking Complete | 60 phút |
| M9-T6 | Logic hoàn tiền | 45 phút |
| M9-T7 | Lịch sử giao dịch | 45 phút |
| M9-T8 | Trang doanh thu Tutor | 45 phút |
