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
    <div className="acme-panel p-3 space-y-2">
      <h3 className="text-xs font-bold uppercase tracking-[0.2em] text-yellow-300 mb-3" style={{ fontFamily: 'monospace' }}>
        People to Interview
      </h3>
      {npcs.map((npc) => (
        <button
          key={npc.id}
          onClick={() => onSelect(npc)}
          className={`w-full text-left px-3 py-2 border-2 transition-all ${
             activeNpcId === npc.id
               ? 'acme-command-btn brightness-105'
               : 'acme-command-btn hover:brightness-110'
            }`}
            data-testid="npc-card"
        >
          <div className="flex items-center gap-2">
            <div className={`w-8 h-8 border border-[#49331c] bg-gradient-to-br ${getAvatarTone(npc.id)} flex items-center justify-center text-white font-bold text-xs`}>
              {npc.name.slice(0, 1).toUpperCase()}
            </div>
            <div className="min-w-0">
              <div className="font-bold text-xs truncate text-[#1c1307]">{npc.name}</div>
              <div className="text-[11px] opacity-85 truncate text-[#2b1b0b]">{npc.role}</div>
            </div>
            <div className={`ml-auto w-2 h-2 ${activeNpcId === npc.id ? 'bg-[#1d4f0b]' : 'bg-[#724d25]'}`} />
          </div>
        </button>
      ))}
      {npcs.length === 0 && (
        <p className="text-gray-400 text-xs italic">No one to interview here.</p>
      )}
    </div>
  );
}
