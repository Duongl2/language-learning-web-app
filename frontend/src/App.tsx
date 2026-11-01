import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { LogOut } from 'lucide-react';
import { useAuth } from './features/auth/authContext';
import { MyDecksContainer } from './features/decks/MyDecksContainer';
import { Flashcard } from './features/flashcards/components/Flashcard';
import {
  HomeOutlined,
  CodeOutlined,
  AudioOutlined,
  MessageOutlined,
  TrophyOutlined,
  TeamOutlined,
  RiseOutlined,
  SettingOutlined,
  SoundOutlined,
  SwapOutlined,
  ThunderboltOutlined,
  LeftOutlined,
  RightOutlined,
  ReloadOutlined,
  CustomerServiceOutlined,
  BookOutlined,
  FlagOutlined,
  EyeOutlined,
  HeartOutlined
} from '@ant-design/icons';

export function App() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [currentView, setCurrentView] = useState<
  'flashcard' | 'lessonList' | 'quiz' | 'myDecks'
>('flashcard');
  const [activeTabSub, setActiveTabSub] = useState<'vocab' | 'example'>('vocab');
  const [isFlipped, setIsFlipped] = useState(false);
  const [selectedRating, setSelectedRating] = useState<number | null>(null);
  const [selectedQuizOption, setSelectedQuizOption] = useState<number | null>(null);

