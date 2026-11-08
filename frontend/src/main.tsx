
import { StrictMode, Suspense } from 'react';
import { createRoot } from 'react-dom/client';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import {
  BrowserRouter,
  Navigate,
  Route,
  Routes,
  useParams,
} from 'react-router-dom';

import App from './App.tsx';
import { AuthPage } from './features/auth/AuthPage.tsx';
import { AuthProvider } from './features/auth/AuthProvider.tsx';
import { ProtectedRoute } from './features/auth/ProtectedRoute.tsx';
import { ProfilePage } from './features/profile/ProfilePage.tsx';
import { CourseCatalogPage, CourseDetailPage, LessonPage } from './features/courses/courseRoutes.tsx';
import { ReviewSessionPage } from './features/reviews/ReviewSessionPage.tsx';
import { QuizPage } from './features/learning/QuizPage';
import { ProgressPage } from './features/learning/ProgressPage';
import { StatisticsPage } from './features/learning/StatisticsPage';
import { CommunityPage } from './features/learning/CommunityPage';
import { CsvPage } from './features/learning/CsvPage';
import './index.css';


const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 60 * 1000,
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function ReviewSessionRoute() {
  const { deckId } = useParams<{ deckId: string }>();

  if (!deckId) {
    return <Navigate to="/" replace />;
  }

  return <ReviewSessionPage deckId={deckId} />;
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthProvider>
          <Suspense fallback={<div className="graph-paper min-h-screen p-8 text-stone-600">Đang tải trang...</div>}>
          <Routes>
            <Route path="/lessons/:lessonId/quiz" element={<ProtectedRoute><QuizPage /></ProtectedRoute>} />
            <Route path="/learning" element={<ProtectedRoute><ProgressPage /></ProtectedRoute>} />
            <Route path="/statistics" element={<ProtectedRoute><StatisticsPage /></ProtectedRoute>} />
            <Route path="/community" element={<ProtectedRoute><CommunityPage /></ProtectedRoute>} />
            <Route path="/decks/:deckId/csv" element={<ProtectedRoute><CsvPage /></ProtectedRoute>} />
            <Route
              path="/login"
              element={<AuthPage mode="login" />}
            />

            <Route
              path="/register"
              element={<AuthPage mode="register" />}
            />

            <Route
              path="/"
              element={
                <ProtectedRoute>
                  <App />
                </ProtectedRoute>
              }
            />

            <Route
              path="/profile"
              element={
                <ProtectedRoute>
                  <ProfilePage />
                </ProtectedRoute>
              }
            />

            <Route
              path="/courses"
              element={<ProtectedRoute><CourseCatalogPage /></ProtectedRoute>}
            />
            <Route
              path="/courses/:courseId"
              element={<ProtectedRoute><CourseDetailPage /></ProtectedRoute>}
            />
            <Route
              path="/lessons/:lessonId"
              element={<ProtectedRoute><LessonPage /></ProtectedRoute>}
            />

            <Route
              path="/reviews/:deckId"
              element={
                <ProtectedRoute>
                  <ReviewSessionRoute />
                </ProtectedRoute>
              }
            />

            <Route
              path="*"
              element={<Navigate to="/" replace />}
            />
          </Routes>
          </Suspense>
        </AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  </StrictMode>
);
