'use client';

import React from 'react';

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
    <div className={`bg-gradient-to-br ${gradient} p-6 text-white border-b-4 border-yellow-400`}>
      <div className="max-w-4xl mx-auto">
        <h2 className="text-3xl font-bold tracking-wide uppercase" style={{ fontFamily: 'monospace' }}>
          📍 {cityName}
        </h2>
        <p className="mt-1 text-sm opacity-80">{continent}</p>
        <p className="mt-2 text-sm leading-relaxed opacity-90">{description}</p>
      </div>
    </div>
  );
}
