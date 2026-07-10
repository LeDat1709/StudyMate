# FRS — Module 4: AI Matching

> **Module:** M4  
> **Phiên bản:** 1.0  
> **Phụ thuộc:** M1, M2 (TutorProfile), M3 (JobPosting)  
> **Bảng DB:** `MatchingResults`  
> **AI Service:** Python FastAPI + Sentence Transformer / Embedding + Cosine Similarity

---

## 1. Mô Tả Tổng Quan

AI Matching là tính năng cốt lõi giúp hệ thống tự động ghép nối Gia sư và Học viên phù hợp nhất dựa trên nội dung hồ sơ, môn học, trình độ và yêu cầu. Sử dụng Sentence Transformer để tạo embedding và Cosine Similarity để tính điểm tương đồng.

---

## 2. Actors & Quyền Hạn

| Actor | Quyền |
|---|---|
| **Student** | Xem danh sách gia sư được AI gợi ý cho job của mình |
| **Tutor** | Xem danh sách job được AI gợi ý phù hợp với hồ sơ |
| **System** | Tự động chạy matching khi có job mới hoặc hồ sơ mới |
| **Admin** | Xem AI log, kiểm tra chất lượng matching |

---

## 3. Danh Sách Chức Năng

### UC-M4-01: AI Gợi Ý Gia Sư Cho Job

**Actor:** Student (trigger), System (xử lý)  
**Mô tả:** Khi học viên tạo job mới, AI tự động gợi ý danh sách gia sư phù hợp nhất.

**Luồng chính:**
1. Student tạo job mới (M3)
2. Hệ thống gọi Python FastAPI: `POST /api/matching/job-to-tutors`
3. AI xử lý:
   - Encode text job (tiêu đề + mô tả + môn + trình độ mong muốn) thành embedding
   - Encode text mỗi TutorProfile (headline + bio + môn dạy + chứng chỉ) thành embedding
   - Tính Cosine Similarity giữa job và từng tutor
   - Trả về top 10 tutor có score cao nhất
4. Lưu kết quả vào `MatchingResults`
5. Hiển thị danh sách gợi ý cho Student trên trang chi tiết job

**Output mỗi kết quả:**
- TutorProfileId
- SimilarityScore (0.0 → 1.0)
- Rank (1 → 10)

**Business Rules:**
- Chỉ matching với Tutor `IsAvailable = true` và `IsVerified = true`
- Timeout API call: 10 giây
- Nếu AI service không phản hồi → hiển thị danh sách fallback theo filter thủ công

**Acceptance Criteria:**
- [ ] Sau khi tạo job, trong vòng 15 giây có danh sách gợi ý
- [ ] Kết quả lưu vào `MatchingResults`
- [ ] Hiển thị top 10 gia sư với score và rank
- [ ] Fallback hoạt động khi AI service down

---

### UC-M4-02: AI Gợi Ý Job Cho Gia Sư

**Actor:** Tutor  
**Mô tả:** Gia sư xem danh sách job phù hợp với hồ sơ của mình.

**Luồng chính:**
1. Tutor vào trang `/Tutor/RecommendedJobs`
2. Hệ thống gọi Python FastAPI: `POST /api/matching/tutor-to-jobs`
3. AI tính similarity giữa hồ sơ tutor và các job đang `Open`
4. Trả về top 10 job phù hợp nhất
5. Hiển thị với điểm phù hợp

**Acceptance Criteria:**
- [ ] Tutor thấy danh sách job phù hợp được sắp xếp theo điểm
- [ ] Kết quả cập nhật khi Tutor cập nhật hồ sơ

---

### UC-M4-03: Ranking & Recommendation Tổng Quát

**Actor:** Guest, Student (trang home)  
**Mô tả:** Trang chủ hiển thị danh sách gia sư nổi bật và gợi ý dựa trên lịch sử.

**Business Rules:**
- Guest: gợi ý theo gia sư có rating cao nhất
- Student đã đăng nhập: gợi ý dựa trên lịch sử job và môn học

**Acceptance Criteria:**
- [ ] Trang chủ hiển thị tối thiểu 6 gia sư gợi ý
- [ ] Student đăng nhập thấy gợi ý cá nhân hóa

---

### UC-M4-04: Xem Điểm Phù Hợp

**Actor:** Student, Tutor  
**Mô tả:** Hiển thị badge/score phù hợp trên card gia sư hoặc card job.

**Giao diện:**
- Badge màu xanh "Phù hợp 92%" trên card tutor trong trang gợi ý
- Tooltip giải thích ngắn: "Phù hợp dựa trên môn học và trình độ"

**Acceptance Criteria:**
- [ ] Score hiển thị dạng phần trăm, làm tròn số nguyên
- [ ] Tooltip xuất hiện khi hover

---

## 4. Kiến Trúc AI Service

```
ASP.NET Core MVC
       │
       │ HTTP POST (JSON)
       ▼
Python FastAPI (AI Service)
       │
       ├── Sentence Transformer Model
       │     └── all-MiniLM-L6-v2 (hoặc multilingual)
       │
       ├── Encode text → vector embedding
       │
       └── Cosine Similarity → score → rank → trả về JSON
```

**Request format:**
```json
{
  "job": {
    "title": "Cần gia sư IELTS 7.5+",
    "description": "...",
    "subject": "IELTS",
    "desiredLevel": "7.5+"
  },
  "tutors": [
    {
      "tutorProfileId": 1,
      "headline": "Gia sư IELTS 8.0+",
      "bio": "...",
      "subjects": ["IELTS", "Tiếng Anh"],
      "certificates": ["IELTS 8.0"]
    }
  ]
}
```

**Response format:**
```json
{
  "results": [
    { "tutorProfileId": 1, "score": 0.9234, "rank": 1 },
    { "tutorProfileId": 5, "score": 0.8871, "rank": 2 }
  ],
  "modelVersion": "all-MiniLM-L6-v2",
  "processingMs": 234
}
```

---

## 5. Out of Scope

- Collaborative filtering (lọc theo hành vi người dùng khác)
- A/B testing matching algorithm
- Training model tùy chỉnh

---

## 6. Task Breakdown

| Task ID | Tên Task | Ước tính |
|---|---|---|
| M4-T1 | MatchingResults Model + Migration | 30 phút |
| M4-T2 | Python FastAPI service cơ bản + Sentence Transformer | 90 phút |
| M4-T3 | API endpoint: job-to-tutors matching | 60 phút |
| M4-T4 | API endpoint: tutor-to-jobs matching | 60 phút |
| M4-T5 | Gọi AI service từ ASP.NET khi tạo Job | 45 phút |
| M4-T6 | Hiển thị kết quả gợi ý trên trang Job và Tutor | 60 phút |
