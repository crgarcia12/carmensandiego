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
    <div className="relative flex items-end gap-2 p-2 border-t border-[#6a471f] bg-[#b89167]">
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
        className="flex-1 resize-none border-2 border-[#6a471f] bg-black text-yellow-100 px-3 py-2 text-sm focus:border-cyan-300 focus:outline-none disabled:bg-[#111] disabled:cursor-not-allowed placeholder-gray-500"
      />
      <button
        type="button"
        onClick={handleSend}
        aria-label="Send"
        tabIndex={2}
        aria-disabled={disabled || isEmpty}
        className={`flex h-10 min-w-[56px] items-center justify-center border-2 text-xs font-bold tracking-[0.12em] transition-colors ${
          disabled || isEmpty
            ? 'bg-[#907353] border-[#6a471f] text-[#6d5a45] cursor-not-allowed'
            : 'acme-command-btn hover:brightness-110'
        }`}
      >
        Send
      </button>
      <span className="absolute bottom-0.5 right-16 text-[10px] text-[#2e1f11]" style={{ fontFamily: 'monospace' }}>
        {value.length}/{MAX_CHARS}
      </span>
    </div>
  );
}
