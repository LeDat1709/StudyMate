# FRS — Module 12: AI Learning Assistant

> **Module:** M12  
> **Phiên bản:** 1.0  
> **Phụ thuộc:** M1, M7 (Booking), M3 (Job)  
> **AI Tech:** Python FastAPI + OpenAI API / Ollama + RAG + Sentence Transformer

---

## 1. Mô Tả Tổng Quan

AI Learning Assistant là điểm khác biệt chiến lược của StudyMate so với các nền tảng tương tự. Cung cấp chatbot hỗ trợ học tập, gợi ý gia sư thông minh, tóm tắt bài học, tạo quiz từ tài liệu và đánh giá tiến độ.

---

## 2. Actors & Quyền Hạn

| Actor | Quyền |
|---|---|
| **Student** | Chat với AI, upload tài liệu, nhận quiz, xem tiến độ |
| **Tutor** | Upload tài liệu môn học cho AI học |
| **Admin** | Xem AI log, cấu hình model |

---

## 3. Danh Sách Chức Năng

### UC-M12-01: AI Chatbot Học Tập (RAG)

**Mô tả:** Student đặt câu hỏi học tập, AI trả lời dựa trên tài liệu môn học (RAG - Retrieval Augmented Generation).

**Luồng:**
1. Student upload tài liệu (PDF, DOCX) lên hệ thống
2. AI chunk + embed tài liệu, lưu vào vector store
3. Student đặt câu hỏi
4. AI tìm context liên quan từ vector store
5. Trả lời dựa trên context + LLM

**Tech:** LangChain + ChromaDB + OpenAI / Ollama

**Acceptance Criteria:**
- [ ] Câu trả lời liên quan đến tài liệu đã upload
- [ ] Có trích dẫn nguồn (tên tài liệu, trang)
- [ ] Response time < 10 giây

---

### UC-M12-02: AI Gợi Ý Gia Sư Thông Minh

**Mô tả:** AI gợi ý gia sư phù hợp dựa trên mục tiêu học tập của Student (IELTS, TOEIC, THPT...).

**Khác với M4:** M4 matching theo job cụ thể, M12 gợi ý theo mục tiêu dài hạn.

**Acceptance Criteria:**
- [ ] Nhập mục tiêu "Luyện IELTS 7.0 trong 3 tháng" → gợi ý gia sư phù hợp
- [ ] Giải thích lý do gợi ý

---

### UC-M12-03: AI Tóm Tắt Bài Học

**Mô tả:** Sau mỗi buổi học, AI tóm tắt nội dung dựa trên tài liệu và ghi chú.

**Trigger:** Booking `Status = "Completed"`

**Acceptance Criteria:**
- [ ] Tóm tắt 5–10 điểm chính của buổi học
- [ ] Gửi cho Student qua notification

---

### UC-M12-04: AI Tạo Quiz Từ Tài Liệu PDF

**Mô tả:** Student upload PDF, AI tự động tạo bộ câu hỏi trắc nghiệm.

**Loại câu hỏi:**
- Multiple choice (4 đáp án)
- True/False
- Fill in the blank

**Acceptance Criteria:**
- [ ] Upload PDF → tạo được 10–20 câu hỏi
- [ ] Student làm quiz, hệ thống chấm điểm tự động

---

### UC-M12-05: AI Đánh Giá Tiến Độ

**Mô tả:** AI phân tích lịch học, điểm quiz và đánh giá để đưa ra nhận xét tiến độ.

**Acceptance Criteria:**
- [ ] Dashboard tiến độ học tập của Student
- [ ] Đề xuất cải thiện cụ thể

---

### UC-M12-06: AI Phát Hiện Tin Nhắn Vi Phạm

**Mô tả:** (Dùng chung với M6-T8) AI phát hiện tin nhắn cố tình chia sẻ thông tin liên hệ để giao dịch ngoài hệ thống.

**Acceptance Criteria:**
- [ ] Phát hiện SĐT, Zalo, Facebook trong tin nhắn
- [ ] Cảnh báo người dùng và log cho Admin

---

## 4. Kiến Trúc AI Service Tổng Quát

```
ASP.NET Core MVC
       │
       │ HTTP / gRPC
       ▼
Python FastAPI (AI Gateway)
       │
       ├── Module Matching (M4)
       │     └── Sentence Transformer
       │
       ├── Module Chatbot (M12-01)
       │     └── RAG: LangChain + ChromaDB + OpenAI/Ollama
       │
       ├── Module Quiz (M12-04)
       │     └── OpenAI GPT / Ollama Mistral
       │
       └── Module Spam Detection (M6, M10, M12-06)
             └── Text classifier / Regex + LLM
```

---

## 5. Out of Scope

- AI chấm bài tự luận
- Nhận diện giọng nói
- AI tạo lộ trình học tập tự động hoàn toàn

---

## 6. Task Breakdown

| Task ID | Tên Task | Ước tính |
|---|---|---|
| M12-T1 | Setup Python FastAPI + cấu trúc project AI | 45 phút |
| M12-T2 | RAG pipeline: upload PDF + embed + ChromaDB | 90 phút |
| M12-T3 | Chatbot endpoint + tích hợp OpenAI/Ollama | 60 phút |
| M12-T4 | UI Chat với AI trên web | 60 phút |
| M12-T5 | Tạo Quiz từ PDF | 90 phút |
| M12-T6 | UI làm Quiz + chấm điểm | 60 phút |
