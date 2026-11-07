
import { useState } from 'react';

import {
  useCreateDeck,
  useUpdateDeck,
} from './useDecks';

import type {
  CreateDeckRequest,
  DeckResponse,
} from '../types/deck.types';

export function useDeckEditor() {
  const [isOpen, setIsOpen] = useState(false);

  const [editingDeck, setEditingDeck] =
    useState<DeckResponse | null>(null);

  const createMutation = useCreateDeck();
  const updateMutation = useUpdateDeck();

  function openCreate() {
    setEditingDeck(null);
    setIsOpen(true);
  }

  function openEdit(deck: DeckResponse) {
    setEditingDeck(deck);
    setIsOpen(true);
  }

  function closeEditor() {
    setIsOpen(false);
    setEditingDeck(null);
  }

  async function submitDeck(request: CreateDeckRequest) {
    if (editingDeck) {
      await updateMutation.mutateAsync({
        id: editingDeck.id,
        request: {
          ...request,
          baseVersion: editingDeck.version,
        },
      });
    } else {
      await createMutation.mutateAsync(request);
    }
  }

  return {
    isOpen,
    editingDeck,
    openCreate,
    openEdit,
    closeEditor,
    submitDeck,
    isSaving:
      createMutation.isPending ||
      updateMutation.isPending,
  };
}
