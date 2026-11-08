# Learning, progress, CSV, statistics and sharing

All routes use `/api/v1`, require a valid Bearer token and scope personal data to the authenticated user. Errors use Problem Details. Learners cannot call the moderation endpoint.

## Quiz (#28–30)

- `GET /lessons/{id}/quiz`: published lessons only; returns question ID, type, prompt, options and matching items. Does not return correctness, accepted answers, pair mappings or explanations.
- `POST /lessons/{id}/quiz/attempts`: `{ "attemptId": "UUID", "answers": [{ "questionId": "UUID", "answer": "..." }] }`. Send exactly one answer for each question. The server awards one point per completely correct answer. Response contains score, maximum score, explanations and correct answers.
- `GET /lessons/{id}/quiz/attempts`: current user's most recent 50 attempts with explanations.

Answer shapes:

| Type | Submission answer | Stored question ConfigJson |
| --- | --- | --- |
| MultipleChoice | Option UUID string | `{}`; correctness belongs to QuestionOption |
| FillInBlank | Text string | `{ "answers": ["hello", "hi"] }` |
| Matching | `{ "left-a": "right-2", "left-b": "right-1" }` | `{ "left": [{"id":"left-a","text":"Hello"}], "right": [{"id":"right-2","text":"Xin chào"}], "pairs": {"left-a":"right-2"} }` |

Fill-in answers use Unicode normalization, trimming and ordinal case-insensitive comparison. Matching requires all and only the configured pairs. A matching answer receives no partial credit. The UI plays a result tone when browser audio is available.

The client generates one attempt UUID per quiz run and reuses it after network failure. An identical replay returns the saved result; a different payload or owner returns 409. Concurrent first submissions may return 409; retry the original UUID. No new attempt is created by that retry. Start a new quiz run to intentionally submit another attempt.

## Progress (#31–32)

- `POST /courses/{id}/enroll`: idempotent enrollment for a published course.
- `PATCH /lessons/{id}/progress`: `{ "status": "NotStarted|InProgress|Completed" }`; implicitly enrolls in the course.
- `GET /me/progress`: enrolled published courses, ordered lesson statuses, completion percentage and most recently accessed unfinished lesson.

Completion cannot be downgraded by a delayed retry; the initial completion timestamp is retained. Status uses optimistic concurrency so a concurrent update cannot overwrite a newer completion. Unique user/lesson and user/course database indexes prevent duplicate records. A concurrent duplicate insert or stale update returns 409 and can be retried. The lesson screen records access and exposes completion and Quiz actions.

## CSV (#33–34)

- `GET /csv/template`: UTF-8 CSV with `front,back` header.
- `POST /csv/preview`: `{ "content": "front,back\nHello,Xin chào" }`; returns physical line numbers, per-row errors and valid/invalid counts.
- `POST /decks/{id}/csv/import`: same body; owner only. All rows must pass validation. Adds the batch and increments the deck version in one EF SaveChanges transaction.
- `GET /decks/{id}/csv/export`: owner-only UTF-8 CSV with BOM.

Limits: 2 MB source file, 5000 cards, 10000 characters per side. The browser decodes UTF-8 strictly; the API rejects replacement and NUL characters. Quoted delimiters, doubled quotes, CRLF and multiline fields are supported. Duplicate rows in a batch are rejected. Exports quote and escape every field and prefix formula-like values with an apostrophe, including formulas preceded by whitespace. Import is an explicit append action; intentionally confirming the same file again appends another batch.

## Statistics and streak (#37–40)

- `GET /statistics/overview?from=2026-11-01&to=2026-11-08`: new/learned/due owned cards, review count, rating distribution and retention proxy.
- `GET /statistics/heatmap?from=...&to=...`: activity dates in the user's timezone, review and completed-lesson counts.
- `GET /me/streak`: current/longest streak, today's count, timezone, daily target and activity calendar.
- `PATCH /me/goal`: `{ "dailyTarget": 10 }`, from 1 to 1000.

Date boundaries are inclusive in the user's saved timezone. Review events are deduplicated by client event UUID. The daily goal counts reviews, not lesson completions; completed lessons appear separately in the calendar. A streak survives during the current day if yesterday qualified, then resets after a missed full day. Changing timezone or goal recalculates historical qualification with the current settings. The retention figure is the share of Hard/Good/Easy ratings and is not a measured long-term retention estimate. Pending offline reviews are excluded until synchronized. All-time card counts are independent of the selected review date range.

## Community (#41–43)

- `GET /public/decks?page=1&pageSize=10&search=...&languageId=UUID`: paginated active-language public decks; excludes deleted/private decks.
- `GET /public/decks/{id}`: metadata and up to five sample cards. Private/deleted decks return 404.
- `POST /public/decks/{id}/clone`: creates a private deck with fresh deck/card/tag IDs, authenticated owner and `SourceDeckId`. Copies card content/media references, not review history or schedules. This is an intentional create action; avoid retrying blindly after ambiguous network failure.
- `POST /admin/decks/{id}/hide`: Admin policy only; hides the deck, increments its version and records an audit log in the same save.

## UI entry points

`/learning`, `/statistics`, `/community`, `/lessons/{id}/quiz` and `/decks/{id}/csv` are protected routes. The home sidebar links to the new screens. Deck cards link to CSV import/export; lesson screens link to Quiz and progress.

## Verification (#53–54)

```powershell
dotnet build backend/LanguageLearning.sln --configuration Release
dotnet test backend/LanguageLearning.sln --configuration Release
cd frontend
npm ci
npm test
npx playwright install chromium
npm run test:e2e
npm run build
```

Full backend tests require a separate migrated MySQL database named `language_learning_test_db`. Set `ISSUE27_TEST_CONNECTION` and `ISSUE50_TEST_CONNECTION` to that database; never point tests at personal application data. New rule/integration tests also run independently against SQLite. Browser tests mock API responses to verify UI routing, form submission and preview/confirmation behavior; backend API tests independently verify real authorization and persistence.
