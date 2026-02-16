'use client';

import React from 'react';

interface StepCounterProps {
  steps: number;
  maxSteps: number;
}

export default function StepCounter({ steps, maxSteps }: StepCounterProps) {
  const remaining = maxSteps - steps;
  const color = remaining <= 3 ? (remaining <= 1 ? 'text-red-400' : 'text-amber-400') : 'text-green-400';

  return (
    <div className={`font-bold text-sm ${color}`} style={{ fontFamily: 'monospace' }}>
      Steps: {steps}/{maxSteps}
    </div>
  );
}
