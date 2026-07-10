# Mini Plan – Group N2: Làm Việc Với AI Agent Code

## Mục tiêu tuần này

Trong tuần này, Group N2 cần **hiểu và vận hành được quy trình làm việc với AI Agent Code** theo chuẩn của MeU.

Mục tiêu không phải chỉ là "biết prompt AI", mà là biết cách:
- Biến yêu cầu thành plan rõ ràng
- Chia task đủ nhỏ
- Tạo prompt đúng scope
- Giao cho agent triển khai
- Review kết quả, test, báo cáo thay đổi và quản lý rủi ro

Sau tuần này, mỗi thành viên trong Group N2 cần có khả năng **tham gia quy trình phát triển với AI Agent Code một cách có kiểm soát**.

---

## Kết quả cần đạt

Cuối tuần, Group N2 cần chứng minh được các năng lực sau:

### 1. Hiểu mindset làm việc của team

| Mindset | Mô tả |
|---|---|
| **Self-Trust** | Tự hiểu task, tự chịu trách nhiệm với phần việc của mình |
| **Team Trust** | Cập nhật tiến độ rõ ràng, báo blocker sớm, phối hợp với nhóm |
| **System Trust** | Làm việc theo quy trình: có plan, có prompt, có review, có report |

### 2. Hiểu flow làm việc với AI Agent Code

1. Làm rõ yêu cầu
2. Lập plan triển khai
3. Chia task nhỏ **30–90 phút**
4. Tạo file `prompt.xml` cho từng task
5. Review prompt trước khi giao agent
6. Agent đọc hiểu kiến trúc/source
7. Agent code trong scope
8. Engineer review, test, build
9. Báo cáo thay đổi, kết quả và rủi ro

### 3. Thực hành kỹ thuật

Thực hành được **ít nhất 1 task kỹ thuật nhỏ** theo đúng quy trình.

### 4. Tài liệu tổng kết

Có tài liệu hoặc note tổng kết cách Group N2 sẽ vận hành quy trình này trong các task tiếp theo.

---

## Phạm vi công việc

### 1. Đọc và hiểu quy trình làm việc với AI Agent Code

Group N2 cần đọc flow/quy trình được cung cấp và nắm rõ từng bước.

Cần trả lời được:

- Vì sao **không giao task lớn** trực tiếp cho agent?
- Vì sao mỗi prompt nên nằm trong khoảng **30–90 phút**?
- Vì sao cần **FRS/Requirement** trước khi tạo prompt?
- Vì sao agent cần **đọc kiến trúc/source** trước khi code?
- Vì sao engineer vẫn phải **review, test, build** sau khi agent code?
- Vì sao phải **báo cáo thay đổi, kết quả, rủi ro** sau mỗi task?

---

### 2. Chuẩn hóa cách chia task

Mỗi thành viên cần thực hành chia một yêu cầu kỹ thuật thành các task nhỏ.

**Mỗi task nhỏ cần có:**

| Trường | Mô tả |
|---|---|
| Tên task | Rõ ràng, cụ thể |
| Mục tiêu | Kết quả cần đạt |
| Scope cho phép | Được làm gì |
| Scope không được làm | Không được đụng vào gì |
| Output cần có | File, component, function... |
| Acceptance Criteria | Điều kiện hoàn thành |
| Test Plan | Cách kiểm tra kết quả |
| Ước lượng thời gian | 30–90 phút |

**Không được tạo task mơ hồ như:**

```
❌ "Làm module user"
❌ "Fix toàn bộ UI"
❌ "Tối ưu hệ thống"
❌ "Refactor source"
```

**Cần chuyển thành task nhỏ như:**

```
✅ "Kiểm tra và cập nhật column table theo response API"
✅ "Tạo guard kiểm tra quyền truy cập POS page"
✅ "Tách component filter trong màn hình báo cáo"
✅ "Fix mapping data schedule theo employee_id và date"
```

---

### 3. Thực hành tạo prompt.xml

Mỗi thành viên cần tạo **ít nhất 1 file prompt** theo cấu trúc chuẩn.

**Prompt cần có tối thiểu:**

