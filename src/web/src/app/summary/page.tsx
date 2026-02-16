'use client';

import React, { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import GameNav from '../components/GameNav';
import { useSession } from '../hooks/useSession';
import { useGame } from '../hooks/useGame';

export default function SummaryPage() {
  const router = useRouter();
  const { caseId, clearSession } = useSession();
  const { summary, loadSummary, loading } = useGame();

  useEffect(() => {
    if (caseId) {
      loadSummary(caseId);
    }
  }, [caseId, loadSummary]);

  const handlePlayAgain = () => {
    clearSession();
    router.push('/');
  };

  if (loading || !summary) {
    return (
      <div className="min-h-screen bg-gray-950 text-white">
        <GameNav showDossier={false} />
        <div className="flex items-center justify-center h-96">
          <p className="text-yellow-400 animate-pulse" style={{ fontFamily: 'monospace' }}>Loading case summary...</p>
        </div>
      </div>
    );
  }

  const isWin = summary.outcome === 'won';

  return (
    <div className="min-h-screen bg-gray-950 text-white">
      <GameNav showDossier={false} />

      <div className="max-w-2xl mx-auto px-4 py-8">
        <div className={`border-4 rounded-lg p-8 text-center ${
          isWin ? 'border-green-400 bg-green-900/20' : 'border-red-400 bg-red-900/20'
        }`}>
          <div className="text-6xl mb-4">{isWin ? '🎉' : '😔'}</div>
          <h2
            className={`text-3xl font-bold mb-4 ${isWin ? 'text-green-400' : 'text-red-400'}`}
            style={{ fontFamily: 'monospace' }}
          >
            {isWin ? 'Case Closed!' : 'Case Failed'}
          </h2>
          <p className="text-gray-200 text-sm leading-relaxed mb-6">
            {isWin
              ? `You caught the thief and recovered the ${summary.stolenTreasure.name}!`
              : `The ${summary.stolenTreasure.name} was not recovered. The correct suspect was ${summary.correctSuspect?.name ?? 'unknown'}.`}
          </p>
        </div>

        <div className="mt-6 space-y-4">
          <div className="bg-gray-900 rounded-lg p-4 border border-gray-700">
            <h3 className="text-sm font-bold uppercase tracking-wider text-yellow-400 mb-2" style={{ fontFamily: 'monospace' }}>
              📊 Case Statistics
            </h3>
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <p className="text-gray-400">Steps Taken</p>
                <p className="text-white font-bold">{summary.stepsUsed} / {summary.totalSteps}</p>
              </div>
              <div>
                <p className="text-gray-400">Correct Suspect</p>
                <p className="text-yellow-400 font-bold">{summary.correctSuspect?.name ?? 'Unknown'}</p>
              </div>
            </div>
          </div>

          <div className="bg-gray-900 rounded-lg p-4 border border-gray-700">
            <h3 className="text-sm font-bold uppercase tracking-wider text-yellow-400 mb-2" style={{ fontFamily: 'monospace' }}>
              🌍 Cities Visited
            </h3>
            <div className="flex flex-wrap gap-2">
              {summary.citiesVisited.map((city, i) => (
                <span key={i} className="px-3 py-1 rounded-full text-xs bg-gray-700 text-gray-200">
                  {city}
                </span>
              ))}
            </div>
          </div>
        </div>

        <div className="mt-8 text-center">
          <button
            onClick={handlePlayAgain}
            className="px-8 py-3 rounded-lg text-sm font-bold uppercase tracking-wider bg-yellow-400 text-gray-900 hover:bg-yellow-300 transition-colors"
            style={{ fontFamily: 'monospace' }}
          >
            🔄 Play Again
          </button>
        </div>
      </div>
    </div>
  );
}
