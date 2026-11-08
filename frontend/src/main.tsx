
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
import './index.css';
import { learningRoutes } from './features/learning/LearningNavigation';


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
            {Object.values(learningRoutes).map(route => <Route key={route.path} path={route.path} element={<ProtectedRoute><route.default /></ProtectedRoute>} />)}
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
