import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { learningApi } from './api/communityApi';
import { languagesApi } from '../decks/api/languagesApi';
import { LearningLayout, QueryStatus } from './components/LearningLayout';

export function CommunityPage() {
  const [search, setSearch] = useState('');
  const [languageId, setLanguageId] = useState('');
  const [page, setPage] = useState(1);
  const [selected, setSelected] = useState('');
  const navigate = useNavigate();
  const cache = useQueryClient();
  const filter = new URLSearchParams({ page: String(page), pageSize: '12', search, ...(languageId && { languageId }) }).toString();
  const decks = useQuery({ queryKey: ['community', filter], queryFn: () => learningApi.publicDecks(filter) });
  const languages = useQuery({ queryKey: ['languages'], queryFn: languagesApi.getLanguages });
  const preview = useQuery({ queryKey: ['community-preview', selected], queryFn: () => learningApi.publicDeck(selected), enabled: Boolean(selected) });
  const clone = useMutation({ mutationFn: (id: string) => learningApi.clone(id), onSuccess: () => {
    void cache.invalidateQueries({ queryKey: ['decks'] });
    navigate('/?view=myDecks');
  } });
  return <LearningLayout title="Thư viện bộ thẻ cộng đồng">
    <div className="flex flex-wrap gap-4"><label>Tìm bộ thẻ<input className="ml-2 rounded border p-2" value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} /></label>
      <label>Ngôn ngữ<select className="ml-2 rounded border p-2" value={languageId} onChange={e => { setLanguageId(e.target.value); setPage(1); }}><option value="">Tất cả</option>{languages.data?.map(language => <option key={language.id} value={language.id}>{language.name}</option>)}</select></label>
    </div>
    <QueryStatus pending={decks.isPending} error={decks.error} empty={decks.data?.items.length === 0} onRetry={() => { void decks.refetch(); }} />
    {clone.isError && <p role="alert">{clone.error.message}</p>}
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">{decks.data?.items.map(deck => <article key={deck.id} className="space-y-3 rounded-xl border-2 border-stone-900 bg-white p-5"><h2 className="text-xl font-bold">{deck.title}</h2><p>{deck.description}</p><p>{deck.cardCount} thẻ · {deck.tags.join(', ')}</p><button className="font-bold text-rose-700" onClick={() => setSelected(deck.id)}>Xem trước</button><button disabled={clone.isPending} onClick={() => clone.mutate(deck.id)} className="block rounded bg-rose-700 px-4 py-2 text-white disabled:opacity-50">Sao chép về bộ thẻ của tôi</button></article>)}</div>
    {decks.data && <div className="flex items-center gap-4"><button disabled={!decks.data.hasPreviousPage} onClick={() => setPage(p => p - 1)}>Trang trước</button><span>Trang {page}/{Math.max(1, decks.data.totalPages)}</span><button disabled={!decks.data.hasNextPage} onClick={() => setPage(p => p + 1)}>Trang sau</button></div>}
    {selected && <section aria-label="Xem trước bộ thẻ" className="rounded-xl border-2 bg-white p-5"><button onClick={() => setSelected('')} className="float-right">Đóng xem trước</button><h2 className="text-xl font-bold">{preview.data?.deck.title ?? 'Xem trước'}</h2><QueryStatus pending={preview.isPending} error={preview.error} empty={preview.data?.cards.length === 0} />{preview.data?.cards.map((card, index) => <p className="border-t py-3" key={index}>{card.front} — {card.back}</p>)}</section>}
  </LearningLayout>;
}
