'use client';

import { useState, useCallback } from 'react';
import { api, CaseData, CityData, Suspect, CaseSummary, WarrantResult, TravelResult } from '../lib/api';

export function useGame() {
  const [caseData, setCaseData] = useState<CaseData | null>(null);
  const [cityData, setCityData] = useState<CityData | null>(null);
  const [suspects, setSuspects] = useState<Suspect[]>([]);
  const [summary, setSummary] = useState<CaseSummary | null>(null);
  const [warrantResult, setWarrantResult] = useState<WarrantResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const startCase = useCallback(async (sessionId: string) => {
    setLoading(true);
    setError(null);
    try {
      const c = await api.createCase(sessionId);
      setCaseData(c);
      return c;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to start case');
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  const loadCase = useCallback(async (caseId: string) => {
    setLoading(true);
    setError(null);
    try {
      const c = await api.getCase(caseId);
      setCaseData(c);
      return c;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load case');
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  const loadCity = useCallback(async (caseId: string) => {
    setLoading(true);
    setError(null);
    try {
      const city = await api.getCity(caseId);
      setCityData(city);
      return city;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load city');
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  const travel = useCallback(async (caseId: string, cityId: string) => {
    setLoading(true);
    setError(null);
    try {
      const result = await api.travel(caseId, cityId);
      // After travel, reload full city data and case data
      const [city, c] = await Promise.all([
        api.getCity(caseId),
        api.getCase(caseId),
      ]);
      setCityData(city);
      setCaseData(c);
      return result;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to travel');
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  const loadSuspects = useCallback(async (caseId: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.getSuspects(caseId);
      setSuspects(response.suspects);
      return response.suspects;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load suspects');
      return [];
    } finally {
      setLoading(false);
    }
  }, []);

  const issueWarrant = useCallback(async (caseId: string, suspectId: string) => {
    setLoading(true);
    setError(null);
    try {
      const result = await api.issueWarrant(caseId, suspectId);
      setWarrantResult(result);
      return result;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to issue warrant');
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  const loadSummary = useCallback(async (caseId: string) => {
    setLoading(true);
    setError(null);
    try {
      const s = await api.getSummary(caseId);
      setSummary(s);
      return s;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load summary');
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    caseData, cityData, suspects, summary, warrantResult,
    loading, error,
    startCase, loadCase, loadCity, travel,
    loadSuspects, issueWarrant, loadSummary,
  };
}
