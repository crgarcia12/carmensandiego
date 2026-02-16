const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5001';

export interface Session {
  id: string;
  status: string;
  createdAt: string;
}

export interface CaseData {
  id: string;
  title: string;
  stolenTreasure: string;
  narrative: string;
  status: string;
  steps: number;
  maxSteps: number;
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

export interface CityData {
  id: string;
  name: string;
  description: string;
  continent: string;
  npcs: Npc[];
  travelOptions: TravelOption[];
}

export interface NpcChatResponse {
  message: string;
  npcName: string;
}

export interface Suspect {
  id: string;
  name: string;
  traits: string[];
}

export interface WarrantResult {
  correct: boolean;
  message: string;
}

export interface CaseSummary {
  outcome: 'win' | 'lose';
  message: string;
  citiesVisited: string[];
  correctSuspect: string;
  steps: number;
}

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
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

  travel(caseId: string, cityId: string): Promise<CityData> {
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

  getSuspects(caseId: string): Promise<Suspect[]> {
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
