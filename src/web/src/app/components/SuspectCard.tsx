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
    <div className="bg-gray-800 border-2 border-gray-600 rounded-lg p-4 hover:border-yellow-400 transition-all">
      {/* Photo placeholder */}
      <div className="w-full h-24 bg-gradient-to-br from-gray-700 to-gray-600 rounded-md mb-3 flex items-center justify-center">
        <span className="text-4xl">🕵️</span>
      </div>

      <h4 className="font-bold text-sm text-yellow-400 mb-2" style={{ fontFamily: 'monospace' }}>
        {suspect.name}
      </h4>

      <ul className="space-y-1 mb-3">
        {suspect.traits.map((trait, i) => (
          <li key={i} className="text-xs text-gray-300 flex items-start gap-1">
            <span className="text-yellow-500">▸</span>
            {trait}
          </li>
        ))}
      </ul>

      <button
        onClick={() => onWarrant(suspect.id)}
        disabled={disabled}
        className="w-full py-2 px-3 rounded-md text-xs font-bold uppercase tracking-wider bg-red-700 text-white hover:bg-red-600 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
        style={{ fontFamily: 'monospace' }}
      >
        🚨 Issue Warrant
      </button>
    </div>
  );
}
