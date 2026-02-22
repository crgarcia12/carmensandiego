'use client';

import Image from 'next/image';
import { NpcMessage } from '../hooks/useNpcChat';

interface PromptTracePanelProps {
  messages: NpcMessage[];
}

export default function PromptTracePanel({ messages }: PromptTracePanelProps) {
  return (
    <div className="acme-panel p-3 relative overflow-hidden min-h-[230px]">
      <div className="flex items-center justify-between gap-3">
        <h3 className="text-xs font-bold text-yellow-300 uppercase tracking-[0.2em]" style={{ fontFamily: 'monospace' }}>
          Prompt + Answer Log
        </h3>
        <div className="hidden xl:block w-16 h-10 border border-yellow-300/40 bg-black p-1">
          <Image
            src="/detective-briefing-board.svg"
            alt="Detective board preview"
            width={64}
            height={36}
            className="w-full h-full object-cover"
          />
        </div>
      </div>
      <div className="mt-3 space-y-2 max-h-[260px] overflow-y-auto pr-1">
        {messages.length === 0 ? (
          <p className="text-xs text-gray-400" style={{ fontFamily: 'monospace' }}>
            Ask a question to see prompts and answers.
          </p>
        ) : (
          messages.map((message) => (
            <div key={message.id} className="slide-fade-in acme-panel-inset p-2">
              <p
                className={`text-[10px] font-bold uppercase tracking-wider ${
                  message.role === 'user' ? 'text-cyan-300' : 'text-green-300'
                }`}
                style={{ fontFamily: 'monospace' }}
              >
                {message.role === 'user' ? 'Prompt' : `Answer${message.npcName ? ` (${message.npcName})` : ''}`}
              </p>
              <p className="mt-1 text-xs text-yellow-100 whitespace-pre-wrap">{message.content}</p>
            </div>
          ))
        )}
      </div>
    </div>
  );
}
