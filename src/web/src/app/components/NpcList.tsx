'use client';

import React from 'react';
import { Npc } from '../lib/api';

interface NpcListProps {
  npcs: Npc[];
  activeNpcId: string | null;
  onSelect: (npc: Npc) => void;
}

const avatarTones = [
  'from-cyan-500 to-blue-600',
  'from-purple-500 to-indigo-600',
  'from-emerald-500 to-teal-600',
  'from-amber-500 to-orange-600',
  'from-pink-500 to-rose-600',
];

function getAvatarTone(id: string): string {
  const total = Array.from(id).reduce((sum, char) => sum + char.charCodeAt(0), 0);
  return avatarTones[total % avatarTones.length];
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
          className={`w-full text-left px-4 py-3 rounded-lg border-2 transition-all hover:-translate-y-0.5 ${
            activeNpcId === npc.id
              ? 'border-yellow-400 bg-yellow-400/10 text-yellow-300 shadow-[0_0_20px_rgba(250,204,21,0.2)]'
              : 'border-gray-600 bg-gray-800/60 text-gray-200 hover:border-yellow-400/50 hover:bg-gray-700/50'
           }`}
           data-testid="npc-card"
        >
          <div className="flex items-center gap-3">
            <div className={`w-10 h-10 rounded-full bg-gradient-to-br ${getAvatarTone(npc.id)} flex items-center justify-center text-white font-bold text-sm shadow-md`}>
              {npc.name.slice(0, 1).toUpperCase()}
            </div>
            <div className="min-w-0">
              <div className="font-semibold text-sm truncate">{npc.name}</div>
              <div className="text-xs opacity-75 truncate">{npc.role}</div>
            </div>
            <div className={`ml-auto w-2 h-2 rounded-full ${activeNpcId === npc.id ? 'bg-yellow-300 pulse-glow' : 'bg-gray-500'}`} />
          </div>
        </button>
      ))}
      {npcs.length === 0 && (
        <p className="text-gray-500 text-sm italic">No one to interview here.</p>
      )}
    </div>
  );
}
