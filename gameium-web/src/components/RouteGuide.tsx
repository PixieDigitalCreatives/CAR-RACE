import React, { useState } from 'react';
import { Compass, CheckCircle2, MapPin, AlertCircle, Sparkles } from 'lucide-react';
import { CIRCUITS } from '../data/gameData';
import type { RouteData } from '../data/gameData';
import { audioService } from '../services/audioSynth';

interface RouteGuideProps {
  onPlayRoute: (route: RouteData) => void;
}

export const RouteGuide: React.FC<RouteGuideProps> = ({ onPlayRoute }) => {
  const [selectedCircuit, setSelectedCircuit] = useState<RouteData>(CIRCUITS[0]);

  return (
    <section id="circuits" style={{ padding: '80px 0', background: 'rgba(5, 9, 20, 0.4)' }}>
      <div className="container">
        {/* Section Header */}
        <div style={{ textAlign: 'center', marginBottom: '3.5rem' }}>
          <div style={{
            color: 'var(--accent-cyan)',
            fontFamily: 'var(--font-mono)',
            fontSize: '0.85rem',
            fontWeight: 700,
            letterSpacing: '0.12em',
            textTransform: 'uppercase',
            marginBottom: '0.4rem'
          }}>
            Championship Circuits
          </div>
          <h2 style={{ fontSize: 'clamp(2rem, 3.5vw, 3rem)', color: '#fff' }}>
            FOUR AFRICAN COASTAL ROUTES
          </h2>
          <p style={{ color: 'var(--text-sub)', maxWidth: '640px', margin: '0 auto' }}>
            Tour the night-lit boulevards, busy market lanes, and sea breeze straights of our metropolis. Route 1 is fully playable in the current prototype.
          </p>
        </div>

        {/* 4 Circuit Cards Grid */}
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(270px, 1fr))',
          gap: '1.5rem',
          marginBottom: '3rem'
        }}>
          {CIRCUITS.map((c) => {
            const isSelected = c.id === selectedCircuit.id;
            const isPlayable = c.status === 'PLAYABLE';

            return (
              <div
                key={c.id}
                onClick={() => {
                  audioService.playClick(650);
                  setSelectedCircuit(c);
                }}
                className="glass-panel"
                style={{
                  position: 'relative',
                  overflow: 'hidden',
                  cursor: 'pointer',
                  borderRadius: 'var(--radius-md)',
                  border: isSelected ? '2px solid var(--accent-cyan)' : '1px solid var(--border-subtle)',
                  background: isSelected ? 'rgba(15, 25, 48, 0.95)' : 'var(--bg-card)',
                  transform: isSelected ? 'translateY(-4px)' : 'none',
                  boxShadow: isSelected ? '0 12px 30px rgba(0, 210, 255, 0.25)' : 'none'
                }}
              >
                {/* Image Banner */}
                <div style={{ position: 'relative', height: '160px', overflow: 'hidden' }}>
                  <img
                    src={c.cardImage}
                    alt={c.name}
                    style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                    onError={(e) => {
                      (e.target as HTMLImageElement).src = '/assets/hero-bg.jpg';
                    }}
                  />
                  <div style={{
                    position: 'absolute',
                    inset: 0,
                    background: 'linear-gradient(to top, rgba(3, 6, 15, 0.95) 0%, transparent 70%)'
                  }} />

                  {/* Status Badge */}
                  <div style={{
                    position: 'absolute',
                    top: '0.8rem',
                    right: '0.8rem',
                    display: 'inline-flex',
                    alignItems: 'center',
                    gap: '0.35rem',
                    padding: '0.25rem 0.65rem',
                    borderRadius: 'var(--radius-full)',
                    fontSize: '0.68rem',
                    fontWeight: 700,
                    fontFamily: 'var(--font-mono)',
                    background: isPlayable ? 'rgba(0, 255, 136, 0.2)' : 'rgba(255, 170, 0, 0.2)',
                    color: isPlayable ? 'var(--accent-lime)' : 'var(--accent-gold)',
                    border: `1px solid ${isPlayable ? 'var(--accent-lime)' : 'var(--accent-gold)'}`
                  }}>
                    {isPlayable ? <CheckCircle2 size={12} /> : <AlertCircle size={12} />}
                    {isPlayable ? 'PLAYABLE' : 'IN DEV'}
                  </div>

                  {/* Route Number */}
                  <div style={{
                    position: 'absolute',
                    bottom: '0.8rem',
                    left: '0.8rem',
                    fontSize: '0.75rem',
                    fontFamily: 'var(--font-mono)',
                    color: 'var(--accent-cyan)',
                    fontWeight: 700
                  }}>
                    ROUTE 0{c.routeNumber}
                  </div>
                </div>

                {/* Content */}
                <div style={{ padding: '1.2rem' }}>
                  <h3 style={{ fontSize: '1.25rem', color: '#fff', marginBottom: '0.3rem' }}>
                    {c.name}
                  </h3>
                  <div style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: '0.8rem',
                    fontSize: '0.75rem',
                    color: 'var(--text-sub)',
                    fontFamily: 'var(--font-mono)',
                    marginBottom: '0.8rem'
                  }}>
                    <span>{c.lengthKm} KM</span>
                    <span>•</span>
                    <span>{c.laps} LAPS</span>
                    <span>•</span>
                    <span style={{ color: getDifficultyColor(c.difficulty) }}>{c.difficulty.toUpperCase()}</span>
                  </div>

                  <p style={{
                    fontSize: '0.82rem',
                    color: 'var(--text-muted)',
                    lineHeight: 1.5,
                    display: '-webkit-box',
                    WebkitLineClamp: 2,
                    WebkitBoxOrient: 'vertical',
                    overflow: 'hidden'
                  }}>
                    {c.description}
                  </p>
                </div>
              </div>
            );
          })}
        </div>

        {/* Selected Circuit Deep Dive */}
        <div className="glass-panel" style={{
          padding: '2rem',
          borderRadius: 'var(--radius-lg)',
          border: '1px solid var(--border-active)'
        }}>
          <div style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))',
            gap: '2.5rem',
            alignItems: 'center'
          }}>
            {/* Left Column: Circuit Overview & Features */}
            <div>
              <div style={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: '0.5rem',
                fontSize: '0.78rem',
                color: 'var(--accent-cyan)',
                fontFamily: 'var(--font-mono)',
                fontWeight: 700,
                marginBottom: '0.5rem'
              }}>
                <MapPin size={14} /> ROUTE 0{selectedCircuit.routeNumber} DOSSIER
              </div>
              <h3 style={{ fontSize: '2.2rem', color: '#fff', marginBottom: '0.8rem' }}>
                {selectedCircuit.name}
              </h3>
              <p style={{ color: 'var(--text-sub)', lineHeight: 1.6, marginBottom: '1.5rem', fontSize: '0.95rem' }}>
                {selectedCircuit.description}
              </p>

              {/* Highlights */}
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.6rem', marginBottom: '1.8rem' }}>
                {selectedCircuit.keyFeatures.map((feat, i) => (
                  <div key={i} style={{ display: 'flex', alignItems: 'center', gap: '0.6rem', fontSize: '0.85rem', color: 'var(--text-main)' }}>
                    <Sparkles size={14} color="var(--accent-gold)" />
                    {feat}
                  </div>
                ))}
              </div>

              {/* Action Button */}
              {selectedCircuit.status === 'PLAYABLE' ? (
                <button
                  onClick={() => {
                    audioService.playClick(880);
                    onPlayRoute(selectedCircuit);
                  }}
                  className="btn-primary"
                  style={{ padding: '0.9rem 2.2rem' }}
                >
                  <Compass size={18} fill="#03060f" /> LOAD IN BROWSER PLAYER
                </button>
              ) : (
                <div style={{
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: '0.6rem',
                  padding: '0.75rem 1.4rem',
                  background: 'rgba(255, 170, 0, 0.1)',
                  border: '1px solid var(--accent-gold)',
                  borderRadius: 'var(--radius-sm)',
                  color: 'var(--accent-gold)',
                  fontFamily: 'var(--font-mono)',
                  fontSize: '0.82rem'
                }}>
                  <AlertCircle size={16} /> Circuit In Active Modeling • Scheduled for Phase 2
                </div>
              )}
            </div>

            {/* Right Column: Track Stats & Lap Record */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1.2rem' }}>
              <div className="glass-panel" style={{ padding: '1.2rem', background: 'rgba(10, 18, 38, 0.8)' }}>
                <div style={{ fontSize: '0.72rem', color: 'var(--text-muted)', fontFamily: 'var(--font-mono)' }}>TRACK RECORD</div>
                <div style={{ display: 'flex', alignItems: 'baseline', gap: '0.8rem', margin: '0.3rem 0' }}>
                  <span style={{ fontSize: '1.8rem', fontWeight: 800, color: 'var(--accent-gold)', fontFamily: 'var(--font-mono)' }}>
                    {selectedCircuit.trackRecord.time}
                  </span>
                  <span style={{ fontSize: '0.85rem', color: '#fff' }}>
                    by {selectedCircuit.trackRecord.holder}
                  </span>
                </div>
                <div style={{ fontSize: '0.75rem', color: 'var(--text-sub)' }}>
                  Vehicle: {selectedCircuit.trackRecord.vehicle}
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                <div className="glass-panel" style={{ padding: '1rem' }}>
                  <div style={{ fontSize: '0.7rem', color: 'var(--text-muted)', fontFamily: 'var(--font-mono)' }}>SURFACE</div>
                  <div style={{ fontSize: '0.88rem', fontWeight: 700, color: '#fff', marginTop: '0.2rem' }}>
                    {selectedCircuit.surface}
                  </div>
                </div>

                <div className="glass-panel" style={{ padding: '1rem' }}>
                  <div style={{ fontSize: '0.7rem', color: 'var(--text-muted)', fontFamily: 'var(--font-mono)' }}>TIME OF DAY</div>
                  <div style={{ fontSize: '0.88rem', fontWeight: 700, color: '#fff', marginTop: '0.2rem' }}>
                    {selectedCircuit.timeOfDay}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

const getDifficultyColor = (diff: string) => {
  switch (diff.toLowerCase()) {
    case 'easy': return 'var(--accent-lime)';
    case 'medium': return 'var(--accent-cyan)';
    case 'hard': return 'var(--accent-gold)';
    case 'extreme': return 'var(--accent-crimson)';
    default: return 'var(--accent-cyan)';
  }
};
