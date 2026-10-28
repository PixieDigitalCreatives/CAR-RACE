import React, { useState } from 'react';
import { Volume2, VolumeX, Maximize, Play, Menu, X, Terminal, Trophy, Compass } from 'lucide-react';
import { audioService } from '../services/audioSynth';

interface NavbarProps {
  onOpenArchitecture: () => void;
  onPlayClick: () => void;
}

export const Navbar: React.FC<NavbarProps> = ({ onOpenArchitecture, onPlayClick }) => {
  const [isMuted, setIsMuted] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const toggleSound = () => {
    const muted = audioService.toggleMute();
    setIsMuted(muted);
    if (!muted) audioService.playClick(600);
  };

  const handleFullscreen = () => {
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen().catch(() => {});
    } else {
      document.exitFullscreen().catch(() => {});
    }
  };

  return (
    <header style={{
      position: 'fixed',
      top: 0,
      left: 0,
      right: 0,
      zIndex: 1000,
      background: 'rgba(3, 6, 15, 0.85)',
      backdropFilter: 'blur(16px)',
      borderBottom: '1px solid var(--border-subtle)'
    }}>
      <div className="container" style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        height: '72px'
      }}>
        {/* Brand Logo & Title */}
        <a href="#" style={{ display: 'flex', alignItems: 'center', gap: '0.85rem', textDecoration: 'none' }}>
          <img 
            src="/assets/gameium-icon.png" 
            alt="GAMEIUM Icon" 
            style={{ width: '38px', height: '38px', borderRadius: '8px', boxShadow: '0 0 15px rgba(0,210,255,0.4)' }}
          />
          <div>
            <div style={{
              fontFamily: 'var(--font-display)',
              fontWeight: 900,
              fontSize: '1.4rem',
              letterSpacing: '0.08em',
              background: 'linear-gradient(135deg, #ffffff 30%, var(--accent-cyan) 100%)',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
              lineHeight: 1
            }}>
              GAMEIUM
            </div>
            <div style={{
              fontSize: '0.62rem',
              letterSpacing: '0.2em',
              color: 'var(--accent-gold)',
              fontWeight: 700,
              textTransform: 'uppercase',
              fontFamily: 'var(--font-mono)'
            }}>
              African Coastal Racing
            </div>
          </div>
        </a>

        {/* Desktop Navigation */}
        <nav style={{ display: 'flex', alignItems: 'center', gap: '1.8rem' }} className="desktop-nav">
          <a href="#play" style={navLinkStyle}>
            <Play size={15} color="var(--accent-cyan)" /> Play Online
          </a>
          <a href="#garage" style={navLinkStyle}>
            Garage (3D)
          </a>
          <a href="#circuits" style={navLinkStyle}>
            <Compass size={15} color="var(--accent-gold)" /> Circuits
          </a>
          <a href="#leaderboard" style={navLinkStyle}>
            <Trophy size={15} color="var(--accent-lime)" /> Leaderboard
          </a>
          <a href="#controls" style={navLinkStyle}>
            Controls
          </a>
          <button 
            onClick={onOpenArchitecture}
            style={{
              ...navLinkStyle,
              background: 'transparent',
              border: 'none',
              cursor: 'pointer',
              display: 'flex',
              alignItems: 'center',
              gap: '0.35rem'
            }}
          >
            <Terminal size={14} color="var(--accent-cyan)" /> Web Architecture
          </button>
        </nav>

        {/* Action Controls */}
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          <button 
            onClick={toggleSound} 
            title={isMuted ? "Unmute Engine Audio" : "Mute Audio"}
            style={iconBtnStyle}
          >
            {isMuted ? <VolumeX size={18} color="var(--accent-crimson)" /> : <Volume2 size={18} color="var(--accent-cyan)" />}
          </button>

          <button 
            onClick={handleFullscreen} 
            title="Toggle Browser Fullscreen"
            style={iconBtnStyle}
          >
            <Maximize size={18} color="var(--text-main)" />
          </button>

          <button 
            onClick={onPlayClick}
            className="btn-primary" 
            style={{ padding: '0.55rem 1.1rem', fontSize: '0.85rem' }}
          >
            <Play size={14} fill="#03060f" /> RACE NOW
          </button>

          {/* Mobile hamburger toggle */}
          <button 
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
            style={{ ...iconBtnStyle, display: 'none' }}
            className="mobile-hamburger"
          >
            {mobileMenuOpen ? <X size={20} /> : <Menu size={20} />}
          </button>
        </div>
      </div>

      {/* Mobile Drawer */}
      {mobileMenuOpen && (
        <div style={{
          background: 'var(--bg-obsidian)',
          borderBottom: '1px solid var(--border-subtle)',
          padding: '1.2rem',
          display: 'flex',
          flexDirection: 'column',
          gap: '1rem'
        }}>
          <a href="#play" onClick={() => setMobileMenuOpen(false)} style={mobileLinkStyle}>Play Online</a>
          <a href="#garage" onClick={() => setMobileMenuOpen(false)} style={mobileLinkStyle}>Garage 3D</a>
          <a href="#circuits" onClick={() => setMobileMenuOpen(false)} style={mobileLinkStyle}>Circuits</a>
          <a href="#leaderboard" onClick={() => setMobileMenuOpen(false)} style={mobileLinkStyle}>Leaderboard</a>
          <a href="#controls" onClick={() => setMobileMenuOpen(false)} style={mobileLinkStyle}>Controls Guide</a>
          <button 
            onClick={() => { setMobileMenuOpen(false); onOpenArchitecture(); }} 
            style={{ ...mobileLinkStyle, background: 'none', border: 'none', textAlign: 'left', cursor: 'pointer' }}
          >
            Web Architecture & API
          </button>
        </div>
      )}
    </header>
  );
};

const navLinkStyle: React.CSSProperties = {
  color: 'var(--text-sub)',
  textDecoration: 'none',
  fontSize: '0.88rem',
  fontWeight: 600,
  letterSpacing: '0.03em',
  display: 'flex',
  alignItems: 'center',
  gap: '0.4rem',
  transition: 'color 0.2s ease',
  fontFamily: 'var(--font-body)'
};

const iconBtnStyle: React.CSSProperties = {
  background: 'rgba(255, 255, 255, 0.05)',
  border: '1px solid var(--border-subtle)',
  borderRadius: '8px',
  width: '38px',
  height: '38px',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  cursor: 'pointer',
  transition: 'all 0.2s ease'
};

const mobileLinkStyle: React.CSSProperties = {
  color: 'var(--text-main)',
  textDecoration: 'none',
  fontSize: '1rem',
  fontWeight: 600,
  padding: '0.5rem 0'
};
