'use client';

import { useRouter } from 'next/navigation';
import RetroPixelHeader from './components/RetroPixelHeader';
import { useSession } from './hooks/useSession';
import { useGame } from './hooks/useGame';

export default function Home() {
  const router = useRouter();
  const { session, caseId, loading, createSession, storeCaseId } = useSession();
  const { startCase, loading: gameLoading } = useGame();

  const handleStartCase = async () => {
    let activeSession = session;
    if (!activeSession) {
      activeSession = await createSession();
    }
    if (!activeSession) return;

    const newCase = await startCase(activeSession.id);
    if (newCase) {
      storeCaseId(newCase.id);
      router.push('/briefing');
    }
  };

  const handleResume = () => {
    router.push('/city');
  };

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-950">
        <p className="text-yellow-400 animate-pulse" style={{ fontFamily: 'monospace' }}>Loading...</p>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-gray-950 px-4">
      <RetroPixelHeader title="Carmen Sandiego" subtitle="Web Edition" />

      <div className="mt-8 space-y-4 w-full max-w-sm">
        <button
          onClick={handleStartCase}
          disabled={gameLoading}
          className="w-full py-3 px-6 rounded-lg text-sm font-bold uppercase tracking-wider bg-yellow-400 text-gray-900 hover:bg-yellow-300 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          style={{ fontFamily: 'monospace' }}
        >
          {gameLoading ? '⏳ Creating Case...' : '🔍 Start New Case'}
        </button>

        {caseId && (
          <button
            onClick={handleResume}
            className="w-full py-3 px-6 rounded-lg text-sm font-bold uppercase tracking-wider bg-gray-700 text-yellow-400 border-2 border-yellow-400 hover:bg-gray-600 transition-colors"
            style={{ fontFamily: 'monospace' }}
          >
            ▶️ Resume Investigation
          </button>
        )}
      </div>

      <p className="mt-12 text-xs text-gray-500 text-center" style={{ fontFamily: 'monospace' }}>
        Track stolen treasures across the globe.<br />
        Interview witnesses. Follow the clues. Catch the thief.
      </p>
    </div>
  );
}
