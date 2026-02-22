'use client';

import React from 'react';
import { Suspect } from '../lib/api';

interface SuspectCardProps {
  suspect: Suspect;
  onWarrant: (suspectId: string) => void;
  disabled: boolean;
}

export default function SuspectCard({ suspect, onWarrant, disabled }: SuspectCardProps) {
  return (
    <div className="acme-bezel p-2">
      <div className="acme-panel p-3 h-full">
        {/* Photo placeholder */}
        <div className="w-full h-28 acme-panel-inset mb-3 flex items-center justify-center">
          <span className="text-4xl">🕵️</span>
        </div>

        <h4 className="font-bold text-sm text-yellow-300 mb-2 uppercase tracking-[0.08em]" style={{ fontFamily: 'monospace' }}>
          {suspect.name}
        </h4>

        <ul className="space-y-1 mb-3">
          {Object.entries(suspect.traits).map(([key, value]) => (
            <li key={key} className="text-xs text-yellow-100 flex items-start gap-1">
              <span className="text-yellow-400">▸</span>
              <span className="capitalize text-yellow-300">{key.replace(/([A-Z])/g, ' $1').trim()}:</span> {value}
            </li>
          ))}
        </ul>

        <button
          onClick={() => onWarrant(suspect.id)}
          disabled={disabled}
          className="w-full py-2 px-3 acme-command-btn text-xs font-bold uppercase tracking-[0.1em] hover:brightness-110 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          style={{ fontFamily: 'monospace' }}
        >
          Issue Warrant
        </button>
      </div>
    </div>
  );
}
