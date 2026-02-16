'use client';

import { useState, useEffect, useCallback } from 'react';
import { api, Session, setSessionId } from '../lib/api';

const SESSION_KEY = 'carmen-session';
const CASE_KEY = 'carmen-case';

interface StoredState {
  sessionId: string;
  caseId?: string;
}

export function useSession() {
  const [session, setSession] = useState<Session | null>(null);
  const [caseId, setCaseId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const stored = localStorage.getItem(SESSION_KEY);
    if (stored) {
      try {
        const parsed: StoredState = JSON.parse(stored);
        setSessionId(parsed.sessionId);
        api.getSession(parsed.sessionId)
          .then((s) => {
            setSession(s);
            setSessionId(s.id);
            if (parsed.caseId) setCaseId(parsed.caseId);
          })
          .catch(() => {
            localStorage.removeItem(SESSION_KEY);
            setSessionId(null);
          })
          .finally(() => setLoading(false));
      } catch {
        localStorage.removeItem(SESSION_KEY);
        setLoading(false);
      }
    } else {
      setLoading(false);
    }
  }, []);

  const persist = useCallback((sessionId: string, activeCase?: string) => {
    const state: StoredState = { sessionId, caseId: activeCase };
    localStorage.setItem(SESSION_KEY, JSON.stringify(state));
  }, []);

  const createSession = useCallback(async () => {
    const s = await api.createSession();
    setSession(s);
    setSessionId(s.id);
    persist(s.id);
    return s;
  }, [persist]);

  const storeCaseId = useCallback((id: string) => {
    setCaseId(id);
    if (session) persist(session.id, id);
    localStorage.setItem(CASE_KEY, id);
  }, [session, persist]);

  const clearSession = useCallback(() => {
    localStorage.removeItem(SESSION_KEY);
    localStorage.removeItem(CASE_KEY);
    setSession(null);
    setCaseId(null);
    setSessionId(null);
  }, []);

  return { session, caseId, loading, createSession, storeCaseId, clearSession };
}
