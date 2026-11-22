import { useState } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { QueryStatus } from './components/LearningLayout';
import { QuizSession } from './QuizPage';
import { ProgressPage } from './ProgressPage';
import { Flashcard } from '../flashcards/components/Flashcard';
import { learningApi } from './api/quizApi';
import { learningApi as progressApi } from './api/progressApi';
import { ActivityCalendar } from './components/ActivityCalendar';

vi.mock('./api/quizApi', () => ({ learningApi: { quiz: vi.fn(), submit: vi.fn(), history: vi.fn() } }));
vi.mock('./api/progressApi', () => ({ learningApi: { progress: vi.fn() } }));

function renderPage(page: React.ReactNode) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } });
  return render(<QueryClientProvider client={client}><MemoryRouter>{page}</MemoryRouter></QueryClientProvider>);
}

describe('Learning states', () => {
  it('fills inactive days in the calendar and exposes activity counts accessibly', () => {
    render(<ActivityCalendar from="2026-11-06" to="2026-11-08" days={[{ date: '2026-11-07', reviewCount: 10, lessonsCompleted: 1, goalMet: true }]} />);
    expect(screen.getAllByRole('listitem')).toHaveLength(3);
    expect(screen.getByRole('listitem', { name: '2026-11-07: 10 lượt ôn, 1 bài hoàn thành' })).toHaveClass('bg-green-700');
    expect(screen.getByRole('listitem', { name: '2026-11-08: 0 lượt ôn, 0 bài hoàn thành' })).toBeInTheDocument();
  });
  it('renders loading, empty and retryable error states', async () => {
    const retry = vi.fn();
    const view = render(<QueryStatus pending error={null} empty={false} />);
    expect(screen.getByRole('status')).toHaveTextContent('Đang tải');
    view.rerender(<QueryStatus pending={false} error={null} empty />);
    expect(screen.getByText('Chưa có dữ liệu.')).toBeInTheDocument();
    view.rerender(<QueryStatus pending={false} error={new Error('Network unavailable')} empty={false} onRetry={retry} />);
    expect(screen.getByRole('alert')).toHaveTextContent('Network unavailable');
    await userEvent.click(screen.getByRole('button', { name: 'Thử lại' }));
    expect(retry).toHaveBeenCalledOnce();
  });

  it('flips a flashcard by keyboard and preserves its accessible state', async () => {
    function Card() {
      const [flipped, setFlipped] = useState(false);
      return <Flashcard front="Hello" back="Xin chào" isFlipped={flipped} onFlip={() => setFlipped(x => !x)} />;
    }
    render(<Card />);
    const button = screen.getByRole('button');
    expect(button).toHaveAttribute('aria-pressed', 'false');
    button.focus();
    await userEvent.keyboard(' ');
    expect(button).toHaveAttribute('aria-pressed', 'true');
    await userEvent.keyboard('{Enter}');
    expect(button).toHaveAttribute('aria-pressed', 'false');
  });
});

describe('Quiz submission', () => {
  beforeEach(() => {
    vi.mocked(learningApi.history).mockResolvedValue([]);
    vi.mocked(learningApi.quiz).mockResolvedValue([{ id: 'q1', type: 'MultipleChoice', prompt: 'Hello?', options: [{ id: 'a1', content: 'Xin chào' }, { id: 'a2', content: 'Tạm biệt' }], left: [], right: [] }]);
  });

  it('submits only answers, displays server results, and keeps the same attempt identifier on retry', async () => {
    vi.mocked(learningApi.submit).mockRejectedValueOnce(new Error('Temporary failure')).mockResolvedValueOnce({ id: 'attempt', lessonId: 'lesson', score: 1, maxScore: 1, completedAt: '2026-11-08T00:00:00Z', answers: [{ questionId: 'q1', isCorrect: true, explanation: 'Greeting', correctAnswer: 'Xin chào' }] });
    renderPage(<QuizSession lessonId="lesson" />);
    const submit = await screen.findByRole('button', { name: 'Nộp bài' });
    expect(submit).toBeDisabled();
    await userEvent.click(screen.getByRole('radio', { name: 'Xin chào' }));
    await userEvent.click(submit);
    expect(await screen.findByRole('alert')).toHaveTextContent('Temporary failure');
    await userEvent.click(submit);
    expect(await screen.findByRole('heading', { name: 'Điểm: 1/1' })).toBeInTheDocument();
    expect(screen.getByText(/Greeting/)).toBeInTheDocument();
    expect(vi.mocked(learningApi.submit).mock.calls[0][1]).toBe(vi.mocked(learningApi.submit).mock.calls[1][1]);
    expect(vi.mocked(learningApi.submit).mock.calls[0][2]).toEqual([{ questionId: 'q1', answer: 'a1' }]);
  });

  it('renders the empty quiz and never offers submission', async () => {
    vi.mocked(learningApi.quiz).mockResolvedValue([]);
    renderPage(<QuizSession lessonId="empty" />);
    await waitFor(() => expect(screen.getAllByText('Chưa có dữ liệu.').length).toBeGreaterThan(0));
    expect(screen.queryByRole('button', { name: 'Nộp bài' })).not.toBeInTheDocument();
  });

  it('exposes a load failure with a retry action', async () => {
    vi.mocked(learningApi.quiz).mockRejectedValue(new Error('Cannot load quiz'));
    renderPage(<QuizSession lessonId="broken" />);
    expect(await screen.findByRole('alert')).toHaveTextContent('Cannot load quiz');
    expect(screen.getByRole('button', { name: 'Thử lại' })).toBeInTheDocument();
  });
});

it('links resume learning to the actual unfinished lesson and shows course completion', async () => {
  vi.mocked(progressApi.progress).mockResolvedValue({ resume: { lessonId: 'unfinished', title: 'Greetings', courseId: 'course' }, courses: [{ courseId: 'course', title: 'English A1', completedLessons: 1, totalLessons: 2, percent: 50, lessons: [] }] });
  renderPage(<ProgressPage />);
  expect(await screen.findByRole('link', { name: /Tiếp tục học/ })).toHaveAttribute('href', '/lessons/unfinished');
  expect(screen.getByRole('progressbar', { name: 'Tiến độ English A1' })).toHaveValue(50);
});
