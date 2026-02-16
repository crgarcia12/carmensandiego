'use client';

import React from 'react';

interface RetroPixelHeaderProps {
  title: string;
  subtitle?: string;
}

export default function RetroPixelHeader({ title, subtitle }: RetroPixelHeaderProps) {
  return (
    <div className="text-center py-8">
      <div className="inline-block border-4 border-yellow-400 rounded-lg p-6 bg-gray-900 shadow-[0_0_30px_rgba(250,204,21,0.2)]">
        <h1
          className="text-4xl md:text-5xl font-bold text-yellow-400 tracking-widest uppercase"
          style={{ fontFamily: 'monospace', textShadow: '3px 3px 0 #1a1a2e' }}
        >
          {title}
        </h1>
        {subtitle && (
          <p className="mt-2 text-sm text-cyan-400 tracking-wider uppercase" style={{ fontFamily: 'monospace' }}>
            {subtitle}
          </p>
        )}
      </div>
    </div>
  );
}
