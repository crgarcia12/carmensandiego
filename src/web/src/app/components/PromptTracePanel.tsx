'use client';

import Image from 'next/image';
import { NpcMessage } from '../hooks/useNpcChat';

interface PromptTracePanelProps {
  messages: NpcMessage[];
}

export default function PromptTracePanel({ messages }: PromptTracePanelProps) {
  return (
    <div className="bg-gray-900 rounded-lg p-4 border border-gray-700 relative overflow-hidden">
      <div className="absolute -right-10 -top-10 w-24 h-24 rounded-full bg-yellow-400/15 blur-xl pulse-glow pointer-events-none" />

      <div className="flex items-center justify-between gap-3">
        <h3 className="text-sm font-bold text-yellow-400" style={{ fontFamily: 'monospace' }}>
          🧠 Prompt + Answer Log
        </h3>
        <div className="hidden xl:block w-16 h-10 rounded border border-yellow-300/40 bg-gray-950/60 p-1 float-gentle">
          <Image
            src="/detective-briefing-board.svg"
            alt="Detective board preview"
            width={64}
            height={36}
            className="w-full h-full object-cover rounded"
          />
        </div>
      </div>
      <div className="mt-3 space-y-2 max-h-[260px] overflow-y-auto pr-1">
        {messages.length === 0 ? (
          <p className="text-xs text-gray-500" style={{ fontFamily: 'monospace' }}>
            Ask a question to see prompts and answers.
          </p>
        ) : (
          messages.map((message) => (
            <div key={message.id} className="slide-fade-in rounded-md border border-gray-700 bg-gray-800/95 p-2">
              <p
                className={`text-[10px] font-bold uppercase tracking-wider ${
                  message.role === 'user' ? 'text-blue-300' : 'text-green-300'
                }`}
                style={{ fontFamily: 'monospace' }}
              >
                {message.role === 'user' ? 'Prompt' : `Answer${message.npcName ? ` (${message.npcName})` : ''}`}
              </p>
              <p className="mt-1 text-xs text-gray-100 whitespace-pre-wrap">{message.content}</p>
            </div>
          ))
        )}
      </div>
    </div>
  );
}
