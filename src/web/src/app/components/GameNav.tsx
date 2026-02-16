'use client';

import React from 'react';
import Link from 'next/link';
import StepCounter from './StepCounter';

interface GameNavProps {
  steps?: number;
  maxSteps?: number;
  caseId?: string | null;
  showDossier?: boolean;
}

export default function GameNav({ steps, maxSteps, caseId, showDossier = true }: GameNavProps) {
  return (
    <nav className="flex items-center justify-between px-4 py-3 bg-gray-900 border-b-2 border-yellow-400">
      <Link href="/" className="flex items-center gap-2 text-yellow-400 hover:text-yellow-300 transition-colors">
        <span className="text-xl">🔍</span>
        <span className="text-lg font-bold tracking-wider uppercase" style={{ fontFamily: 'monospace' }}>
          Carmen Sandiego
        </span>
      </Link>

      {steps !== undefined && maxSteps !== undefined && (
        <StepCounter steps={steps} maxSteps={maxSteps} />
      )}

      {showDossier && caseId && (
        <Link
          href="/dossier"
          className="px-3 py-1.5 rounded-md text-xs font-bold uppercase tracking-wider bg-yellow-400 text-gray-900 hover:bg-yellow-300 transition-colors"
          style={{ fontFamily: 'monospace' }}
        >
          📋 Dossier
        </Link>
      )}

      {!showDossier && <div />}
    </nav>
  );
}
