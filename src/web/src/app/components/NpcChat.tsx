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
    <div className="flex flex-col h-full bg-gray-900 rounded-lg border-2 border-gray-700">
      <div className="px-4 py-2 border-b border-gray-700 bg-gray-800 rounded-t-lg">
        <h4 className="text-sm font-bold text-yellow-400" style={{ fontFamily: 'monospace' }}>
          💬 Talking to {npcName}
        </h4>
      </div>

      <div className="flex-1 overflow-y-auto px-4 py-3 space-y-3 min-h-0" style={{ maxHeight: '400px' }}>
        {messages.map((msg) => (
          <div key={msg.id} className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}>
            <div className={`max-w-[85%] rounded-xl px-4 py-2.5 text-sm ${
              msg.role === 'user'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-700 text-gray-100'
            }`}>
              {msg.role === 'npc' && msg.npcName && (
                <div className="text-xs font-bold text-yellow-400 mb-1">{msg.npcName}</div>
              )}
              <p className="whitespace-pre-wrap">{msg.content}</p>
            </div>
          </div>
        ))}
        <TypingIndicator visible={isLoading} />
        <div ref={bottomRef} />
      </div>

      <ChatInput onSend={onSend} disabled={isLoading} />
    </div>
  );
}
