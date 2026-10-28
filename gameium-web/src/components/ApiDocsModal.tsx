import React from 'react';
import { X, Code, Copy, Check } from 'lucide-react';

interface ApiDocsModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const ApiDocsModal: React.FC<ApiDocsModalProps> = ({ isOpen, onClose }) => {
  const [copiedIndex, setCopiedIndex] = React.useState<number | null>(null);

  if (!isOpen) return null;

  const copyToClipboard = (text: string, index: number) => {
    navigator.clipboard.writeText(text);
    setCopiedIndex(index);
    setTimeout(() => setCopiedIndex(null), 2000);
  };

  const getLeaderboardExample = `// GET /api/v1/leaderboard?routeId=downtown-circuit&category=all&limit=25
// Response (200 OK):
{
  "status": "success",
  "data": {
    "circuit": {
      "id": "downtown-circuit",
      "name": "Downtown Circuit",
      "recordTime": "1:18.42"
    },
    "entries": [
      {
        "rank": 1,
        "driverId": "usr_99812",
        "driverName": "KofiMensah_KE",
        "vehicleId": "apex-hypercar",
        "vehicleName": "Apex Hypercar",
        "lapTimeMs": 78420,
        "lapTimeFormatted": "1:18.42",
        "verified": true,
        "platform": "WebGL",
        "createdAt": "2026-10-28T21:14:00Z"
      }
    ]
  }
}`;

  const postScoreExample = `// POST /api/v1/scores
// Headers:
// Authorization: Bearer <player_jwt_token>
// Content-Type: application/json
// X-Gameium-Checksum: <hmac_sha256_hash>

{
  "routeId": "downtown-circuit",
  "vehicleId": "cheetah-gt",
  "lapNumber": 1,
  "lapTimeMs": 80310,
  "checkpointSplitsMs": [
    5420, 11200, 16800, 23100, 29400, 35000, 41200, 
    46800, 52100, 58400, 64200, 70100, 75500, 80310
  ],
  "topSpeedKmh": 242,
  "clientTimestamp": "2026-10-29T00:45:00Z"
}`;

  return (
    <div style={{
      position: 'fixed',
      inset: 0,
      zIndex: 2000,
      background: 'rgba(3, 6, 15, 0.85)',
      backdropFilter: 'blur(16px)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      padding: '1.5rem'
    }}>
      <div className="glass-panel" style={{
        width: '100%',
        maxWidth: '820px',
        maxHeight: '90vh',
        overflowY: 'auto',
        borderRadius: 'var(--radius-lg)',
        border: '1px solid var(--border-active)',
        padding: '2.5rem',
        position: 'relative',
        background: '#070d1e'
      }}>
        <button
          onClick={onClose}
          style={{
            position: 'absolute',
            top: '1.5rem',
            right: '1.5rem',
            background: 'rgba(255, 255, 255, 0.05)',
            border: '1px solid var(--border-subtle)',
            borderRadius: '50%',
            width: '36px',
            height: '36px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            cursor: 'pointer',
            color: '#fff'
          }}
        >
          <X size={18} />
        </button>

        <div style={{ display: 'flex', alignItems: 'center', gap: '0.8rem', marginBottom: '0.6rem' }}>
          <Code size={24} color="var(--accent-gold)" />
          <h2 style={{ fontSize: '1.8rem', color: '#fff' }}>
            GAMEIUM REST API SPECIFICATION
          </h2>
        </div>
        <p style={{ color: 'var(--text-sub)', fontSize: '0.9rem', marginBottom: '2rem' }}>
          Production schema contracts for leaderboard synchronization, time-attack verification, and driver authentication.
        </p>

        {/* Endpoint 1 */}
        <div style={{ marginBottom: '2rem' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem' }}>
              <span style={{ background: 'rgba(0, 210, 255, 0.2)', color: 'var(--accent-cyan)', padding: '0.2rem 0.6rem', borderRadius: '4px', fontSize: '0.75rem', fontFamily: 'var(--font-mono)', fontWeight: 800 }}>GET</span>
              <span style={{ fontFamily: 'var(--font-mono)', color: '#fff', fontSize: '0.95rem' }}>/api/v1/leaderboard</span>
            </div>
            <button
              onClick={() => copyToClipboard(getLeaderboardExample, 1)}
              style={{ background: 'none', border: 'none', color: 'var(--text-sub)', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '0.3rem', fontSize: '0.75rem' }}
            >
              {copiedIndex === 1 ? <Check size={14} color="var(--accent-lime)" /> : <Copy size={14} />} {copiedIndex === 1 ? 'Copied' : 'Copy'}
            </button>
          </div>
          <pre style={{ background: '#03060f', padding: '1rem', borderRadius: '6px', fontFamily: 'var(--font-mono)', fontSize: '0.78rem', color: '#cbd5e1', overflowX: 'auto' }}>
            {getLeaderboardExample}
          </pre>
        </div>

        {/* Endpoint 2 */}
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem' }}>
              <span style={{ background: 'rgba(0, 255, 136, 0.2)', color: 'var(--accent-lime)', padding: '0.2rem 0.6rem', borderRadius: '4px', fontSize: '0.75rem', fontFamily: 'var(--font-mono)', fontWeight: 800 }}>POST</span>
              <span style={{ fontFamily: 'var(--font-mono)', color: '#fff', fontSize: '0.95rem' }}>/api/v1/scores</span>
            </div>
            <button
              onClick={() => copyToClipboard(postScoreExample, 2)}
              style={{ background: 'none', border: 'none', color: 'var(--text-sub)', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '0.3rem', fontSize: '0.75rem' }}
            >
              {copiedIndex === 2 ? <Check size={14} color="var(--accent-lime)" /> : <Copy size={14} />} {copiedIndex === 2 ? 'Copied' : 'Copy'}
            </button>
          </div>
          <pre style={{ background: '#03060f', padding: '1rem', borderRadius: '6px', fontFamily: 'var(--font-mono)', fontSize: '0.78rem', color: '#cbd5e1', overflowX: 'auto' }}>
            {postScoreExample}
          </pre>
        </div>
      </div>
    </div>
  );
};
