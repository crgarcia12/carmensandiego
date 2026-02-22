'use client';

import React from 'react';

interface RetroPixelHeaderProps {
  title: string;
  subtitle?: string;
}

export default function RetroPixelHeader({ title, subtitle }: RetroPixelHeaderProps) {
  return (
    <div className="text-center py-7">
      <div className="retro-border inline-block bg-gray-900 px-8 py-5">
        <h1
          className="text-4xl md:text-5xl font-bold text-yellow-400 tracking-[0.22em] uppercase leading-tight"
          style={{
            fontFamily: 'var(--font-geist-mono), monospace',
            textShadow: '2px 2px 0 #2c1f00, -1px -1px 0 #000',
          }}
        >
          {title}
        </h1>
        {subtitle && (
          <p
            className="mt-3 text-xs text-cyan-400 tracking-[0.3em] uppercase"
            style={{ fontFamily: 'var(--font-geist-mono), monospace' }}
          >
            {subtitle}
          </p>
        )}
      </div>
    </div>
  );
}
