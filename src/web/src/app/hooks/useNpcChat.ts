'use client';

import { useState, useCallback } from 'react';
import { api } from '../lib/api';

export interface NpcMessage {
  id: string;
  role: 'user' | 'npc';
  content: string;
  npcName?: string;
}

export function useNpcChat() {
  const [messages, setMessages] = useState<NpcMessage[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  const sendMessage = useCallback(async (caseId: string, npcId: string, npcName: string, text: string) => {
    const trimmed = text.trim();
    if (!trimmed) return;

    const userMsg: NpcMessage = {
      id: `user-${Date.now()}`,
      role: 'user',
      content: trimmed,
    };
    setMessages((prev) => [...prev, userMsg]);
    setIsLoading(true);

    try {
      const res = await api.chatWithNpc(caseId, npcId, trimmed);
      const npcMsg: NpcMessage = {
        id: `npc-${Date.now()}`,
        role: 'npc',
        content: res.npcMessage.text,
        npcName: res.npcMessage.npcName || npcName,
      };
      setMessages((prev) => [...prev, npcMsg]);
    } catch {
      const errMsg: NpcMessage = {
        id: `error-${Date.now()}`,
        role: 'npc',
        content: 'Unable to reach this person right now. Try again.',
        npcName: npcName,
      };
      setMessages((prev) => [...prev, errMsg]);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const clearMessages = useCallback(() => {
    setMessages([]);
  }, []);

  return { messages, isLoading, sendMessage, clearMessages };
}
