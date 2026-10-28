import React from 'react';
import { GitBranch } from 'lucide-react';

export const Footer: React.FC = () => {
  return (
    <footer style={{
      background: '#02040a',
      borderTop: '1px solid var(--border-subtle)',
      padding: '60px 0 30px 0',
      marginTop: '60px'
    }}>
      <div className="container">
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))',
          gap: '2.5rem',
          marginBottom: '3rem'
        }}>
          {/* Column 1: Brand & Origin */}
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '1rem' }}>
              <img 
                src="/assets/gameium-icon.png" 
                alt="GAMEIUM" 
                style={{ width: '32px', height: '32px', borderRadius: '6px' }}
              />
              <span style={{ fontFamily: 'var(--font-display)', fontWeight: 900, fontSize: '1.25rem', color: '#fff', letterSpacing: '0.06em' }}>
                GAMEIUM
              </span>
            </div>
            <p style={{ color: 'var(--text-sub)', fontSize: '0.84rem', lineHeight: 1.6, marginBottom: '1.2rem' }}>
              The high-octane African coastal night-racing game. Powered by Unity 6 Universal Render Pipeline and WebAssembly for instant in-browser competitive racing.
            </p>
            <div style={{ fontSize: '0.75rem', color: 'var(--accent-gold)', fontFamily: 'var(--font-mono)' }}>
              Design Authority: thegameio_Racing_Game.ppt
            </div>
          </div>

          {/* Column 2: Circuits & Vehicles */}
          <div>
            <h4 style={{ fontSize: '0.88rem', color: '#fff', marginBottom: '1rem', letterSpacing: '0.08em' }}>
              EXPLORE
            </h4>
            <ul style={{ listStyle: 'none', display: 'flex', flexDirection: 'column', gap: '0.5rem', fontSize: '0.82rem', color: 'var(--text-sub)' }}>
              <li><a href="#play" style={footerLinkStyle}>Route 1: Downtown Circuit (Playable)</a></li>
              <li><a href="#circuits" style={footerLinkStyle}>Route 2: Waterfront Run (In Dev)</a></li>
              <li><a href="#circuits" style={footerLinkStyle}>Route 3: Market District (In Dev)</a></li>
              <li><a href="#circuits" style={footerLinkStyle}>Route 4: Industrial Port (In Dev)</a></li>
              <li><a href="#garage" style={footerLinkStyle}>3D Vehicle Showroom (5 Chassis)</a></li>
            </ul>
          </div>

          {/* Column 3: Tech Stack & Repository */}
          <div>
            <h4 style={{ fontSize: '0.88rem', color: '#fff', marginBottom: '1rem', letterSpacing: '0.08em' }}>
              OPEN SOURCE & SPECS
            </h4>
            <ul style={{ listStyle: 'none', display: 'flex', flexDirection: 'column', gap: '0.6rem', fontSize: '0.82rem', color: 'var(--text-sub)' }}>
              <li style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <GitBranch size={14} color="var(--accent-cyan)" />
                <a 
                  href="https://github.com/PixieDigitalCreatives/CAR-RACE.git" 
                  target="_blank" 
                  rel="noreferrer" 
                  style={{ ...footerLinkStyle, color: 'var(--accent-cyan)' }}
                >
                  PixieDigitalCreatives/CAR-RACE
                </a>
              </li>
              <li>Engine: Unity 6000.6.2f1 (URP 17.6)</li>
              <li>WebGL Memory: 256 MB Initial Allocation</li>
              <li>Web Frontend: React 19 + TypeScript + Vite</li>
              <li>Audio: Native Web Audio Synthesizer</li>
            </ul>
          </div>
        </div>

        {/* Bottom Bar */}
        <div style={{
          borderTop: '1px solid rgba(255, 255, 255, 0.05)',
          paddingTop: '1.5rem',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          flexWrap: 'wrap',
          gap: '1rem',
          fontSize: '0.75rem',
          color: 'var(--text-muted)'
        }}>
          <div>
            © {new Date().getFullYear()} GAMEIUM. All rights reserved. CC0 PBR textures courtesy of ambientCG & Poly Haven.
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.35rem' }}>
            <span>Engineered for zero-install coastal speed</span>
          </div>
        </div>
      </div>
    </footer>
  );
};

const footerLinkStyle: React.CSSProperties = {
  color: 'var(--text-sub)',
  textDecoration: 'none',
  transition: 'color 0.2s ease'
};
