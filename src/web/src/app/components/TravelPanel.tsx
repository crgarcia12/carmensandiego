'use client';

import React from 'react';
import { TravelOption } from '../lib/api';

interface TravelPanelProps {
  options: TravelOption[];
  onTravel: (cityId: string) => void;
  disabled: boolean;
}

export default function TravelPanel({ options, onTravel, disabled }: TravelPanelProps) {
  return (
    <div className="acme-note-paper p-3 space-y-2 min-h-[420px]">
      <h3 className="text-xs font-bold uppercase tracking-[0.2em] text-[#3a2a10] mb-3" style={{ fontFamily: 'monospace' }}>
        Travel Destinations
      </h3>
      {options.map((opt) => (
        <button
          key={opt.cityId}
          onClick={() => onTravel(opt.cityId)}
          disabled={disabled}
          className="w-full text-left px-2 py-1.5 border-b border-[#6f6228] text-[#1f1708] hover:bg-[#efe58f] transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <div className="font-semibold text-sm">{opt.cityName}</div>
          <div className="text-xs opacity-75 mt-0.5">{opt.description}</div>
        </button>
      ))}
      {options.length === 0 && (
        <p className="text-[#6b5c27] text-sm italic">No travel options available.</p>
      )}
    </div>
  );
}
