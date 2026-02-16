'use client';

import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import GameNav from '../components/GameNav';
import CityBackground from '../components/CityBackground';
import NpcList from '../components/NpcList';
import NpcChat from '../components/NpcChat';
import TravelPanel from '../components/TravelPanel';
import { useSession } from '../hooks/useSession';
import { useGame } from '../hooks/useGame';
import { useNpcChat } from '../hooks/useNpcChat';
import { Npc } from '../lib/api';

type MobileTab = 'city' | 'chat' | 'travel';

export default function CityPage() {
  const router = useRouter();
  const { caseId } = useSession();
  const { caseData, cityData, loadCase, loadCity, travel, loading } = useGame();
  const { messages, isLoading: chatLoading, sendMessage, clearMessages } = useNpcChat();
  const [activeNpc, setActiveNpc] = useState<Npc | null>(null);
  const [mobileTab, setMobileTab] = useState<MobileTab>('city');

  useEffect(() => {
    if (caseId) {
      loadCase(caseId);
      loadCity(caseId);
    }
  }, [caseId, loadCase, loadCity]);

  useEffect(() => {
    if (caseData?.status === 'completed') {
      router.push('/summary');
    }
  }, [caseData?.status, router]);

  const handleSelectNpc = (npc: Npc) => {
    setActiveNpc(npc);
    clearMessages();
    setMobileTab('chat');
  };

  const handleSendMessage = (text: string) => {
    if (!caseId || !activeNpc) return;
    sendMessage(caseId, activeNpc.id, activeNpc.name, text);
  };

  const handleTravel = async (cityId: string) => {
    if (!caseId) return;
    setActiveNpc(null);
    clearMessages();
    await travel(caseId, cityId);
    setMobileTab('city');
  };

  if (loading && !cityData) {
    return (
      <div className="min-h-screen bg-gray-950 text-white">
        <GameNav remainingSteps={caseData?.remainingSteps} caseId={caseId} />
        <div className="flex items-center justify-center h-96">
          <p className="text-yellow-400 animate-pulse" style={{ fontFamily: 'monospace' }}>Arriving at destination...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-950 text-white">
      <GameNav remainingSteps={caseData?.remainingSteps} caseId={caseId} />

      {cityData && (
        <CityBackground
          cityName={cityData.city.name}
          description={`${cityData.city.region}, ${cityData.city.continent}`}
          continent={cityData.city.continent}
        />
      )}

      {/* Mobile tab navigation */}
      <div className="lg:hidden flex border-b border-gray-700">
        {(['city', 'chat', 'travel'] as MobileTab[]).map((tab) => (
          <button
            key={tab}
            onClick={() => setMobileTab(tab)}
            className={`flex-1 py-2 text-xs font-bold uppercase tracking-wider transition-colors ${
              mobileTab === tab
                ? 'text-yellow-400 border-b-2 border-yellow-400 bg-gray-800'
                : 'text-gray-400 hover:text-gray-200'
            }`}
            style={{ fontFamily: 'monospace' }}
          >
            {tab === 'city' ? '🏙️ City' : tab === 'chat' ? '💬 Chat' : '✈️ Travel'}
          </button>
        ))}
      </div>

      <div className="max-w-7xl mx-auto px-4 py-4">
        {/* Desktop: 3-column layout */}
        <div className="hidden lg:grid lg:grid-cols-3 lg:gap-4">
          {/* Left: NPCs */}
          <div className="bg-gray-900 rounded-lg p-4 border border-gray-700">
            {cityData && (
              <NpcList
                npcs={cityData.npcs}
                activeNpcId={activeNpc?.id || null}
                onSelect={handleSelectNpc}
              />
            )}
          </div>

          {/* Center: Chat */}
          <div className="min-h-[400px]">
            {activeNpc ? (
              <NpcChat
                npcName={activeNpc.name}
                messages={messages}
                isLoading={chatLoading}
                onSend={handleSendMessage}
              />
            ) : (
              <div className="flex items-center justify-center h-full bg-gray-900 rounded-lg border-2 border-gray-700">
                <p className="text-gray-500 text-sm" style={{ fontFamily: 'monospace' }}>
                  Select a person to interview
                </p>
              </div>
            )}
          </div>

          {/* Right: Travel */}
          <div className="bg-gray-900 rounded-lg p-4 border border-gray-700">
            {cityData && (
              <TravelPanel
                options={cityData.travelOptions}
                onTravel={handleTravel}
                disabled={loading}
              />
            )}
          </div>
        </div>

        {/* Mobile: Tab content */}
        <div className="lg:hidden">
          {mobileTab === 'city' && cityData && (
            <div className="bg-gray-900 rounded-lg p-4 border border-gray-700">
              <NpcList
                npcs={cityData.npcs}
                activeNpcId={activeNpc?.id || null}
                onSelect={handleSelectNpc}
              />
            </div>
          )}

          {mobileTab === 'chat' && (
            <div className="min-h-[400px]">
              {activeNpc ? (
                <NpcChat
                  npcName={activeNpc.name}
                  messages={messages}
                  isLoading={chatLoading}
                  onSend={handleSendMessage}
                />
              ) : (
                <div className="flex items-center justify-center h-96 bg-gray-900 rounded-lg border-2 border-gray-700">
                  <p className="text-gray-500 text-sm" style={{ fontFamily: 'monospace' }}>
                    Select a person from the City tab
                  </p>
                </div>
              )}
            </div>
          )}

          {mobileTab === 'travel' && cityData && (
            <div className="bg-gray-900 rounded-lg p-4 border border-gray-700">
              <TravelPanel
                options={cityData.travelOptions}
                onTravel={handleTravel}
                disabled={loading}
              />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
