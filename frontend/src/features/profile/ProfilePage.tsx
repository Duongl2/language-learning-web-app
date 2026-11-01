import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { profileApi } from './api/profileApi';

const TIMEZONES = Intl.supportedValuesOf('timeZone');

function isValidTimeZone(value: string): boolean {
  try {
    new Intl.DateTimeFormat('en', { timeZone: value });
    return true;
  } catch {
    return false;
  }
}

export function ProfilePage() {
  const queryClient = useQueryClient();
  const { data: profile, isLoading, error } = useQuery({
    queryKey: ['profile'],
    queryFn: profileApi.get,
  });
  const [displayName, setDisplayName] = useState<string | null>(null);
  const [timezone, setTimezone] = useState<string | null>(null);
  const [validationError, setValidationError] = useState<string | null>(null);
  const mutation = useMutation({
    mutationFn: profileApi.update,
    onSuccess: (updated) => {
      queryClient.setQueryData(['profile'], updated);
      setDisplayName(null);
      setTimezone(null);
    },
  });

  const nameValue = displayName ?? profile?.displayName ?? '';
  const timezoneValue = timezone ?? profile?.timezone ?? '';

  function save(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const name = nameValue.trim();
    const zone = timezoneValue.trim();
    if (!name || name.length > 80) {
      setValidationError('Tên hiển thị phải có từ 1 đến 80 ký tự.');
      return;
    }
    if (!zone || !isValidTimeZone(zone)) {
      setValidationError('Vui lòng chọn múi giờ hợp lệ.');
      return;
    }
    setValidationError(null);
    mutation.mutate({ displayName: name, timezone: zone });
  }

  return (
    <main className="graph-paper min-h-screen px-4 py-10 text-stone-900">
      <div className="mx-auto max-w-xl rounded-2xl border-2 border-stone-900 bg-white p-6 shadow-[5px_5px_0_#262626]">
        <Link to="/" className="text-sm font-semibold text-rose-700">← Về trang học</Link>
        <h1 className="mt-4 text-2xl font-extrabold">Hồ sơ cá nhân</h1>
        {isLoading && <p className="mt-4">Đang tải hồ sơ...</p>}
        {error && <p role="alert" className="mt-4 text-red-700">{error.message}</p>}
        {profile && (
          <form onSubmit={save} className="mt-6 space-y-5">
            <div>
              <span className="block text-sm font-bold">Email</span>
              <p className="mt-1 text-sm text-stone-600">{profile.email}</p>
            </div>
            <div>
              <label htmlFor="profile-name" className="block text-sm font-bold">Tên hiển thị</label>
              <input id="profile-name" value={nameValue} onChange={(event) => setDisplayName(event.target.value)} maxLength={80} className="mt-1 w-full rounded-lg border-2 border-stone-300 p-2 focus:border-rose-500" />
            </div>
            <div>
              <label htmlFor="profile-timezone" className="block text-sm font-bold">Múi giờ</label>
              <select id="profile-timezone" value={timezoneValue} onChange={(event) => setTimezone(event.target.value)} className="mt-1 w-full rounded-lg border-2 border-stone-300 p-2 focus:border-rose-500">
                {!TIMEZONES.includes(timezoneValue) && <option value={timezoneValue}>{timezoneValue}</option>}
                {TIMEZONES.map((zone) => <option key={zone} value={zone}>{zone}</option>)}
              </select>
              <p className="mt-1 text-xs text-stone-500">Múi giờ dùng để tính ngày học và streak.</p>
            </div>
            {validationError && <p role="alert" className="text-sm text-red-700">{validationError}</p>}
            {mutation.isError && <p role="alert" className="text-sm text-red-700">{mutation.error.message}</p>}
            {mutation.isSuccess && <p role="status" className="text-sm text-green-700">Đã lưu hồ sơ.</p>}
            <button type="submit" disabled={mutation.isPending} className="rounded-lg border-2 border-stone-900 bg-rose-500 px-4 py-2 font-bold text-white disabled:opacity-60">
              {mutation.isPending ? 'Đang lưu...' : 'Lưu thay đổi'}
            </button>
          </form>
        )}
      </div>
    </main>
  );
}
