'use client';

import React from 'react';
import Image from 'next/image';

const continentGradients: Record<string, string> = {
  'Europe': 'from-blue-800 to-indigo-900',
  'Asia': 'from-red-800 to-orange-900',
  'Africa': 'from-amber-700 to-yellow-900',
  'North America': 'from-emerald-800 to-teal-900',
  'South America': 'from-green-700 to-lime-900',
  'Oceania': 'from-cyan-700 to-blue-900',
};

interface CityBackgroundProps {
  cityName: string;
  description: string;
  continent: string;
}

export default function CityBackground({ cityName, description, continent }: CityBackgroundProps) {
  const gradient = continentGradients[continent] || 'from-gray-800 to-gray-900';

  return (
    <div className={`relative overflow-hidden bg-gradient-to-br ${gradient} p-6 text-white border-b-4 border-yellow-400`}>
      <div className="absolute inset-0 city-grid-overlay opacity-40 pointer-events-none" />
      <div className="absolute -top-14 -right-14 w-40 h-40 rounded-full bg-yellow-400/20 blur-2xl pulse-glow pointer-events-none" />
      <div className="absolute -bottom-12 -left-10 w-32 h-32 rounded-full bg-amber-300/20 blur-2xl pulse-glow pointer-events-none" />

      <div className="max-w-7xl mx-auto relative z-10 flex flex-col md:flex-row md:items-center md:justify-between gap-5">
        <div>
          <h2 className="text-3xl font-bold tracking-wide uppercase" style={{ fontFamily: 'monospace' }}>
            📍 {cityName}
          </h2>
          <p className="mt-1 text-sm opacity-80">{continent}</p>
          <p className="mt-2 text-sm leading-relaxed opacity-90">{description}</p>
        </div>
        <div className="w-full md:w-[260px] rounded-lg border border-yellow-300/40 bg-gray-950/40 p-2 float-gentle">
          <Image
            src="/detective-briefing-board.svg"
            alt="Detective briefing board illustration"
            width={260}
            height={146}
            className="w-full h-auto rounded-md opacity-95"
            priority
          />
        </div>
      </div>
    </div>
  );
}
