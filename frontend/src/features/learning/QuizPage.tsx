import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link, useParams } from 'react-router-dom';
import { learningApi } from './api/quizApi';
import { LearningLayout, QueryStatus } from './components/LearningLayout';
import type { QuizAnswer, QuizResult } from './types/quiz.types';

function playFeedback(correct: boolean) {
  try {
    const audio = new AudioContext();
    const oscillator = audio.createOscillator();
    const gain = audio.createGain();
    oscillator.frequency.value = correct ? 660 : 220;
    gain.gain.setValueAtTime(0.07, audio.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.001, audio.currentTime + 0.2);
    oscillator.connect(gain).connect(audio.destination);
    oscillator.start();
    oscillator.stop(audio.currentTime + 0.2);
    oscillator.onended = () => { void audio.close(); };
  } catch { /* Audio may be unavailable on restricted browsers. */ }
}

export function QuizPage() {
  const { lessonId = '' } = useParams();
  return <QuizSession key={lessonId} lessonId={lessonId} />;
}

export function QuizSession({ lessonId }: { lessonId: string }) {
  const cache = useQueryClient();
  const quiz = useQuery({ queryKey: ['quiz', lessonId], queryFn: () => learningApi.quiz(lessonId) });
  const history = useQuery({ queryKey: ['quiz-history', lessonId], queryFn: () => learningApi.history(lessonId) });
  const [answers, setAnswers] = useState<Record<string, QuizAnswer>>({});
  const [attemptId, setAttemptId] = useState(() => crypto.randomUUID());
  const [result, setResult] = useState<QuizResult | null>(null);
  const submit = useMutation({
    mutationFn: () => learningApi.submit(lessonId, attemptId, (quiz.data ?? []).map(q => ({ questionId: q.id, answer: answers[q.id] }))),
    onSuccess: data => {
      setResult(data);
      playFeedback(data.score === data.maxScore);
      void cache.invalidateQueries({ queryKey: ['quiz-history', lessonId] });
    },
  });
  const answered = (quiz.data ?? []).filter(q => q.type === 'Matching'
    ? q.left.every(item => typeof answers[q.id] === 'object' && Boolean((answers[q.id] as Record<string, string>)[item.id]))
    : typeof answers[q.id] === 'string' && (answers[q.id] as string).trim().length > 0).length;

  return <LearningLayout title="Luyện tập Quiz">
    <Link to={`/lessons/${lessonId}`} className="font-bold text-rose-700">← Quay lại bài học</Link>
    <QueryStatus pending={quiz.isPending} error={quiz.error} empty={quiz.data?.length === 0} onRetry={() => { void quiz.refetch(); }} />
    {quiz.data && quiz.data.length > 0 && <>
      <label className="block">Đã trả lời {answered}/{quiz.data.length}<progress aria-label="Tiến độ làm bài" className="block w-full" max={quiz.data.length} value={answered} /></label>
      <form onSubmit={event => { event.preventDefault(); submit.mutate(); }} className="space-y-5">
        {quiz.data.map((q, index) => <fieldset key={q.id} disabled={submit.isPending || result !== null} className="rounded-xl border-2 border-stone-900 bg-white p-5">
          <legend className="font-bold">{index + 1}. {q.prompt}</legend>
          {q.type === 'MultipleChoice' && q.options.map(option => <label key={option.id} className="my-2 flex gap-3">
            <input type="radio" name={q.id} value={option.id} checked={answers[q.id] === option.id} onChange={() => setAnswers(old => ({ ...old, [q.id]: option.id }))} />{option.content}
          </label>)}
          {q.type === 'FillInBlank' && <input aria-label={`Đáp án câu ${index + 1}`} className="w-full rounded border p-3" value={typeof answers[q.id] === 'string' ? answers[q.id] as string : ''} onChange={event => setAnswers(old => ({ ...old, [q.id]: event.target.value }))} />}
          {q.type === 'Matching' && q.left.map(item => <label key={item.id} className="my-3 flex flex-wrap items-center gap-4">{item.text}
            <select aria-label={`Ghép ${item.text}`} className="rounded border p-2" value={typeof answers[q.id] === 'object' ? (answers[q.id] as Record<string, string>)[item.id] ?? '' : ''} onChange={event => {
              const value = event.target.value;
              setAnswers(old => ({ ...old, [q.id]: { ...(typeof old[q.id] === 'object' ? old[q.id] as Record<string, string> : {}), [item.id]: value } }));
            }}><option value="">Chọn cặp</option>{q.right.map(right => <option key={right.id} value={right.id}>{right.text}</option>)}</select>
          </label>)}
        </fieldset>)}
        {!result && <button disabled={submit.isPending || answered !== quiz.data.length} className="rounded-lg bg-rose-700 px-6 py-3 font-bold text-white disabled:opacity-50">{submit.isPending ? 'Đang chấm...' : 'Nộp bài'}</button>}
      </form>
    </>}
    {submit.isError && <p role="alert">{submit.error.message} Bạn có thể nộp lại cùng lượt làm bài.</p>}
    {result && <section aria-label="Kết quả" className="rounded-xl border-2 bg-white p-5"><h2 className="text-2xl font-bold">Điểm: {result.score}/{result.maxScore}</h2>
      {result.answers.map(answer => <p key={answer.questionId} className="mt-3">{quiz.data?.find(q => q.id === answer.questionId)?.prompt}: <strong>{answer.isCorrect ? 'Đúng' : 'Sai'}</strong> — Đáp án: {answer.correctAnswer}. {answer.explanation}</p>)}
      <button className="mt-4 font-bold text-rose-700" onClick={() => { setAnswers({}); setResult(null); setAttemptId(crypto.randomUUID()); submit.reset(); }}>Làm lại</button>
    </section>}
    <section><h2 className="text-xl font-bold">Lịch sử làm bài</h2>
      <QueryStatus pending={history.isPending} error={history.error} empty={history.data?.length === 0} />
      {history.data?.map(item => <details key={item.id} className="my-3 rounded border bg-white p-3"><summary>{new Date(item.completedAt).toLocaleString()} — {item.score}/{item.maxScore}</summary>{item.answers.map(answer => <p key={answer.questionId}>{answer.isCorrect ? 'Đúng' : 'Sai'}: {answer.correctAnswer}. {answer.explanation}</p>)}</details>)}
    </section>
  </LearningLayout>;
}