```xml
<prompt>
  <meta />
  <goal />
  <frsReference />          <!-- hoặc requirement reference -->
  <planReference />
  <taskUnit />
  <context />
  <scope />
  <hardRules />
  <tasks />
  <constraints />
  <acceptanceCriteria />
  <testPlan />
  <outputRequired />
</prompt>
```

**Prompt phải đủ rõ để agent biết:**

- Cần làm gì
- Đọc file nào
- Không được sửa phần nào
- Output cuối cùng là gì
- Cách kiểm tra kết quả
- Cần báo cáo lại những gì

---

### 4. Thực hành chạy quy trình với 1 task nhỏ

Group N2 chọn **1 task kỹ thuật nhỏ** để chạy thử quy trình.

**Flow bắt buộc:**

```
1. Nhận requirement
2. Lập mini plan
3. Chia task nhỏ
4. Tạo prompt.xml
5. Review prompt trong nhóm
6. Cho agent đọc hiểu source/kiến trúc
7. Agent triển khai trong scope
8. Engineer review code
9. Engineer chạy test/build nếu có
10. Ghi nhận kết quả và rủi ro
```

> Task thực hành nên **nhỏ, độc lập, không ảnh hưởng production**.

**Ví dụ task phù hợp:**

- Mapping lại column table từ API response
- Cập nhật label/status hiển thị
- Fix UI nhỏ có ảnh chụp minh chứng
- Tách helper function nhỏ
- Viết checklist test cho một màn hình
- Đọc một module và viết summary kỹ thuật

---

### 5. Tổng kết mindset làm việc

Group N2 cần viết **note ngắn về mindset làm việc** khi dùng AI Agent Code.

**Nội dung cần có:**

- AI Agent **không thay thế** trách nhiệm của engineer
- Engineer phải **hiểu requirement** trước khi prompt
- Engineer phải **kiểm soát scope**
- Engineer phải **review code** trước khi accept
- Engineer phải **test/build** trước khi báo Done
- Engineer phải **giải thích được** thay đổi agent đã làm
- **Không merge hoặc handoff** khi chưa hiểu thay đổi

---

## Deliverables cuối tuần

Cuối tuần Group N2 cần nộp:

| # | Deliverable |
|---|---|
| 1 | File/note tổng hợp quy trình làm việc với AI Agent Code |
| 2 | Ít nhất 1 ví dụ task đã được chia nhỏ theo chuẩn 30–90 phút |
| 3 | Ít nhất 1 file `prompt.xml` mẫu |
| 4 | Kết quả thực hành 1 task nhỏ với agent |
| 5 | Báo cáo ngắn (xem format bên dưới) |

---

## Acceptance Criteria

Task được xem là **hoàn thành** khi:

- [ ] Group N2 giải thích được toàn bộ flow làm việc với AI Agent Code
- [ ] Mỗi thành viên hiểu vì sao task phải nhỏ, rõ scope và có acceptance criteria
- [ ] Có ít nhất 1 prompt mẫu đạt chuẩn
- [ ] Có ít nhất 1 task nhỏ được chạy thử theo quy trình
- [ ] Engineer review được output của agent, không chỉ copy/paste kết quả
- [ ] Có test/build hoặc checklist kiểm tra sau khi agent code
- [ ] Có report cuối tuần rõ ràng
- [ ] Team lead/EM có thể dùng kết quả này để giao task kỹ thuật thật cho Group N2 ở tuần sau

---

## Weekly Report Format

### 1. Completed this week

- Đã đọc và hiểu những phần nào của quy trình
- Đã tạo task nhỏ nào
- Đã tạo `prompt.xml` nào
- Đã thực hành với agent trên task nào

### 2. Key learning

- Em hiểu gì về cách làm việc với AI Agent Code
- Em hiểu gì về việc chia task 30–90 phút
- Em hiểu gì về trách nhiệm review/test của engineer

### 3. Issues / Blockers

- Phần nào chưa rõ
- Prompt phần nào còn khó viết
- Khi agent code có vấn đề gì
- Cần EM/Lead hỗ trợ gì

### 4. Plan for next week

- Áp dụng quy trình vào task kỹ thuật thật
- Cải thiện `prompt.xml`
- Chuẩn hóa checklist review/test
- Báo cáo kết quả theo từng task
