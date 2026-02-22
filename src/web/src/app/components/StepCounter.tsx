'use client';

import React from 'react';

interface StepCounterProps {
  remainingSteps: number;
  totalSteps?: number;
}

export default function StepCounter({ remainingSteps, totalSteps = 10 }: StepCounterProps) {
  const used = totalSteps - remainingSteps;
  const color = remainingSteps <= 3 ? (remainingSteps <= 1 ? 'text-red-400' : 'text-amber-300') : 'text-green-300';

  return (
    <div className={`acme-panel px-2 py-1 font-bold text-xs ${color}`} style={{ fontFamily: 'monospace' }}>
      Steps: {used}/{totalSteps} | Left: {remainingSteps}
    </div>
  );
}
