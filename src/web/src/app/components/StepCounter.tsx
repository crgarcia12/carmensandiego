'use client';

import React from 'react';

interface StepCounterProps {
  remainingSteps: number;
  totalSteps?: number;
}

export default function StepCounter({ remainingSteps, totalSteps = 10 }: StepCounterProps) {
  const used = totalSteps - remainingSteps;
  const color = remainingSteps <= 3 ? (remainingSteps <= 1 ? 'text-red-400' : 'text-amber-400') : 'text-green-400';

  return (
    <div className={`font-bold text-sm ${color}`} style={{ fontFamily: 'monospace' }}>
      Steps: {used}/{totalSteps} (🦶 {remainingSteps} left)
    </div>
  );
}
