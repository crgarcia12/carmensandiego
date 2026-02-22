'use client';

import React from 'react';
import Link from 'next/link';
import StepCounter from './StepCounter';

interface GameNavProps {
  remainingSteps?: number;
  caseId?: string | null;
  showDossier?: boolean;
}

export default function GameNav({ remainingSteps, caseId, showDossier = true }: GameNavProps) {
  return (
    <nav className="acme-bezel mx-2 mt-2 px-2 py-2">
      <div className="flex items-center justify-between gap-2">
        <Link href="/" className="acme-panel px-3 py-1.5 hover:text-yellow-300 transition-colors">
          <span className="text-sm font-bold tracking-[0.2em] uppercase text-[#ff8c2b]" style={{ fontFamily: 'monospace' }}>
            ACME
          </span>
          <span className="ml-2 text-[11px] font-bold uppercase tracking-[0.16em] text-cyan-300" style={{ fontFamily: 'monospace' }}>
            Crime Net
          </span>
        </Link>

        {remainingSteps !== undefined && (
          <StepCounter remainingSteps={remainingSteps} />
        )}

        {showDossier && caseId && (
          <Link
            href="/dossier"
            className="acme-command-btn px-3 py-1 text-[11px] font-bold uppercase tracking-[0.15em] hover:brightness-105"
            style={{ fontFamily: 'monospace' }}
          >
            Dossier
          </Link>
        )}

        {!showDossier && <div className="w-[84px]" />}
      </div>
    </nav>
  );
}
