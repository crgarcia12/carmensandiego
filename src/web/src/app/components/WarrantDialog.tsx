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
      <div className="bg-gray-800 border-2 border-yellow-400 rounded-lg p-6 max-w-md w-full mx-4 shadow-2xl">
        <h3 className="text-lg font-bold text-yellow-400 mb-3" style={{ fontFamily: 'monospace' }}>
          🚨 Issue Arrest Warrant
        </h3>
        <p className="text-gray-200 text-sm mb-6">
          Are you sure you want to issue an arrest warrant for <strong className="text-yellow-400">{suspectName}</strong>?
          This action will end your investigation.
        </p>
        <div className="flex gap-3">
          <button
            onClick={onCancel}
            className="flex-1 py-2 px-4 rounded-md text-sm font-semibold bg-gray-600 text-gray-200 hover:bg-gray-500 transition-colors"
          >
            Cancel
          </button>
          <button
            onClick={onConfirm}
            className="flex-1 py-2 px-4 rounded-md text-sm font-bold uppercase bg-red-600 text-white hover:bg-red-500 transition-colors"
            style={{ fontFamily: 'monospace' }}
          >
            Confirm Warrant
          </button>
        </div>
      </div>
    </div>
  );
}
