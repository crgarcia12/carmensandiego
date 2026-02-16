const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5001';

export interface Session {
  id: string;
  status: string;
  createdAt: string;
}

export interface StolenTreasure {
  name: string;
  description: string;
}

export interface CaseData {
  id: string;
  title: string;
  briefing: string;
  stolenTreasure: StolenTreasure;
  currentCity: string;
  remainingSteps: number;
  status: string;
  trail: string[];
  correctSuspectId: string;
  currentCityIndex: number;
  visitedCities: string[];
  warrantIssued: boolean;
}

export interface Npc {
  id: string;
  name: string;
  role: string;
}

export interface TravelOption {
  cityId: string;
  cityName: string;
  description: string;
}

export interface CityInfo {
  id: string;
  name: string;
  region: string;
  continent: string;
  backgroundKey: string;
}

export interface CityData {
  city: CityInfo;
  npcs: Npc[];
  travelOptions: TravelOption[];
  remainingSteps: number;
  isFinalCity: boolean;
}

export interface TravelResult {
  city: CityInfo;
  remainingSteps: number;
  caseStatus: string;
}

export interface NpcChatResponse {
  npcMessage: {
    npcId: string;
    npcName: string;
    text: string;
    timestamp: string;
  };
  chatHistory: {
    messageCount: number;
    remainingMessages: number;
  };
}

export interface SuspectTraits {
  hairColor: string;
  eyeColor: string;
  hobby: string;
  favoriteFood: string;
  vehicle: string;
  distinguishingFeature: string;
}

export interface Suspect {
  id: string;
  name: string;
  photoKey: string;
  traits: SuspectTraits;
}

export interface SuspectListResponse {
  suspects: Suspect[];
}

export interface WarrantResult {
  result: 'won' | 'lost';
  caseStatus: string;
  message?: string;
  reason?: string;
  warrant?: { suspectId: string; cityId: string; issuedAt: string };
  correctSuspect?: { id: string; name: string };
}

export interface CaseSummary {
  outcome: 'won' | 'lost';
  citiesVisited: string[];
  stepsUsed: number;
  totalSteps: number;
  correctSuspect: { id: string; name: string } | null;
  playerWarrant: { suspectId: string } | null;
  stolenTreasure: StolenTreasure;
}

// Session ID is stored and passed to all /api/cases/* calls
let _sessionId: string | null = null;

export function setSessionId(id: string | null) {
  _sessionId = id;
}

export function getSessionId(): string | null {
  return _sessionId;
}

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options?.headers as Record<string, string> || {}),
  };

  // Auto-attach session ID for /api/cases/* endpoints
  if (path.startsWith('/api/cases') && _sessionId && !headers['X-Session-Id']) {
    headers['X-Session-Id'] = _sessionId;
  }

  const res = await fetch(`${API_URL}${path}`, {
    ...options,
    headers,
  });
  if (!res.ok) {
    const body = await res.text().catch(() => '');
    throw new Error(body || `${res.status} ${res.statusText}`);
  }
  return res.json();
}

export const api = {
  createSession(): Promise<Session> {
    return request('/api/sessions', { method: 'POST' });
  },

  getSession(id: string): Promise<Session> {
    return request(`/api/sessions/${id}`);
  },

  createCase(sessionId: string): Promise<CaseData> {
    return request('/api/cases', {
      method: 'POST',
      headers: { 'X-Session-Id': sessionId },
    });
  },

  getCase(caseId: string): Promise<CaseData> {
    return request(`/api/cases/${caseId}`);
  },

  getCity(caseId: string): Promise<CityData> {
    return request(`/api/cases/${caseId}/city`);
  },

  travel(caseId: string, cityId: string): Promise<TravelResult> {
    return request(`/api/cases/${caseId}/travel`, {
      method: 'POST',
      body: JSON.stringify({ cityId }),
    });
  },

  chatWithNpc(caseId: string, npcId: string, message: string): Promise<NpcChatResponse> {
    return request(`/api/cases/${caseId}/npcs/${npcId}/chat`, {
      method: 'POST',
      body: JSON.stringify({ message }),
    });
  },

  getSuspects(caseId: string): Promise<SuspectListResponse> {
    return request(`/api/cases/${caseId}/suspects`);
  },

  issueWarrant(caseId: string, suspectId: string): Promise<WarrantResult> {
    return request(`/api/cases/${caseId}/warrant`, {
      method: 'POST',
      body: JSON.stringify({ suspectId }),
    });
  },

  getSummary(caseId: string): Promise<CaseSummary> {
    return request(`/api/cases/${caseId}/summary`);
  },
};
