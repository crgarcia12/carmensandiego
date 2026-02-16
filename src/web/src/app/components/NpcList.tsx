'use client';

import React from 'react';
import { Npc } from '../lib/api';

interface NpcListProps {
  npcs: Npc[];
  activeNpcId: string | null;
  onSelect: (npc: Npc) => void;
}

export default function NpcList({ npcs, activeNpcId, onSelect }: NpcListProps) {
  return (
    <div className="space-y-2">
      <h3 className="text-sm font-bold uppercase tracking-wider text-yellow-400 mb-3" style={{ fontFamily: 'monospace' }}>
        🗣️ People to Interview
      </h3>
      {npcs.map((npc) => (
        <button
          key={npc.id}
          onClick={() => onSelect(npc)}
          className={`w-full text-left px-4 py-3 rounded-lg border-2 transition-all ${
            activeNpcId === npc.id
              ? 'border-yellow-400 bg-yellow-400/10 text-yellow-300'
              : 'border-gray-600 bg-gray-800/50 text-gray-200 hover:border-yellow-400/50 hover:bg-gray-700/50'
          }`}
        >
          <div className="font-semibold text-sm">{npc.name}</div>
          <div className="text-xs opacity-70">{npc.role}</div>
        </button>
      ))}
      {npcs.length === 0 && (
        <p className="text-gray-500 text-sm italic">No one to interview here.</p>
      )}
    </div>
  );
}
