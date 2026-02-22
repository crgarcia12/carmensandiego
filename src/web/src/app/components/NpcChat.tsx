'use client';

import React, { useRef, useEffect } from 'react';
import ChatInput from './ChatInput';
import TypingIndicator from './TypingIndicator';
import { NpcMessage } from '../hooks/useNpcChat';

interface NpcChatProps {
  npcName: string;
  messages: NpcMessage[];
  isLoading: boolean;
  onSend: (message: string) => void;
}

export default function NpcChat({ npcName, messages, isLoading, onSend }: NpcChatProps) {
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, isLoading]);

  return (
    <div className="chat-panel relative flex flex-col h-full acme-panel overflow-hidden">
      <div className="absolute inset-0 city-grid-overlay opacity-12 pointer-events-none" />

      <div className="px-4 py-2 border-b border-[#6a471f] bg-black relative z-10">
        <h4 className="text-xs font-bold text-yellow-300 uppercase tracking-[0.2em]" style={{ fontFamily: 'monospace' }}>
          Interviewing: {npcName}
        </h4>
      </div>

      <div className="relative z-10 flex-1 overflow-y-auto px-4 py-3 space-y-3 min-h-0" style={{ maxHeight: '400px' }}>
        {messages.map((msg) => (
          <div
            key={msg.id}
            className={`slide-fade-in flex items-end gap-2 ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}
          >
            {msg.role === 'npc' && (
              <div className="w-8 h-8 acme-command-btn flex items-center justify-center text-xs font-bold text-[#1c1307]">
                {msg.npcName?.slice(0, 1).toUpperCase() || 'N'}
              </div>
            )}

            <div
              className={`${msg.role === 'user' ? 'player-message' : 'npc-message'} max-w-[82%] px-3 py-2 text-sm ${
                msg.role === 'user'
                  ? 'acme-command-btn text-[#1c1307]'
                  : 'acme-panel-inset text-yellow-100'
               }`}
              data-testid={msg.role === 'user' ? 'player-message' : 'npc-message'}
            >
              {msg.role === 'npc' && msg.npcName && (
                <div className="text-[10px] font-bold text-yellow-300 mb-1 tracking-[0.1em] uppercase">{msg.npcName}</div>
              )}
              <p className="whitespace-pre-wrap">{msg.content}</p>
            </div>

            {msg.role === 'user' && (
              <div className="w-8 h-8 acme-panel-inset flex items-center justify-center text-[10px] font-bold text-cyan-300">
                YOU
              </div>
            )}
          </div>
        ))}
        <TypingIndicator visible={isLoading} />
        <div ref={bottomRef} />
      </div>

      <div className="relative z-10">
        <ChatInput onSend={onSend} disabled={isLoading} />
      </div>
    </div>
  );
}
