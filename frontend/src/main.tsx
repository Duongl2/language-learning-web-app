
import { StrictMode } from 'react';
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
import { ReviewSessionPage } from './features/reviews/ReviewSessionPage.tsx';
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
          <Routes>
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
        </AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  </StrictMode>
);
