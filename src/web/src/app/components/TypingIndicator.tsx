'use client';

import React from 'react';

interface TypingIndicatorProps {
  visible: boolean;
}

export default function TypingIndicator({ visible }: TypingIndicatorProps) {
  if (!visible) return null;

  return (
    <div
      data-testid="typing-indicator"
      className="flex justify-start mb-3"
    >
      <div className="acme-panel-inset px-3 py-2 flex items-center gap-1">
        <span className="w-2 h-2 bg-yellow-300 animate-bounce [animation-delay:0ms]" />
        <span className="w-2 h-2 bg-yellow-300 animate-bounce [animation-delay:150ms]" />
        <span className="w-2 h-2 bg-yellow-300 animate-bounce [animation-delay:300ms]" />
      </div>
    </div>
  );
}
