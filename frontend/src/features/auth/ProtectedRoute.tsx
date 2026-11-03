
import { useEffect, useState } from 'react';
import { Navigate, useLocation } from 'react-router-dom';

import { useAuth } from './authContext';
import { OfflineStatusBar } from '../../offline/OfflineStatusBar';
import { db } from '../../offline/db';

import type { ReactNode } from 'react';

interface ProtectedRouteProps {
  children: ReactNode;
}

interface VerifiedOfflineDeck {
  deckId: string;
  userId: string;
  allowed: boolean;
}

export function ProtectedRoute({
  children,
}: ProtectedRouteProps) {
  const { user, offlineUser, isRestoring } = useAuth();
  const location = useLocation();

  const [verifiedDeck, setVerifiedDeck] =
    useState<VerifiedOfflineDeck | null>(null);

  const match = location.pathname.match(
    /^\/reviews\/([a-zA-Z0-9-]+)\/?$/
  );

  const deckId = match?.[1] ?? null;

  const canUseOffline =
    !user &&
    !isRestoring &&
    !navigator.onLine &&
    offlineUser !== null &&
    deckId !== null;

  useEffect(() => {
    if (!canUseOffline || !deckId || !offlineUser) {
      return;
    }

    let active = true;

    async function verifyOfflineDeck() {
      try {
        const deck = await db.decks.get(deckId!);

        const cardsCount = await db.cards
          .where('deckId')
          .equals(deckId!)
          .count();

        const schedulesCount = await db.cardSchedules
          .where('deckId')
          .equals(deckId!)
          .count();

        if (active) {
          setVerifiedDeck({
            deckId: deckId!,
            userId: offlineUser!.id,
            allowed:
              deck?.ownerId === offlineUser!.id &&
              cardsCount > 0 &&
              schedulesCount > 0,
          });
        }
      } catch {
        if (active) {
          setVerifiedDeck({
            deckId: deckId!,
            userId: offlineUser!.id,
            allowed: false,
          });
        }
      }
    }

    void verifyOfflineDeck();

    return () => {
      active = false;
    };
  }, [canUseOffline, deckId, offlineUser]);

  if (isRestoring) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-[#FAF6EE] text-stone-600">
        Đang khôi phục phiên đăng nhập...
      </div>
    );
  }

  if (user) {
    return (
      <>
        <OfflineStatusBar />
        {children}
      </>
    );
  }

  if (canUseOffline && offlineUser && deckId) {
    const verified =
      verifiedDeck?.deckId === deckId &&
      verifiedDeck?.userId === offlineUser.id;

    if (!verified) {
      return (
        <main className="flex min-h-screen items-center justify-center bg-[#FAF6EE] text-stone-600">
          Đang kiểm tra dữ liệu học offline...
        </main>
      );
    }

    if (verifiedDeck.allowed) {
      return (
        <>
          <OfflineStatusBar />
          {children}
        </>
      );
    }

    return (
      <main className="flex min-h-screen items-center justify-center bg-[#FAF6EE] px-4">
        <div className="rounded-xl border border-orange-200 bg-white p-6 text-center">
          <h1 className="text-lg font-bold text-stone-900">
            Không thể mở bộ thẻ offline
          </h1>

          <p className="mt-2 text-sm text-stone-600">
            Bộ thẻ chưa được tải đầy đủ hoặc không thuộc
            tài khoản đang sử dụng.
          </p>
        </div>
      </main>
    );
  }

  return (
    <Navigate
      to="/login"
      replace
      state={{
        from: `${location.pathname}${location.search}${location.hash}`,
      }}
    />
  );
}
