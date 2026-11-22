import { expect, test } from '@playwright/test';

test.beforeEach(async ({ page }) => {
  const payload = Buffer.from(JSON.stringify({ sub: 'learner', email: 'learner@example.com', role: 'Learner', exp: Math.floor(Date.now() / 1000) + 3600 })).toString('base64url');
  await page.addInitScript(() => sessionStorage.setItem('auth_refresh_token', 'e2e-refresh'));
  await page.route('**/api/v1/**', async route => {
    const path = new URL(route.request().url()).pathname;
    if (route.request().method() === 'OPTIONS') return route.fulfill({ status: 204, headers: { 'Access-Control-Allow-Origin': '*', 'Access-Control-Allow-Headers': '*', 'Access-Control-Allow-Methods': '*' } });
    let body: unknown = [];
    if (path.endsWith('/auth/refresh')) body = { accessToken: `e2e.${payload}.test`, refreshToken: 'e2e-refresh' };
    if (path.endsWith('/me/streak')) body = { currentStreak: 2, longestStreak: 3, dailyTarget: 10, todayReviews: 2, timezone: 'Asia/Ho_Chi_Minh', days: [] };
    if (path.endsWith('/quiz')) body = [{ id: 'q1', type: 'MultipleChoice', prompt: 'Choose a greeting', options: [{ id: 'a1', content: 'Hello' }, { id: 'a2', content: 'Goodbye' }], left: [], right: [] }];
    if (path.endsWith('/quiz/attempts') && route.request().method() === 'POST') {
      const request = route.request().postDataJSON();
      expect(request.answers).toEqual([{ questionId: 'q1', answer: 'a1' }]);
      body = { id: request.attemptId, lessonId: 'lesson', score: 1, maxScore: 1, completedAt: new Date().toISOString(), answers: [{ questionId: 'q1', isCorrect: true, correctAnswer: 'Hello', explanation: 'A greeting.' }] };
    }
    if (path.endsWith('/public/decks')) body = { items: [{ id: 'deck', languageId: 'en', title: 'Community A1', description: 'English', cardCount: 1, tags: [] }], page: 1, totalPages: 1, hasNextPage: false, hasPreviousPage: false };
    if (path.endsWith('/public/decks/deck')) body = { deck: { title: 'Community A1' }, cards: [{ front: 'Hello', back: 'Xin chào' }] };
    if (path.endsWith('/public/decks/deck/clone')) body = { id: 'cloned-deck' };
    if (path === '/api/v1/decks') body = { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0, hasNextPage: false, hasPreviousPage: false };
    if (path.endsWith('/csv/preview')) body = { validCount: 1, invalidCount: 0, rows: [{ line: 2, front: 'Hello', back: 'Xin chào', error: null }] };
    if (path.endsWith('/csv/import')) body = { importedCount: 1 };
    await route.fulfill({ json: body, headers: { 'Access-Control-Allow-Origin': '*' } });
  });
});

test('learner answers a quiz and sees the server score', async ({ page }) => {
  await page.goto('/lessons/lesson/quiz');
  await expect(page.getByRole('button', { name: 'Nộp bài' })).toBeDisabled();
  await page.getByRole('radio', { name: 'Hello', exact: true }).check();
  await page.getByRole('button', { name: 'Nộp bài' }).click();
  await expect(page.getByRole('heading', { name: 'Điểm: 1/1' })).toBeVisible();
  await expect(page.getByText(/A greeting/)).toBeVisible();
});

test('community search opens a public deck preview', async ({ page }) => {
  await page.goto('/community');
  await page.getByLabel('Tìm bộ thẻ').fill('A1');
  await page.getByRole('button', { name: 'Xem trước', exact: true }).click();
  await expect(page.getByRole('region', { name: 'Xem trước bộ thẻ' })).toContainText('Xin chào');
});

test('CSV preview requires confirmation before importing', async ({ page }) => {
  await page.goto('/decks/deck/csv/import');
  await page.getByLabel(/Chọn file CSV/).setInputFiles({ name: 'cards.csv', mimeType: 'text/csv', buffer: Buffer.from('front,back\nHello,Xin chào') });
  await expect(page.getByText('1 dòng hợp lệ · 0 dòng lỗi')).toBeVisible();
  await page.getByRole('button', { name: 'Xác nhận nhập thẻ' }).click();
  await expect(page.getByRole('status')).toContainText('Đã nhập 1 thẻ.');
});

test('cloning a community deck returns the learner to My Decks', async ({ page }) => {
  await page.goto('/community');
  await page.getByRole('button', { name: 'Sao chép về bộ thẻ của tôi' }).click();
  await expect(page).toHaveURL(/\?view=myDecks$/);
});
