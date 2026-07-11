# Break Task — Module 3: Job Posting

> **Module:** M3  
> **FRS:** `Docs/FRS/FRS_M3_JobPosting.md`  
> **Branch:** `feat/m3-job-posting`  
> **Phụ thuộc:** M1 Auth, M2 Subjects (seed)

## Dependency

```
M3-T1 (Model + Migration)
  └──▶ M3-T2 (Create)
          ├──▶ M3-T3 (Public list + paging)
          │         └──▶ M3-T6 (Search & filter)
          ├──▶ M3-T4 (Details)
          ├──▶ M3-T5 (Edit / Close / Delete)
          └──▶ M3-T7 (My Jobs)
M3-T1 ──▶ M3-T8 (Auto-expire)  ← gọi từ list/details/myjobs
```

## Danh sách Task

| Task | Name | Est | UC |
|------|------|-----|-----|
| M3-T1 | JobPosting model + DbContext + Migration | 30m | — |
| M3-T2 | Create job (Student) + validation | 60m | UC-01 |
| M3-T3 | Public list + pagination (10/page) | 45m | UC-02 |
| M3-T4 | Job details page | 45m | UC-03 |
| M3-T5 | Edit / Close / Delete | 45m | UC-04,05 |
| M3-T6 | Search & filter + query string | 60m | UC-06 |
| M3-T7 | My Jobs (Student tabs) | 45m | UC-07 |
| M3-T8 | Auto-expire Open→Expired by deadline | 30m | BR |

## Out of scope M3

- Apply (M5), AI Matching (M4), Admin hide job (M8), SignalR

## Rules

- Max 5 jobs Status=Open per student
- Offline/Both → Address required
- Public list: only Open (after expire pass)
- Delete blocked if has applications — M3: no Applications table yet → delete always allowed; note for M5