useEffect(() => {
  if (currentView !== 'flashcard') {
    return;
  }

  function handleKeyDown(event: KeyboardEvent) {
    if (event.repeat || event.ctrlKey || event.altKey || event.metaKey) {
      return;
    }

    const target = event.target;

    if (
      target instanceof HTMLElement &&
      (target.closest('button, input, textarea, select') ||
        target.isContentEditable)
    ) {
      return;
    }

    if (event.code === 'Space') {
      event.preventDefault();
      setIsFlipped((previous) => !previous);
      return;
    }

    if (['1', '2', '3', '4'].includes(event.key)) {
      event.preventDefault();
      setSelectedRating(Number(event.key));
    }
  }

  window.addEventListener('keydown', handleKeyDown);

  return () => {
    window.removeEventListener('keydown', handleKeyDown);
  };
}, [currentView]);

  return (
    <div className="min-h-screen flex bg-[#FAF6EE] text-[#1F1F1F] font-sans antialiased">
      <aside className="w-56 bg-[#FAF6EE] border-r-2 border-[#262626] flex flex-col justify-between shrink-0 select-none">
        <div>
          <div className="p-4 border-b-2 border-[#262626] flex items-center justify-between">
            <div className="flex items-center gap-2">
              <span className="bg-[#E11D48] text-white text-[11px] font-mono font-bold px-1.5 py-0.5 rounded border border-[#9F1239]">
                &lt;iT/&gt;
              </span>
              <div className="leading-tight">
                <span className="font-bold text-sm block tracking-tight text-stone-900">
                  日本語
                </span>
                <span className="text-[10px] uppercase font-bold text-stone-500 block -mt-1 tracking-wider">
                  KOTOBA
                </span>
              </div>
            </div>
            <div className="flex items-center gap-1 text-xs font-bold text-stone-700">
              <ThunderboltOutlined className="text-amber-500 text-sm" />
              <span>0</span>
            </div>
          </div>

          <div className="p-3 space-y-1">
            <span className="text-[10px] uppercase font-bold tracking-wider text-stone-400 px-3 py-1 block">
              HỌC TẬP
            </span>

            <button
              onClick={() => setCurrentView('lessonList')}
              className={`w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-xs font-semibold transition text-left ${
                currentView === 'lessonList'
                  ? 'bg-white border-notebook shadow-2xs text-stone-900'
                  : 'text-stone-600 hover:bg-stone-200/50'
              }`}
            >
              <HomeOutlined />
              <span>Trang chủ</span>
            </button>

            <button
              onClick={() => setCurrentView('flashcard')}
              className={`w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-xs font-semibold transition text-left ${
                currentView === 'flashcard'
                  ? 'bg-white border-notebook shadow-2xs text-stone-900'
                  : 'text-stone-600 hover:bg-stone-200/50'
              }`}
            >
              <CodeOutlined />
              <span>Từ vựng IT</span>
            </button>

            <button
              onClick={() => setCurrentView('quiz')}
              className={`w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-xs font-semibold transition text-left ${
                currentView === 'quiz'
                  ? 'bg-white border-notebook shadow-2xs text-stone-900'
                  : 'text-stone-600 hover:bg-stone-200/50'
              }`}
            >
              <ThunderboltOutlined />
              <span>Luyện tập Quiz</span>
            </button>

            <button
              type="button"
              onClick={() => setCurrentView('myDecks')}
              className={`w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-xs font-semibold transition text-left ${
                currentView === 'myDecks'
                  ? 'bg-white border-notebook shadow-2xs text-stone-900'
                  : 'text-stone-600 hover:bg-stone-200/50'
              }`}
            >
              <BookOutlined />
              <span>Bộ thẻ của tôi</span>
            </button>

            <button className="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-xs font-semibold text-stone-600 hover:bg-stone-200/50 transition text-left">
              <AudioOutlined />
              <span>Shadowing</span>
            </button>

            <button className="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-xs font-semibold text-stone-600 hover:bg-stone-200/50 transition text-left">
              <MessageOutlined />
              <span>Hội thoại</span>
            </button>

            <button className="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-xs font-semibold text-stone-600 hover:bg-stone-200/50 transition text-left">
              <TrophyOutlined />
              <span>Luyện thi chứng chỉ</span>
            </button>

            <button className="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-xs font-semibold text-stone-600 hover:bg-stone-200/50 transition text-left">
              <TeamOutlined />
              <span>Luyện phỏng vấn</span>
            </button>

            <button className="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-xs font-semibold text-stone-600 hover:bg-stone-200/50 transition text-left">
              <RiseOutlined />
              <span>Bảng xếp hạng</span>
            </button>
          </div>
        </div>

        <div className="p-3 border-t-2 border-[#262626] space-y-2">
          <button type="button" onClick={() => navigate('/profile')} className="w-full flex items-center gap-2 px-3 py-1.5 text-xs text-stone-600 hover:text-stone-900 transition">
            <SettingOutlined />
            <span>Cài đặt</span>
          </button>
          <div className="flex items-center gap-2 p-2 rounded-lg bg-white border-notebook">
            <div className="w-7 h-7 rounded-full bg-sky-600 text-white flex items-center justify-center font-bold text-xs">
              {user?.email.charAt(0).toUpperCase() ?? '?'}
            </div>
            <div className="min-w-0 text-[11px] leading-tight">
              <span className="block truncate font-bold text-stone-900" title={user?.email}>
                {user?.email}
              </span>
              <span className="text-stone-400 text-[10px]">{user?.role}</span>
            </div>
          </div>
          <button
            type="button"
            onClick={() => {
              void logout().then(({ revoked, offlineCleared }) => {
                navigate('/login', { replace: true });
                if (!offlineCleared) {
                  window.alert('Không thể xóa dữ liệu học ngoại tuyến. Hãy xóa dữ liệu trang web trong trình duyệt trước khi dùng chung thiết bị.');
                } else if (!revoked) {
                  window.alert('Đã xóa phiên trên thiết bị, nhưng máy chủ chưa xác nhận đăng xuất.');
                }
              });
            }}
            className="flex w-full items-center gap-2 rounded-lg px-3 py-1.5 text-left text-xs font-semibold text-stone-600 transition hover:bg-stone-200/50"
          >
            <LogOut size={15} aria-hidden="true" />
            <span>Đăng xuất</span>
          </button>
        </div>
      </aside>

      <main className="flex-1 graph-paper overflow-y-auto p-6 md:p-8 flex flex-col items-center">
        <div className="w-full max-w-4xl space-y-6">

          {currentView === 'myDecks' && (
            <MyDecksContainer />
          )}

          {currentView === 'flashcard' && (
            <div className="space-y-6">
              <div className="flex flex-wrap items-center justify-between gap-3 text-xs">
                <div className="flex items-center bg-white rounded-lg border-notebook overflow-hidden">
                  <button
                    onClick={() => setActiveTabSub('vocab')}
                    className={`px-3 py-1.5 flex items-center gap-1.5 font-bold transition ${
                      activeTabSub === 'vocab' ? 'bg-[#262626] text-white' : 'text-stone-700 hover:bg-stone-100'
                    }`}
                  >
                    <BookOutlined /> Từ vựng
                  </button>
                  <button
                    onClick={() => setActiveTabSub('example')}
                    className={`px-3 py-1.5 flex items-center gap-1.5 font-bold transition ${
                      activeTabSub === 'example' ? 'bg-[#262626] text-white' : 'text-stone-700 hover:bg-stone-100'
                    }`}
                  >
                    <BookOutlined /> Ví dụ
                  </button>
                </div>

                <div className="px-4 py-1 rounded-lg bg-white border-notebook font-bold text-sm">
                  1 / 109
                </div>

                <div className="flex items-center gap-1.5">
                  <button className="px-2.5 py-1.5 rounded-lg bg-white notebook-btn text-xs font-bold flex items-center gap-1">
                    <SwapOutlined /> JA → VI
                  </button>
                  <button className="px-2.5 py-1.5 rounded-lg bg-white notebook-btn text-xs font-bold flex items-center gap-1">
                    <RightOutlined className="text-[10px]" /> Tự động
                  </button>
                  <button className="px-2.5 py-1.5 rounded-lg bg-white notebook-btn text-xs font-bold flex items-center gap-1">
                    <ReloadOutlined /> Xáo trộn
                  </button>
                  <button className="p-1.5 rounded-lg bg-white notebook-btn text-xs">
                    <SettingOutlined />
                  </button>
                </div>
              </div>

              <Flashcard
                front="言い回し"
                back="Cách nói, cách diễn đạt"
                isFlipped={isFlipped}
                onFlip={() => setIsFlipped((previous) => !previous)}
              />

              <div className="flex items-center justify-center gap-3">
                <button className="px-5 py-2.5 rounded-lg bg-white notebook-btn text-xs font-bold text-stone-800 flex items-center gap-1.5">
                  <LeftOutlined className="text-[10px]" /> Trước
                </button>

                {[
                  { value: 1, label: 'Quên', color: 'bg-red-600' },
                  { value: 2, label: 'Khó', color: 'bg-orange-500' },
                  { value: 3, label: 'Tốt', color: 'bg-green-600' },
                  { value: 4, label: 'Dễ', color: 'bg-blue-600' },
                ].map((rating) => (
                  <button
                    key={rating.value}
                    type="button"
                    onClick={() => setSelectedRating(rating.value)}
                    aria-pressed={selectedRating === rating.value}
                    className={`rounded-lg border-2 border-stone-900 px-4 py-2.5 text-xs font-bold text-white transition ${
                      rating.color
                    } ${
                      selectedRating === rating.value
                        ? 'ring-2 ring-stone-900 ring-offset-2'
                        : 'hover:opacity-85'
                    }`}
                  >
                    {rating.value} · {rating.label}
                  </button>
                ))}

                <button className="px-5 py-2.5 rounded-lg bg-white notebook-btn text-xs font-bold text-stone-800 flex items-center gap-1.5">
                  Sau <RightOutlined className="text-[10px]" />
                </button>
              </div>

              <div className="space-y-3 pt-4">
                <h3 className="font-bold text-sm text-stone-900">Chọn chế độ học</h3>
                <div className="grid grid-cols-2 sm:grid-cols-5 gap-3">
                  <button
                    onClick={() => setCurrentView('flashcard')}
                    className="p-3 rounded-lg bg-[#262626] text-white border-notebook text-left space-y-2 transition"
                  >
                    <div className="flex items-center gap-1.5 text-xs font-bold">
                      <BookOutlined /> Flashcard
                    </div>
                    <span className="text-[10px] px-2 py-0.5 rounded bg-white/20 font-medium inline-block">
                      Chưa học
                    </span>
                  </button>

                  <button
                    onClick={() => setCurrentView('quiz')}
                    className="p-3 rounded-lg bg-white hover:bg-stone-50 border-notebook text-left space-y-2 transition"
                  >
                    <div className="flex items-center gap-1.5 text-xs font-bold text-stone-800">
                      <CodeOutlined /> Trắc nghiệm
                    </div>
                    <span className="text-[10px] px-2 py-0.5 rounded bg-stone-100 border border-stone-200 text-stone-600 font-medium inline-block">
                      Chưa học
                    </span>
                  </button>

                  <button className="p-3 rounded-lg bg-white hover:bg-stone-50 border-notebook text-left space-y-2 transition">
                    <div className="flex items-center gap-1.5 text-xs font-bold text-stone-800">
                      <ThunderboltOutlined /> Gõ từ
                    </div>
                    <span className="text-[10px] px-2 py-0.5 rounded bg-stone-100 border border-stone-200 text-stone-600 font-medium inline-block">
                      Chưa học
                    </span>
                  </button>

                  <button className="p-3 rounded-lg bg-white hover:bg-stone-50 border-notebook text-left space-y-2 transition">
                    <div className="flex items-center gap-1.5 text-xs font-bold text-stone-800">
                      <BookOutlined /> Đọc hiểu
                    </div>
                    <span className="text-[10px] px-2 py-0.5 rounded bg-stone-100 border border-stone-200 text-stone-600 font-medium inline-block">
                      Chưa học
                    </span>
                  </button>

                  <button className="p-3 rounded-lg bg-white hover:bg-stone-50 border-notebook text-left space-y-2 transition">
                    <div className="flex items-center gap-1.5 text-xs font-bold text-stone-800">
                      <CustomerServiceOutlined /> Nghe ghép câu
                    </div>
                    <span className="text-[10px] px-2 py-0.5 rounded bg-stone-100 border border-stone-200 text-stone-600 font-medium inline-block">
                      Chưa học
                    </span>
                  </button>
                </div>
              </div>

              <div className="grid sm:grid-cols-2 gap-4 pt-2">
                <div className="bg-white rounded-xl border-notebook p-4 space-y-3 shadow-2xs">
                  <div className="flex justify-between items-start">
                    <div className="space-y-0.5">
                      <span className="text-xs font-bold text-stone-400">1.</span>
                      <h4 className="text-xl font-bold text-stone-900">言い回し</h4>
                    </div>
                    <div className="text-right">
                      <span className="text-xs font-medium text-stone-600 block">いいまわし</span>
                      <span className="text-[10px] font-bold text-stone-400 uppercase">NGÔN HỒI</span>
                      <span className="text-xs font-bold text-stone-800 block">Cách nói, cách diễn đạt</span>
                    </div>
                    <div className="flex gap-1">
                      <button className="w-7 h-7 rounded border-notebook flex items-center justify-center hover:bg-stone-100 text-xs">
                        <FlagOutlined />
                      </button>
                      <button className="w-7 h-7 rounded border-notebook flex items-center justify-center hover:bg-stone-100 text-xs">
                        <SoundOutlined />
                      </button>
                    </div>
                  </div>

                  <div className="border-t border-stone-200 pt-3 text-xs text-stone-700 flex justify-between items-center bg-[#FAF6EE] p-2.5 rounded-lg border border-[#E5DEC9]">
                    <div className="space-y-0.5">
                      <p className="font-semibold text-stone-900">仕様書の言い回しが曖昧なので、修正してください。</p>
                      <p className="text-stone-500 text-[11px]">→ Cách diễn đạt trong tài liệu đặc tả còn mơ hồ, hãy chỉnh sửa lại.</p>
                    </div>
                    <button className="w-6 h-6 rounded border-notebook flex items-center justify-center text-stone-700 hover:bg-white text-[11px] shrink-0 ml-2">
                      <SoundOutlined />
                    </button>
                  </div>
                </div>

                <div className="bg-white rounded-xl border-notebook p-4 space-y-3 shadow-2xs">
                  <div className="flex justify-between items-start">
                    <div className="space-y-0.5">
                      <span className="text-xs font-bold text-stone-400">2.</span>
                      <h4 className="text-xl font-bold text-stone-900">一部</h4>
                    </div>
                    <div className="text-right">
                      <span className="text-xs font-medium text-stone-600 block">いちぶ</span>
                      <span className="text-[10px] font-bold text-stone-400 uppercase">NHẤT BỘ</span>
                      <span className="text-xs font-bold text-stone-800 block">Một phần</span>
                    </div>
                    <div className="flex gap-1">
                      <button className="w-7 h-7 rounded border-notebook flex items-center justify-center hover:bg-stone-100 text-xs">
                        <FlagOutlined />
                      </button>
                      <button className="w-7 h-7 rounded border-notebook flex items-center justify-center hover:bg-stone-100 text-xs">
                        <SoundOutlined />
                      </button>
                    </div>
                  </div>

                  <div className="border-t border-stone-200 pt-3 text-xs text-stone-700 flex justify-between items-center bg-[#FAF6EE] p-2.5 rounded-lg border border-[#E5DEC9]">
                    <div className="space-y-0.5">
                      <p className="font-semibold text-stone-900">一部の機能はまだ実装されていません。</p>
                      <p className="text-stone-500 text-[11px]">→ Một phần chức năng vẫn chưa được implement (cài đặt).</p>
                    </div>
                    <button className="w-6 h-6 rounded border-notebook flex items-center justify-center text-stone-700 hover:bg-white text-[11px] shrink-0 ml-2">
                      <SoundOutlined />
                    </button>
                  </div>
                </div>
              </div>
            </div>
          )}

          {currentView === 'lessonList' && (
            <div className="space-y-6">
              <div className="space-y-1">
                <button
                  onClick={() => setCurrentView('flashcard')}
                  className="text-xs text-stone-500 hover:text-stone-800 flex items-center gap-1 font-bold"
                >
                  <LeftOutlined className="text-[10px]" /> Trang chủ
                </button>
                <span className="text-xs text-stone-400 font-medium">16 bài — 1644 từ vựng</span>
                <div className="pt-1">
                  <h2 className="text-3xl font-extrabold tracking-tight text-stone-900 inline">
                    <span className="marker-highlight px-1">Từ vựng IT</span>
                  </h2>
                </div>
                <p className="text-xs text-stone-600 font-medium pt-1">
                  語彙 — Thuật ngữ tiếng Nhật thường gặp trong dự án IT
                </p>
              </div>

              <div className="bg-white rounded-xl border-notebook-2 p-4 space-y-2">
                <div className="flex justify-between items-center text-xs font-bold text-stone-700">
                  <span>Tiến độ học</span>
                  <span>0/16 bài</span>
                </div>
                <div className="w-full h-3 rounded-full border-notebook bg-[#FAF6EE] overflow-hidden p-0.5">
                  <div className="w-0 h-full rounded-full bg-[#16A34A] transition-all duration-300"></div>
                </div>
                <div className="text-right text-[11px] text-stone-400 font-medium">
                  0% hoàn thành
                </div>
              </div>

              <div className="grid sm:grid-cols-2 gap-4">
                {[
                  { num: 1, title: '頻出用語一覧', sub: 'Bài 1', count: '109 từ vựng' },
                  { num: 2, title: 'レッスン1 プロジェクト概要', sub: 'Bài 2', count: '173 từ vựng' },
                  { num: 3, title: 'レッスン2 見積り', sub: 'Bài 3', count: '125 từ vựng' },
                  { num: 4, title: 'レッスン3 開発環境', sub: 'Bài 4', count: '108 từ vựng' },
                  { num: 5, title: 'レッスン4 議事録', sub: 'Bài 5', count: '133 từ vựng' },
                  { num: 6, title: 'レッスン5 要件定義', sub: 'Bài 6', count: '104 từ vựng' },
                  { num: 7, title: 'レッスン6 基本設計', sub: 'Bài 7', count: '61 từ vựng' },
                  { num: 8, title: 'レッスン7 要求変更', sub: 'Bài 8', count: '91 từ vựng' },
                ].map((item) => (
                  <div
                    key={item.num}
                    onClick={() => setCurrentView('flashcard')}
                    className="relative bg-white rounded-xl border-notebook-2 p-5 pt-6 cursor-pointer hover:-translate-y-0.5 transition shadow-2xs hover:shadow-sm"
                  >
                    <div className="absolute top-0 left-4 -translate-y-1/2 w-8 h-8 rounded-md bg-[#FDE047] border-notebook flex items-center justify-center font-extrabold text-sm text-stone-900 shadow-2xs">
                      {item.num}
                    </div>

                    <div className="space-y-1">
                      <h4 className="font-extrabold text-stone-900 text-sm tracking-tight pt-1">
                        {item.title}
                      </h4>
                      <p className="text-xs text-stone-500 font-medium">{item.sub}</p>
                      <p className="text-[11px] text-stone-400 flex items-center gap-1 pt-1">
                        <BookOutlined className="text-[10px]" /> {item.count}
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {currentView === 'quiz' && (
            <div className="space-y-6">
              <div className="text-xs font-bold text-stone-500">
                Lesson 1 — IT Vocabulary
              </div>

              <div className="flashcard-yellow-grid rounded-xl border-notebook-2 p-6 sm:p-8 space-y-6">
                <div className="flex justify-between items-center text-xs">
                  <button className="px-3 py-1.5 rounded-lg bg-white border-notebook font-bold flex items-center gap-1.5 hover:bg-stone-50">
                    <SoundOutlined /> Bí quá thì nghe
                  </button>

                  <button className="px-3 py-1.5 rounded-lg bg-[#FEF08A] border-notebook font-bold flex items-center gap-1.5 text-stone-900">
                    <EyeOutlined /> Nghĩa
                  </button>
                </div>

                <div className="text-center space-y-2 py-4">
                  <div className="text-xs text-stone-500">たんとうしゃ</div>
                  <h3 className="text-2xl sm:text-3xl font-extrabold text-stone-900">
                    ___さんはインフラの担当者です。
                  </h3>
                  <p className="text-stone-500 italic text-xs sm:text-sm pt-1">
                    Mr. Yamato is in charge of the infrastructure.
                  </p>
                </div>

                <div className="grid grid-cols-2 gap-3">
                  {[
                    { id: 1, text: '判断' },
                    { id: 2, text: '原因' },
                    { id: 3, text: '認識' },
                    { id: 4, text: '大和' },
                  ].map((option) => (
                    <button
                      key={option.id}
                      onClick={() => setSelectedQuizOption(option.id)}
                      className={`p-3.5 rounded-lg border-notebook text-left font-extrabold text-sm flex items-center gap-2.5 transition ${
                        selectedQuizOption === option.id
                          ? 'bg-[#16A34A] text-white border-[#15803D]'
                          : 'bg-white hover:bg-stone-50 text-stone-900'
                      }`}
                    >
                      <span className={`w-5 h-5 rounded border flex items-center justify-center text-xs ${
                        selectedQuizOption === option.id ? 'border-white text-white' : 'border-stone-800 text-stone-800'
                      }`}>
                        {option.id}
                      </span>
                      <span>{option.text}</span>
                    </button>
                  ))}
                </div>

                <div className="flex justify-between items-center text-xs text-stone-500 pt-2 border-t border-stone-300/60">
                  <span className="font-bold">1 / 109</span>
                  <button className="flex items-center gap-1 text-stone-600 hover:text-stone-900 font-medium">
                    <HeartOutlined /> Câu này bó tay
                  </button>
                </div>
              </div>

              <div className="space-y-3 pt-2">
                <h4 className="font-bold text-xs text-stone-800">Choose a study mode</h4>
                <div className="grid grid-cols-2 sm:grid-cols-5 gap-3">
                  <button
                    onClick={() => setCurrentView('flashcard')}
                    className="p-3 rounded-lg bg-white hover:bg-stone-50 border-notebook text-left space-y-1.5 transition"
                  >
                    <div className="text-xs font-bold text-stone-800">Flashcards</div>
                    <span className="text-[10px] px-2 py-0.5 rounded bg-stone-100 text-stone-500 font-medium inline-block">
                      Not studied yet
                    </span>
                  </button>

                  <button
                    onClick={() => setCurrentView('quiz')}
                    className="p-3 rounded-lg bg-[#262626] text-white border-notebook text-left space-y-1.5 transition"
                  >
                    <div className="text-xs font-bold">Quiz</div>
                    <span className="text-[10px] px-2 py-0.5 rounded bg-white/20 font-medium inline-block">
                      Not studied yet
                    </span>
                  </button>

                  <button className="p-3 rounded-lg bg-white hover:bg-stone-50 border-notebook text-left space-y-1.5 transition">
                    <div className="text-xs font-bold text-stone-800">Typing</div>
                    <span className="text-[10px] px-2 py-0.5 rounded bg-stone-100 text-stone-500 font-medium inline-block">
                      Not studied yet
                    </span>
                  </button>

                  <button className="p-3 rounded-lg bg-white hover:bg-stone-50 border-notebook text-left space-y-1.5 transition">
                    <div className="text-xs font-bold text-stone-800">Reading</div>
                    <span className="text-[10px] px-2 py-0.5 rounded bg-stone-100 text-stone-500 font-medium inline-block">
                      Not studied yet
                    </span>
                  </button>

                  <button className="p-3 rounded-lg bg-white hover:bg-stone-50 border-notebook text-left space-y-1.5 transition">
                    <div className="text-xs font-bold text-stone-800">Listening</div>
                    <span className="text-[10px] px-2 py-0.5 rounded bg-stone-100 text-stone-500 font-medium inline-block">
                      Not studied yet
                    </span>
                  </button>
                </div>
              </div>
            </div>
          )}

        </div>
      </main>
    </div>
  );
}
export default App;
