'use client';

import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import GameNav from '../components/GameNav';
import { useSession } from '../hooks/useSession';
import { useGame } from '../hooks/useGame';

export default function BriefingPage() {
  const router = useRouter();
  const { caseId } = useSession();
  const { caseData, loadCase, loading } = useGame();
  const [displayedText, setDisplayedText] = useState('');
  const [typewriterDone, setTypewriterDone] = useState(false);

  useEffect(() => {
    if (caseId && !caseData) {
      loadCase(caseId);
    }
  }, [caseId, caseData, loadCase]);

  useEffect(() => {
    if (!caseData?.briefing) return;
    const text = caseData.briefing;
    let i = 0;
    setDisplayedText('');
    setTypewriterDone(false);

    const interval = setInterval(() => {
      if (i < text.length) {
        setDisplayedText(text.slice(0, i + 1));
        i++;
      } else {
        setTypewriterDone(true);
        clearInterval(interval);
      }
    }, 30);

    return () => clearInterval(interval);
  }, [caseData?.briefing]);

  const handleSkip = () => {
    if (caseData?.briefing) {
      setDisplayedText(caseData.briefing);
      setTypewriterDone(true);
    }
  };

  if (loading || !caseData) {
    return (
      <div className="min-h-screen bg-gray-950 text-white">
        <GameNav showDossier={false} />
        <div className="flex items-center justify-center h-96">
          <p className="text-yellow-400 animate-pulse" style={{ fontFamily: 'monospace' }}>Loading briefing...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-950 text-white">
      <GameNav showDossier={false} />

      <div className="max-w-2xl mx-auto px-4 py-8">
        <div className="border-2 border-yellow-400 rounded-lg p-6 bg-gray-900 shadow-[0_0_20px_rgba(250,204,21,0.1)]">
          <div className="text-center mb-6">
            <p className="text-xs uppercase tracking-widest text-cyan-400 mb-2" style={{ fontFamily: 'monospace' }}>
              📋 Case Briefing
            </p>
            <h2 className="text-2xl font-bold text-yellow-400" style={{ fontFamily: 'monospace' }}>
              {caseData.title}
            </h2>
          </div>

          <div className="bg-gray-800 rounded-md p-4 mb-4 border border-gray-700">
            <p className="text-xs uppercase tracking-wider text-gray-400 mb-1" style={{ fontFamily: 'monospace' }}>
              Stolen Treasure
            </p>
            <p className="text-lg text-red-400 font-semibold">🏛️ {caseData.stolenTreasure.name}</p>
          </div>

          <div className="bg-gray-800 rounded-md p-4 mb-6 border border-gray-700 min-h-[120px]">
            <p className="text-sm text-gray-200 leading-relaxed whitespace-pre-wrap" style={{ fontFamily: 'monospace' }}>
              {displayedText}
              {!typewriterDone && <span className="animate-pulse text-yellow-400">▌</span>}
            </p>
          </div>

          <div className="flex gap-3 justify-center">
            {!typewriterDone && (
              <button
                onClick={handleSkip}
                className="px-4 py-2 rounded-md text-sm bg-gray-700 text-gray-300 hover:bg-gray-600 transition-colors"
              >
                Skip Animation
              </button>
            )}
            <button
              onClick={() => router.push('/city')}
              disabled={!typewriterDone}
              className="px-6 py-2 rounded-md text-sm font-bold uppercase tracking-wider bg-yellow-400 text-gray-900 hover:bg-yellow-300 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              style={{ fontFamily: 'monospace' }}
            >
              🔍 Start Investigation
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
