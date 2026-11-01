
import { useState } from 'react';
import { Flashcard } from '../flashcards/components/Flashcard';
import { useDueCards } from './hooks/useDueCards';

type ReviewRating = 'again' | 'hard' | 'good' | 'easy';

function formatReviewInterval(minutes: number): string {
  if (!Number.isFinite(minutes) || minutes <= 0) {
    return 'Chưa có dự kiến';
  }

  if (minutes < 60) {
    return `${Math.round(minutes)} phút`;
  }

  if (minutes < 1440) {
    return `${Number((minutes / 60).toFixed(1))} giờ`;
  }

  return `${Number((minutes / 1440).toFixed(1))} ngày`;
}

interface ReviewSessionPageProps {
  deckId: string;
}

export function ReviewSessionPage({
  deckId,
}: ReviewSessionPageProps) {
  const { data, isLoading, isError } = useDueCards(deckId);

  const [currentIndex, setCurrentIndex] = useState(0);
  const [isFlipped, setIsFlipped] = useState(false);
  const [ratings, setRatings] = useState<ReviewRating[]>([]);
  const [isTransitioning, setIsTransitioning] = useState(false);

  const handleRating = (rating: ReviewRating) => {
    if (!isFlipped || isTransitioning) return;

    setIsTransitioning(true);

    window.setTimeout(() => {
        setRatings((previous) => [...previous, rating]);
        setCurrentIndex((previous) => previous + 1);
        setIsFlipped(false);
        setIsTransitioning(false);
    }, 200);
  };

  if (isLoading) {
    return (
      <main className="mx-auto max-w-2xl px-4 py-12 text-center">
        <p className="text-stone-500">
          Đang tải danh sách thẻ ôn tập...
        </p>
      </main>
    );
  }

  if (isError) {
    return (
      <main className="mx-auto max-w-2xl px-4 py-12 text-center">
        <p className="text-red-500">
          Không thể tải danh sách thẻ ôn tập.
        </p>
      </main>
    );
  }

  const cards = data ?? [];
  const totalCards = cards.length;

  if (totalCards === 0) {
    return (
      <main className="mx-auto max-w-2xl px-4 py-12 text-center">
        <h1 className="mb-3 text-2xl font-bold text-stone-900">
          Không có thẻ cần ôn tập
        </h1>
        <p className="text-stone-500">
          Hiện tại bạn đã hoàn thành tất cả thẻ đến hạn.
        </p>
      </main>
    );
  }

  const currentCard = cards[currentIndex];

  if (!currentCard) {
    return (
      <main className="mx-auto max-w-2xl px-4 py-12 text-center">
        <div className="rounded-2xl border border-green-200 bg-green-50 p-8">
          <h1 className="mb-3 text-2xl font-bold text-green-700">
            Hoàn thành phiên ôn tập!
          </h1>

          <p className="text-stone-600">
            Bạn đã đánh giá {ratings.length} / {totalCards} thẻ.
          </p>

          <p className="mt-3 text-sm text-stone-500">
            Các đánh giá hiện chỉ được lưu tạm trong phiên này.
          </p>
        </div>
      </main>
    );
  }

  const progress = (currentIndex / totalCards) * 100;
  const intervals = currentCard.intervals;
  return (
    <main className="mx-auto max-w-2xl px-4 py-8">
      <div className="mb-8 text-center">
        <h1 className="mb-2 text-2xl font-bold text-stone-900">
          Ôn tập Flashcard
        </h1>

        <p className="text-sm text-stone-500">
          Ghi nhớ kiến thức mỗi ngày
        </p>
      </div>

      <div className="mb-6">
        <div className="mb-2 flex items-center justify-between text-sm">
          <span className="font-medium text-stone-600">
            Tiến độ ôn tập
          </span>

          <span className="font-semibold text-stone-800">
            {currentIndex + 1} / {totalCards}
          </span>
        </div>

        <div
          className="h-3 overflow-hidden rounded-full bg-stone-200"
          role="progressbar"
          aria-label="Tiến độ ôn tập"
          aria-valuenow={currentIndex}
          aria-valuemin={0}
          aria-valuemax={totalCards}
        >
          <div
            className="h-full rounded-full bg-green-500 transition-all duration-300 ease-out"
            style={{ width: `${progress}%` }}
          />
        </div>

        <p className="mt-2 text-right text-xs text-stone-500">
          Đã hoàn thành {currentIndex} / {totalCards} thẻ
        </p>
      </div>

      <div
        className={`transition-opacity duration-200 ease-in-out ${
            isTransitioning ? 'opacity-0' : 'opacity-100'
        }`}
        >
        <Flashcard
            key={currentCard.cardId}
            front={currentCard.front}
            back={currentCard.back}
            isFlipped={isFlipped}
            onFlip={() => {
            if (!isTransitioning) {
                setIsFlipped((previous) => !previous);
            }
            }}
        />
      </div>

      <div className="mt-6 min-h-28">
        {isFlipped ? (
          <div>
            <p className="mb-4 text-center text-sm font-medium text-stone-600">
              Bạn ghi nhớ thẻ này ở mức độ nào?
            </p>
          
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
        <button
            type="button"
            onClick={() => handleRating('again')}
            disabled={isTransitioning}
            className="rounded-xl bg-red-500 px-4 py-3 text-white transition-colors hover:bg-red-600 disabled:opacity-60"
        >
            <span className="block font-semibold">Again</span>
            <span className="mt-1 block text-xs">
            {intervals
                ? formatReviewInterval(intervals.againMinutes)
                : 'Chưa có dự kiến'}
            </span>
        </button>

        <button
            type="button"
            onClick={() => handleRating('hard')}
            disabled={isTransitioning}
            className="rounded-xl bg-orange-500 px-4 py-3 text-white transition-colors hover:bg-orange-600 disabled:opacity-60"
        >
            <span className="block font-semibold">Hard</span>
            <span className="mt-1 block text-xs">
            {intervals
                ? formatReviewInterval(intervals.hardMinutes)
                : 'Chưa có dự kiến'}
            </span>
        </button>

        <button
            type="button"
            onClick={() => handleRating('good')}
            disabled={isTransitioning}
            className="rounded-xl bg-green-600 px-4 py-3 text-white transition-colors hover:bg-green-700 disabled:opacity-60"
        >
            <span className="block font-semibold">Good</span>
            <span className="mt-1 block text-xs">
            {intervals
                ? formatReviewInterval(intervals.goodMinutes)
                : 'Chưa có dự kiến'}
            </span>
        </button>

        <button
            type="button"
            onClick={() => handleRating('easy')}
            disabled={isTransitioning}
            className="rounded-xl bg-blue-500 px-4 py-3 text-white transition-colors hover:bg-blue-600 disabled:opacity-60"
        >
            <span className="block font-semibold">Easy</span>
            <span className="mt-1 block text-xs">
            {intervals
                ? formatReviewInterval(intervals.easyMinutes)
                : 'Chưa có dự kiến'}
            </span>
        </button>
      </div>
          </div>
        ) : (
          <p className="text-center text-sm text-stone-500">
            Nhấn vào thẻ để xem đáp án.
          </p>
        )}
      </div>
    </main>
  );
}
