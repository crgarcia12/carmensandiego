'use client';

import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import GameNav from '../components/GameNav';
import SuspectCard from '../components/SuspectCard';
import WarrantDialog from '../components/WarrantDialog';
import { useSession } from '../hooks/useSession';
import { useGame } from '../hooks/useGame';
import { Suspect } from '../lib/api';

export default function DossierPage() {
  const router = useRouter();
  const { caseId } = useSession();
  const { caseData, suspects, loadCase, loadSuspects, issueWarrant, loading } = useGame();
  const [warrantTarget, setWarrantTarget] = useState<Suspect | null>(null);

  useEffect(() => {
    if (caseId) {
      loadCase(caseId);
      loadSuspects(caseId);
    }
  }, [caseId, loadCase, loadSuspects]);

  const handleWarrant = (suspectId: string) => {
    const suspect = suspects.find((s) => s.id === suspectId);
    if (suspect) setWarrantTarget(suspect);
  };

  const handleConfirmWarrant = async () => {
    if (!caseId || !warrantTarget) return;
    await issueWarrant(caseId, warrantTarget.id);
    setWarrantTarget(null);
    router.push('/summary');
  };

  return (
    <div className="min-h-screen bg-gray-950 text-white">
      <GameNav remainingSteps={caseData?.remainingSteps} caseId={caseId} showDossier={false} />

      <div className="max-w-5xl mx-auto px-4 py-6">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-2xl font-bold text-yellow-400" style={{ fontFamily: 'monospace' }}>
            📋 Suspect Dossier
          </h2>
          <button
            onClick={() => router.back()}
            className="px-3 py-1.5 rounded-md text-xs font-semibold bg-gray-700 text-gray-300 hover:bg-gray-600 transition-colors"
          >
            ← Back to City
          </button>
        </div>

        {loading && suspects.length === 0 ? (
          <div className="flex items-center justify-center h-48">
            <p className="text-yellow-400 animate-pulse" style={{ fontFamily: 'monospace' }}>Loading dossier...</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {suspects.map((suspect) => (
              <SuspectCard
                key={suspect.id}
                suspect={suspect}
                onWarrant={handleWarrant}
                disabled={loading}
              />
            ))}
          </div>
        )}

        {suspects.length === 0 && !loading && (
          <div className="text-center py-12">
            <p className="text-gray-500" style={{ fontFamily: 'monospace' }}>No suspects identified yet. Keep investigating!</p>
          </div>
        )}
      </div>

      {warrantTarget && (
        <WarrantDialog
          suspectName={warrantTarget.name}
          onConfirm={handleConfirmWarrant}
          onCancel={() => setWarrantTarget(null)}
        />
      )}
    </div>
  );
}
