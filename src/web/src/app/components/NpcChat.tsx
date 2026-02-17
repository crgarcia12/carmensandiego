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
    <div className="chat-panel relative flex flex-col h-full bg-gray-900 rounded-lg border-2 border-gray-700 overflow-hidden">
      <div className="absolute inset-0 city-grid-overlay opacity-20 pointer-events-none" />

      <div className="px-4 py-2 border-b border-gray-700 bg-gray-800/90 rounded-t-lg relative z-10">
        <h4 className="text-sm font-bold text-yellow-400" style={{ fontFamily: 'monospace' }}>
          💬 Talking to {npcName}
        </h4>
      </div>

      <div className="relative z-10 flex-1 overflow-y-auto px-4 py-3 space-y-3 min-h-0" style={{ maxHeight: '400px' }}>
        {messages.map((msg) => (
          <div
            key={msg.id}
            className={`slide-fade-in flex items-end gap-2 ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}
          >
            {msg.role === 'npc' && (
              <div className="w-8 h-8 rounded-full bg-gradient-to-br from-amber-400 to-red-500 flex items-center justify-center text-xs font-bold text-gray-950 shadow-md">
                {msg.npcName?.slice(0, 1).toUpperCase() || 'N'}
              </div>
            )}

            <div
              className={`${msg.role === 'user' ? 'player-message' : 'npc-message'} max-w-[80%] rounded-xl px-4 py-2.5 text-sm shadow-md ${
                msg.role === 'user'
                  ? 'bg-blue-600 text-white border border-blue-400/50'
                  : 'bg-gray-700/95 text-gray-100 border border-gray-500/60'
              }`}
              data-testid={msg.role === 'user' ? 'player-message' : 'npc-message'}
            >
              {msg.role === 'npc' && msg.npcName && (
                <div className="text-xs font-bold text-yellow-400 mb-1 tracking-wide">{msg.npcName}</div>
              )}
              <p className="whitespace-pre-wrap">{msg.content}</p>
            </div>

            {msg.role === 'user' && (
              <div className="w-8 h-8 rounded-full bg-gradient-to-br from-cyan-400 to-blue-500 flex items-center justify-center text-xs font-bold text-gray-950 shadow-md">
                ACME
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
