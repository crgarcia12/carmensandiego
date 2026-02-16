'use client';

import React, { useState, useRef, useCallback, KeyboardEvent } from 'react';

interface ChatInputProps {
  onSend: (message: string) => void;
  disabled: boolean;
}

export default function ChatInput({ onSend, disabled }: ChatInputProps) {
  const [value, setValue] = useState('');
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  const isEmpty = value.trim().length === 0;

  const handleSend = useCallback(() => {
    if (isEmpty || disabled) return;
    onSend(value);
    setValue('');
    if (textareaRef.current) {
      textareaRef.current.style.height = 'auto';
    }
  }, [value, isEmpty, disabled, onSend]);

  const handleKeyDown = useCallback(
    (e: KeyboardEvent<HTMLTextAreaElement>) => {
      if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        handleSend();
      }
    },
    [handleSend]
  );

  const MAX_CHARS = 280;

  const handleChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    const newValue = e.target.value;
    if (newValue.length <= MAX_CHARS) {
      setValue(newValue);
    }
    // Auto-resize
    const ta = e.target;
    ta.style.height = 'auto';
    ta.style.height = `${Math.min(ta.scrollHeight, 150)}px`;
  };

  return (
    <div className="relative flex items-end gap-2 p-3 border-t border-gray-700 bg-gray-800">
      <textarea
        ref={textareaRef}
        role="textbox"
        aria-label="Message"
        tabIndex={1}
        value={value}
        onChange={handleChange}
        onKeyDown={handleKeyDown}
        disabled={disabled}
        placeholder="Type a message..."
        rows={1}
        className="flex-1 resize-none rounded-xl border border-gray-600 bg-gray-900 text-gray-100 px-4 py-2.5 text-sm focus:border-yellow-400 focus:outline-none focus:ring-1 focus:ring-yellow-400 disabled:bg-gray-800 disabled:cursor-not-allowed placeholder-gray-500"
      />
      <button
        type="button"
        onClick={handleSend}
        aria-label="Send"
        tabIndex={2}
        aria-disabled={disabled || isEmpty}
        className={`flex h-10 w-10 items-center justify-center rounded-full transition-colors ${
          disabled || isEmpty
            ? 'bg-gray-600 cursor-not-allowed text-gray-400'
            : 'bg-yellow-400 text-gray-900 hover:bg-yellow-300'
        }`}
      >
        <svg
          xmlns="http://www.w3.org/2000/svg"
          viewBox="0 0 24 24"
          fill="currentColor"
          className="h-5 w-5"
        >
          <path d="M3.478 2.404a.75.75 0 0 0-.926.941l2.432 7.905H13.5a.75.75 0 0 1 0 1.5H4.984l-2.432 7.905a.75.75 0 0 0 .926.94 60.519 60.519 0 0 0 18.445-8.986.75.75 0 0 0 0-1.218A60.517 60.517 0 0 0 3.478 2.404Z" />
        </svg>
      </button>
      <span className="absolute bottom-1 right-14 text-xs text-gray-500" style={{ fontFamily: 'monospace' }}>
        {value.length}/{MAX_CHARS}
      </span>
    </div>
  );
}
