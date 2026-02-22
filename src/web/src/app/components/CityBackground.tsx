'use client';

import React from 'react';
import Image from 'next/image';

interface CityBackgroundProps {
  cityName: string;
  description: string;
  continent: string;
}

export default function CityBackground({ cityName, description, continent }: CityBackgroundProps) {
  return (
    <div className="max-w-7xl mx-auto px-4 pt-3">
      <div className="acme-bezel p-2">
        <div className="acme-panel relative overflow-hidden p-3">
          <div className="absolute inset-0 city-grid-overlay opacity-15 pointer-events-none" />
          <div className="relative z-10 flex flex-col md:flex-row md:items-center md:justify-between gap-4">
            <div>
              <p className="text-[10px] uppercase tracking-[0.3em] text-cyan-400 mb-1" style={{ fontFamily: 'monospace' }}>
                Location Feed
              </p>
              <h2 className="text-3xl font-bold tracking-wide uppercase text-yellow-300" style={{ fontFamily: 'monospace' }}>
                {cityName}
              </h2>
              <p className="mt-1 text-sm text-cyan-300">{continent}</p>
              <p className="mt-2 text-sm leading-relaxed text-gray-100">{description}</p>
            </div>
            <div className="acme-bezel w-full md:w-[320px] p-1">
              <Image
                src="/detective-briefing-board.svg"
                alt="Detective briefing board illustration"
                width={320}
                height={180}
                className="w-full h-auto border-2 border-black"
                priority
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
