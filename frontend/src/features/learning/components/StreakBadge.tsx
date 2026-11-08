import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { Flame } from 'lucide-react';
import { learningApi } from '../api/learningApi';

export function StreakBadge() {
  const streak = useQuery({ queryKey: ['streak'], queryFn: learningApi.streak });
  if (!streak.data) return null;
  return <Link to="/statistics" aria-label={`Chuỗi học ${streak.data.currentStreak} ngày`} className="inline-flex items-center gap-1 text-xs font-bold text-orange-700"><Flame size={16} aria-hidden="true" />{streak.data.currentStreak}</Link>;
}
