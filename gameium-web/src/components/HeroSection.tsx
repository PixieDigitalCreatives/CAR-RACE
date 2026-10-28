import React from 'react';
import { Play, Flame, Gauge, Zap, Sparkles, Layers } from 'lucide-react';
import { audioService } from '../services/audioSynth';

interface HeroSectionProps {
  onPlayClick: () => void;
  onExploreGarage: () => void;
}

export const HeroSection: React.FC<HeroSectionProps> = ({ onPlayClick, onExploreGarage }) => {
  return (
    <section style={{
      position: 'relative',
      minHeight: '92vh',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      paddingTop: '100px',
      paddingBottom: '60px',
      overflow: 'hidden'
    }}>
      {/* Background Graphic Overlay */}
      <div style={{
        position: 'absolute',
        inset: 0,
        backgroundImage: 'radial-gradient(circle at center, rgba(3, 6, 15, 0.4) 0%, rgba(3, 6, 15, 0.95) 100%), url("/assets/hero-bg.jpg")',
        backgroundSize: 'cover',
        backgroundPosition: 'center 40%',
        opacity: 0.35,
        zIndex: 0,
        filter: 'contrast(1.15) saturate(1.2)'
      }} />

      {/* Speed lines & Grid effect */}
      <div className="carbon-grid" style={{
        position: 'absolute',
        inset: 0,
        opacity: 0.4,
        zIndex: 1,
        pointerEvents: 'none'
      }} />

      {/* Radiant Glow Orb */}
      <div style={{
        position: 'absolute',
        top: '25%',
        left: '50%',
        transform: 'translate(-50%, -50%)',
        width: '600px',
        height: '350px',
        background: 'radial-gradient(circle, rgba(0, 210, 255, 0.18) 0%, rgba(255, 170, 0, 0.08) 50%, transparent 70%)',
        filter: 'blur(50px)',
        zIndex: 1,
        pointerEvents: 'none'
      }} />

      <div className="container" style={{ position: 'relative', zIndex: 10, textAlign: 'center' }}>
        {/* Live Engine Status Badge */}
        <div style={{
          display: 'inline-flex',
          alignItems: 'center',
          gap: '0.6rem',
          background: 'rgba(0, 210, 255, 0.08)',
          border: '1px solid rgba(0, 210, 255, 0.3)',
          borderRadius: 'var(--radius-full)',
          padding: '0.35rem 1.1rem',
          marginBottom: '1.8rem',
          boxShadow: '0 0 15px rgba(0, 210, 255, 0.15)'
        }}>
          <span style={{
            width: '8px',
            height: '8px',
            borderRadius: '50%',
            backgroundColor: 'var(--accent-lime)',
            boxShadow: '0 0 8px var(--accent-lime)',
            display: 'inline-block'
          }} />
          <span style={{
            fontFamily: 'var(--font-mono)',
            fontSize: '0.78rem',
            color: 'var(--accent-cyan)',
            letterSpacing: '0.08em',
            textTransform: 'uppercase',
            fontWeight: 600
          }}>
            Unity 6 URP • WebGL 2.0 Browser Target • 60 FPS Engine
          </span>
        </div>

        {/* Main Title Badge & Brand Heading */}
        <div style={{ marginBottom: '1.2rem' }}>
          <h1 style={{
            fontSize: 'clamp(2.8rem, 7vw, 5.8rem)',
            fontWeight: 900,
            lineHeight: 0.95,
            letterSpacing: '0.05em',
            textTransform: 'uppercase',
            margin: '0 auto',
            background: 'linear-gradient(180deg, #ffffff 20%, #b2d5ea 60%, #00d2ff 100%)',
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
            filter: 'drop-shadow(0 0 35px rgba(0, 210, 255, 0.35))'
          }}>
            GAMEIUM
          </h1>
          <div style={{
            fontFamily: 'var(--font-display)',
            fontSize: 'clamp(1rem, 2.4vw, 1.6rem)',
            fontWeight: 700,
            letterSpacing: '0.35em',
            color: 'var(--accent-gold)',
            marginTop: '0.4rem',
            textShadow: '0 0 20px rgba(255, 170, 0, 0.5)'
          }}>
            COASTAL NIGHT RACING
          </div>
        </div>

        {/* Cinematic Subtitle */}
        <p style={{
          maxWidth: '740px',
          margin: '0 auto 2.4rem auto',
          fontSize: 'clamp(1rem, 1.25vw, 1.2rem)',
          color: 'var(--text-sub)',
          lineHeight: 1.6,
          fontWeight: 400
        }}>
          Tear through the illuminated streets of the African coastal metropolis. 
          Master high-grip hairpins under neon skyscrapers, challenge dual directional AI opponents, 
          and push 5 distinct vehicle classes to their absolute limit—directly in your browser.
        </p>

        {/* Call to Actions */}
        <div style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          gap: '1.2rem',
          flexWrap: 'wrap',
          marginBottom: '3.5rem'
        }}>
          <button 
            onClick={() => {
              audioService.playClick(750);
              onPlayClick();
            }}
            className="btn-primary" 
            style={{ fontSize: '1.05rem', padding: '1rem 2.4rem' }}
          >
            <Play size={18} fill="#03060f" /> PLAY IN BROWSER
          </button>

          <button 
            onClick={() => {
              audioService.playClick(520);
              onExploreGarage();
            }}
            className="btn-secondary" 
            style={{ fontSize: '1.05rem', padding: '1rem 2.2rem' }}
          >
            <Layers size={18} color="var(--accent-cyan)" /> 3D GARAGE SHOWROOM
          </button>
        </div>

        {/* 4 Feature Spec Badges */}
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))',
          gap: '1.2rem',
          maxWidth: '1080px',
          margin: '0 auto'
        }}>
          <div className="glass-panel" style={{ padding: '1.2rem', textAlign: 'left' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.5rem' }}>
              <Zap size={20} color="var(--accent-cyan)" />
              <div style={{ fontFamily: 'var(--font-display)', fontWeight: 700, fontSize: '0.95rem' }}>
                ZERO INSTALL
              </div>
            </div>
            <div style={{ color: 'var(--text-sub)', fontSize: '0.82rem', lineHeight: 1.4 }}>
              WebAssembly execution with instant local memory caching. No launchers or APK downloads required.
            </div>
          </div>

          <div className="glass-panel" style={{ padding: '1.2rem', textAlign: 'left' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.5rem' }}>
              <Gauge size={20} color="var(--accent-gold)" />
              <div style={{ fontFamily: 'var(--font-display)', fontWeight: 700, fontSize: '0.95rem' }}>
                ARCADE PHYSICS
              </div>
            </div>
            <div style={{ color: 'var(--text-sub)', fontSize: '0.82rem', lineHeight: 1.4 }}>
              Wheel friction models, counter-steer drift slide, nitrous burst, and high-speed downforce dynamics.
            </div>
          </div>

          <div className="glass-panel" style={{ padding: '1.2rem', textAlign: 'left' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.5rem' }}>
              <Flame size={20} color="var(--accent-crimson)" />
              <div style={{ fontFamily: 'var(--font-display)', fontWeight: 700, fontSize: '0.95rem' }}>
                5 VEHICLE ROLES
              </div>
            </div>
            <div style={{ color: 'var(--text-sub)', fontSize: '0.82rem', lineHeight: 1.4 }}>
              Cheetah GT, Apex Hypercar, Marauder Muscle, Drift King, and Dune Raider with bespoke specs.
            </div>
          </div>

          <div className="glass-panel" style={{ padding: '1.2rem', textAlign: 'left' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.5rem' }}>
              <Sparkles size={20} color="var(--accent-lime)" />
              <div style={{ fontFamily: 'var(--font-display)', fontWeight: 700, fontSize: '0.95rem' }}>
                AFRICAN NIGHTS
              </div>
            </div>
            <div style={{ color: 'var(--text-sub)', fontSize: '0.82rem', lineHeight: 1.4 }}>
              Authentic coastal metropolis art direction with neon signage, ocean mist, and cultural landmarks.
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};
