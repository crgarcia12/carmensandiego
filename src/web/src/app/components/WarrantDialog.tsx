'use client';

import React from 'react';

interface WarrantDialogProps {
  suspectName: string;
  onConfirm: () => void;
  onCancel: () => void;
}

export default function WarrantDialog({ suspectName, onConfirm, onCancel }: WarrantDialogProps) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/70">
      <div className="acme-bezel p-2 max-w-md w-full mx-4 shadow-2xl">
        <div className="acme-panel p-5">
          <h3 className="text-sm font-bold text-yellow-300 uppercase tracking-[0.2em] mb-3" style={{ fontFamily: 'monospace' }}>
            Issue Arrest Warrant
          </h3>
          <p className="text-yellow-100 text-sm mb-6">
            Are you sure you want to issue an arrest warrant for <strong className="text-yellow-300">{suspectName}</strong>?
            This action will end your investigation.
          </p>
          <div className="flex gap-3">
            <button
              onClick={onCancel}
              className="flex-1 py-2 px-4 acme-command-btn text-sm font-semibold hover:brightness-110 transition-colors"
            >
              Cancel
            </button>
            <button
              onClick={onConfirm}
              className="flex-1 py-2 px-4 acme-command-btn text-sm font-bold uppercase hover:brightness-110 transition-colors"
              style={{ fontFamily: 'monospace' }}
            >
              Confirm
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
