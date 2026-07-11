# FRS — Module 6: Chat Realtime

> **Module:** M6  
> **Phiên bản:** 1.0  
> **Phụ thuộc:** M1, M5 (Application accepted)  
> **Bảng DB:** `Conversations`, `Messages`  
> **Tech:** SignalR

---

## 1. Mô Tả Tổng Quan

Module chat realtime cho phép Student và Tutor nhắn tin sau khi Application được chấp nhận. Hỗ trợ gửi text, hình ảnh, file PDF, emoji, và các trạng thái seen/typing/online.

---

## 2. Actors & Quyền Hạn

| Actor | Quyền |
|---|---|
| **Student** | Chat với Tutor đã accept |
| **Tutor** | Chat với Student của mình |
| **Admin** | Xem log tin nhắn bị báo cáo |

---

## 3. Danh Sách Chức Năng

### UC-M6-01: Gửi Tin Nhắn Text

**Actor:** Student, Tutor  
**Mô tả:** Gửi và nhận tin nhắn text realtime qua SignalR.

**Acceptance Criteria:**
- [ ] Tin nhắn hiển thị ngay phía người nhận không cần refresh
- [ ] Hiển thị thời gian gửi
- [ ] Tin nhắn được lưu vào DB

---

### UC-M6-02: Gửi Hình Ảnh

**Business Rules:**
- Định dạng: JPG, PNG, GIF, WEBP
- Kích thước: ≤ 5MB
- Hiển thị thumbnail inline trong chat

**Acceptance Criteria:**
- [ ] Ảnh hiển thị inline, click để xem full
- [ ] File quá lớn → hiển thị lỗi

---

### UC-M6-03: Gửi File PDF

**Business Rules:**
- Kích thước: ≤ 20MB
- Hiển thị tên file + icon PDF + nút download

**Acceptance Criteria:**
- [ ] File PDF gửi được, người nhận download được

---

### UC-M6-04: Emoji

**Mô tả:** Chèn emoji vào tin nhắn qua emoji picker.

**Acceptance Criteria:**
- [ ] Emoji picker hiện khi nhấn icon 😊
- [ ] Emoji hiển thị đúng trên cả 2 phía

---

### UC-M6-05: Trạng Thái Đã Xem (Seen)

**Mô tả:** Hiển thị tick "Đã xem" khi người nhận đã đọc tin nhắn.

**Business Rules:**
- Tin nhắn được đánh dấu `IsRead = true` khi người nhận mở conversation
- Hiển thị "Đã xem lúc HH:mm" dưới tin nhắn cuối

**Acceptance Criteria:**
- [ ] Tick seen hiển thị sau khi người nhận mở chat
- [ ] `IsRead` cập nhật trong DB

---

### UC-M6-06: Trạng Thái Đang Gõ (Typing...)

**Mô tả:** Hiển thị "Đang gõ..." khi đối phương đang nhập tin nhắn.

**Business Rules:**
- Broadcast typing event qua SignalR
- Tự động ẩn sau 3 giây nếu không gõ tiếp

**Acceptance Criteria:**
- [ ] "Đang gõ..." hiển thị realtime
- [ ] Tự ẩn sau 3 giây không gõ

---

### UC-M6-07: Trạng Thái Online

**Mô tả:** Hiển thị chấm xanh khi user đang online.

**Business Rules:**
- User online: đang có kết nối SignalR active
- Cập nhật realtime khi connect/disconnect

**Acceptance Criteria:**
- [ ] Chấm xanh hiển thị khi user online
- [ ] Chấm xám khi offline

---

### UC-M6-08: Danh Sách Cuộc Trò Chuyện

**Mô tả:** Trang `/Chat` hiển thị tất cả conversation của user.

**Giao diện:**
- Sidebar trái: danh sách conversation (ảnh, tên, tin nhắn cuối, thời gian)
- Vùng phải: nội dung chat hiện tại
- Badge số tin nhắn chưa đọc

**Acceptance Criteria:**
- [ ] Danh sách sắp xếp theo tin nhắn mới nhất
- [ ] Badge số chưa đọc hiển thị đúng

---

### UC-M6-09: AI Phát Hiện Tin Nhắn Lừa Đảo

**Mô tả:** AI quét tin nhắn có chứa số điện thoại, link ngoài hệ thống, thông tin liên hệ để giao dịch ngoài.

**Business Rules:**
- Tin nhắn bị flag: `IsFlagged = true`, `AiFlagNote` ghi lý do
- Hiển thị cảnh báo nhẹ cho người gửi: "Hệ thống phát hiện tin nhắn có thể vi phạm chính sách"
- Admin xem được danh sách tin nhắn bị flag

**Acceptance Criteria:**
- [ ] Tin nhắn chứa SĐT/link bị flag
- [ ] Cảnh báo hiển thị cho người gửi
- [ ] Admin xem được log

---

## 4. Out of Scope

- Video call
- Voice message
- Chat nhóm (group chat)
- Xóa/thu hồi tin nhắn

---

## 5. Task Breakdown

| Task ID | Tên Task | Ước tính |
|---|---|---|
| M6-T1 | Conversation + Message Model + Migration | 30 phút |
| M6-T2 | Setup SignalR Hub (ChatHub) | 45 phút |
| M6-T3 | Giao diện chat (layout sidebar + chat area) | 90 phút |
| M6-T4 | Gửi/nhận tin nhắn text realtime | 60 phút |
| M6-T5 | Upload ảnh + file PDF trong chat | 60 phút |
| M6-T6 | Emoji picker | 30 phút |
| M6-T7 | Seen / Typing / Online status | 60 phút |
| M6-T8 | AI phát hiện tin nhắn lừa đảo | 45 phút |
