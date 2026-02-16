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
    <div className="space-y-2">
      <h3 className="text-sm font-bold uppercase tracking-wider text-yellow-400 mb-3" style={{ fontFamily: 'monospace' }}>
        ✈️ Travel Destinations
      </h3>
      {options.map((opt) => (
        <button
          key={opt.cityId}
          onClick={() => onTravel(opt.cityId)}
          disabled={disabled}
          className="w-full text-left px-4 py-3 rounded-lg border-2 border-gray-600 bg-gray-800/50 text-gray-200 hover:border-cyan-400 hover:bg-gray-700/50 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <div className="font-semibold text-sm">🌍 {opt.cityName}</div>
          <div className="text-xs opacity-70 mt-1">{opt.description}</div>
        </button>
      ))}
      {options.length === 0 && (
        <p className="text-gray-500 text-sm italic">No travel options available.</p>
      )}
    </div>
  );
}
