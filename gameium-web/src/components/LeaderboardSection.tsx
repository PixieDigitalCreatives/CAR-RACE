import React, { useState } from 'react';
import { Search, ShieldCheck, Code, Globe } from 'lucide-react';
import { INITIAL_LEADERBOARD } from '../data/gameData';
import { audioService } from '../services/audioSynth';

interface LeaderboardSectionProps {
  onOpenApiDocs: () => void;
}

export const LeaderboardSection: React.FC<LeaderboardSectionProps> = ({ onOpenApiDocs }) => {
  const [selectedRoute, setSelectedRoute] = useState('downtown-circuit');
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('ALL');

  const filteredLeaderboard = INITIAL_LEADERBOARD.filter((entry) => {
    const matchesRoute = entry.routeId === selectedRoute;
    const matchesSearch = entry.driverName.toLowerCase().includes(searchQuery.toLowerCase()) ||
                          entry.vehicle.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesCategory = selectedCategory === 'ALL' || entry.category.toLowerCase().includes(selectedCategory.toLowerCase());
    return matchesRoute && matchesSearch && matchesCategory;
  });

  return (
    <section id="leaderboard" style={{ padding: '80px 0', background: 'rgba(3, 6, 15, 0.6)' }}>
      <div className="container">
        {/* Section Header */}
        <div style={{ textAlign: 'center', marginBottom: '3.5rem' }}>
          <div style={{
            color: 'var(--accent-lime)',
            fontFamily: 'var(--font-mono)',
            fontSize: '0.85rem',
            fontWeight: 700,
            letterSpacing: '0.12em',
            textTransform: 'uppercase',
            marginBottom: '0.4rem'
          }}>
            Global Competition
          </div>
          <h2 style={{ fontSize: 'clamp(2rem, 3.5vw, 3rem)', color: '#fff' }}>
            OFFICIAL TIME ATTACK LEADERBOARD
          </h2>
          <p style={{ color: 'var(--text-sub)', maxWidth: '640px', margin: '0 auto' }}>
            Verified WebGL lap records recorded across coastal circuit sessions. Synchronized with the GAMEIUM backend API.
          </p>
        </div>

        {/* Filter Bar */}
        <div style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          flexWrap: 'wrap',
          gap: '1rem',
          marginBottom: '1.8rem'
        }}>
          {/* Circuit Tabs */}
          <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
            <button
              onClick={() => { audioService.playClick(600); setSelectedRoute('downtown-circuit'); }}
              style={{
                ...filterBtnStyle,
                borderBottom: selectedRoute === 'downtown-circuit' ? '2px solid var(--accent-cyan)' : 'none',
                color: selectedRoute === 'downtown-circuit' ? '#fff' : 'var(--text-sub)'
              }}
            >
              Route 1: Downtown Circuit
            </button>
            <button
              onClick={() => { audioService.playClick(600); setSelectedRoute('waterfront-run'); }}
              style={{
                ...filterBtnStyle,
                borderBottom: selectedRoute === 'waterfront-run' ? '2px solid var(--accent-cyan)' : 'none',
                color: selectedRoute === 'waterfront-run' ? '#fff' : 'var(--text-muted)'
              }}
            >
              Route 2: Waterfront (Upcoming)
            </button>

            {/* Category Filter */}
            <select
              value={selectedCategory}
              onChange={(e) => setSelectedCategory(e.target.value)}
              style={{
                background: 'rgba(255,255,255,0.05)',
                border: '1px solid var(--border-subtle)',
                color: 'var(--text-sub)',
                borderRadius: 'var(--radius-sm)',
                padding: '0.35rem 0.6rem',
                fontSize: '0.75rem',
                fontFamily: 'var(--font-mono)'
              }}
            >
              <option value="ALL">All Chassis Classes</option>
              <option value="Exotic Hypercar">Exotic Hypercar</option>
              <option value="Street Tuner">Street Tuner</option>
              <option value="Heavy Muscle">Heavy Muscle</option>
              <option value="Drift Spec">Drift Spec</option>
              <option value="Trophy Off-Roader">Trophy Off-Roader</option>
            </select>
          </div>

          {/* Search Input & API Docs Trigger */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.8rem' }}>
            <div style={{
              display: 'flex',
              alignItems: 'center',
              background: 'rgba(255,255,255,0.05)',
              border: '1px solid var(--border-subtle)',
              borderRadius: 'var(--radius-sm)',
              padding: '0.45rem 0.8rem',
              gap: '0.5rem'
            }}>
              <Search size={15} color="var(--text-muted)" />
              <input
                type="text"
                placeholder="Search driver or car..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                style={{
                  background: 'transparent',
                  border: 'none',
                  color: '#fff',
                  fontSize: '0.82rem',
                  outline: 'none',
                  fontFamily: 'var(--font-body)'
                }}
              />
            </div>

            <button
              onClick={() => { audioService.playClick(700); onOpenApiDocs(); }}
              className="btn-secondary"
              style={{ padding: '0.5rem 1rem', fontSize: '0.8rem' }}
            >
              <Code size={14} color="var(--accent-cyan)" /> REST API Schema
            </button>
          </div>
        </div>

        {/* Table Container */}
        <div className="glass-panel" style={{ borderRadius: 'var(--radius-md)', overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', minWidth: '680px' }}>
            <thead>
              <tr style={{
                background: 'rgba(255, 255, 255, 0.02)',
                borderBottom: '1px solid var(--border-subtle)',
                fontSize: '0.72rem',
                fontFamily: 'var(--font-mono)',
                color: 'var(--text-muted)',
                letterSpacing: '0.06em'
              }}>
                <th style={{ padding: '1rem 1.4rem' }}>RANK</th>
                <th style={{ padding: '1rem' }}>DRIVER</th>
                <th style={{ padding: '1rem' }}>VEHICLE & CLASS</th>
                <th style={{ padding: '1rem' }}>BEST LAP</th>
                <th style={{ padding: '1rem' }}>GAP</th>
                <th style={{ padding: '1rem' }}>PLATFORM</th>
                <th style={{ padding: '1rem 1.4rem', textAlign: 'right' }}>VERIFICATION</th>
              </tr>
            </thead>
            <tbody>
              {filteredLeaderboard.length > 0 ? (
                filteredLeaderboard.map((entry) => (
                  <tr
                    key={entry.rank}
                    style={{
                      borderBottom: '1px solid rgba(255,255,255,0.04)',
                      transition: 'background 0.2s ease'
                    }}
                    onMouseEnter={(e) => (e.currentTarget.style.background = 'rgba(0, 210, 255, 0.04)')}
                    onMouseLeave={(e) => (e.currentTarget.style.background = 'transparent')}
                  >
                    {/* Rank */}
                    <td style={{ padding: '1.1rem 1.4rem' }}>
                      <span style={{
                        display: 'inline-flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        width: '28px',
                        height: '28px',
                        borderRadius: '50%',
                        fontSize: '0.82rem',
                        fontWeight: 800,
                        fontFamily: 'var(--font-mono)',
                        background: entry.rank === 1 ? 'rgba(255, 170, 0, 0.2)' : entry.rank === 2 ? 'rgba(0, 210, 255, 0.2)' : 'rgba(255, 255, 255, 0.05)',
                        color: entry.rank === 1 ? 'var(--accent-gold)' : entry.rank === 2 ? 'var(--accent-cyan)' : 'var(--text-sub)'
                      }}>
                        {entry.rank}
                      </span>
                    </td>

                    {/* Driver */}
                    <td style={{ padding: '1.1rem 1rem' }}>
                      <div style={{ fontWeight: 700, color: '#fff', fontSize: '0.92rem' }}>
                        {entry.driverName}
                      </div>
                      <div style={{ fontSize: '0.7rem', color: 'var(--text-muted)' }}>
                        {entry.timestamp}
                      </div>
                    </td>

                    {/* Vehicle */}
                    <td style={{ padding: '1.1rem 1rem' }}>
                      <div style={{ color: '#fff', fontSize: '0.88rem', fontWeight: 600 }}>
                        {entry.vehicle}
                      </div>
                      <div style={{ fontSize: '0.72rem', color: 'var(--accent-cyan)', fontFamily: 'var(--font-mono)' }}>
                        {entry.category}
                      </div>
                    </td>

                    {/* Lap Time */}
                    <td style={{ padding: '1.1rem 1rem' }}>
                      <span style={{
                        fontFamily: 'var(--font-mono)',
                        fontWeight: 800,
                        fontSize: '1.1rem',
                        color: entry.rank === 1 ? 'var(--accent-gold)' : '#fff'
                      }}>
                        {entry.lapTime}
                      </span>
                    </td>

                    {/* Gap */}
                    <td style={{ padding: '1.1rem 1rem', fontFamily: 'var(--font-mono)', fontSize: '0.8rem', color: entry.gap === 'LEADER' ? 'var(--accent-gold)' : 'var(--text-sub)' }}>
                      {entry.gap}
                    </td>

                    {/* Platform */}
                    <td style={{ padding: '1.1rem 1rem' }}>
                      <span style={{
                        display: 'inline-flex',
                        alignItems: 'center',
                        gap: '0.35rem',
                        padding: '0.2rem 0.55rem',
                        borderRadius: '4px',
                        background: 'rgba(0, 210, 255, 0.08)',
                        color: 'var(--accent-cyan)',
                        fontSize: '0.72rem',
                        fontFamily: 'var(--font-mono)'
                      }}>
                        <Globe size={11} /> {entry.platform}
                      </span>
                    </td>

                    {/* Verification */}
                    <td style={{ padding: '1.1rem 1.4rem', textAlign: 'right' }}>
                      {entry.verified ? (
                        <span style={{
                          display: 'inline-flex',
                          alignItems: 'center',
                          gap: '0.3rem',
                          color: 'var(--accent-lime)',
                          fontSize: '0.75rem',
                          fontFamily: 'var(--font-mono)',
                          fontWeight: 600
                        }}>
                          <ShieldCheck size={14} /> CHECKSUM PASS
                        </span>
                      ) : (
                        <span style={{ color: 'var(--text-muted)', fontSize: '0.75rem', fontFamily: 'var(--font-mono)' }}>
                          PENDING
                        </span>
                      )}
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={7} style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-sub)' }}>
                    No drivers match the selected filter criteria.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </section>
  );
};

const filterBtnStyle: React.CSSProperties = {
  background: 'transparent',
  border: 'none',
  padding: '0.6rem 1rem',
  fontSize: '0.9rem',
  fontWeight: 700,
  fontFamily: 'var(--font-display)',
  cursor: 'pointer',
  transition: 'all 0.2s ease'
};
