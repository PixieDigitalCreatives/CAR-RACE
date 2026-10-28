import React from 'react';
import { Keyboard, Smartphone, Cpu, Zap } from 'lucide-react';

export const ControlsGuide: React.FC = () => {
  return (
    <section id="controls" style={{ padding: '80px 0' }}>
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
            Input Mapping & Hardware
          </div>
          <h2 style={{ fontSize: 'clamp(2rem, 3.5vw, 3rem)', color: '#fff' }}>
            CONTROLS & PERFORMANCE GUIDE
          </h2>
          <p style={{ color: 'var(--text-sub)', maxWidth: '640px', margin: '0 auto' }}>
            Responsive input mapping for desktop keyboards, gamepads, and touch devices. Optimized for zero-input-lag browser execution.
          </p>
        </div>

        {/* 2-Column Controls Display */}
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(360px, 1fr))',
          gap: '2rem',
          marginBottom: '3rem'
        }}>
          {/* Column 1: Desktop Keyboard & Gamepad */}
          <div className="glass-panel" style={{ padding: '2rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem', marginBottom: '1.5rem' }}>
              <Keyboard size={22} color="var(--accent-cyan)" />
              <h3 style={{ fontSize: '1.25rem', color: '#fff' }}>DESKTOP KEYBOARD CONTROLS</h3>
            </div>

            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              <KeyRow keys={['W', '▲']} action="Accelerate / Throttle" desc="Full linear power delivery to drive wheels" />
              <KeyRow keys={['S', '▼']} action="Brake / Reverse" desc="Disc braking at speed; switches to reverse gear at 0 km/h" />
              <KeyRow keys={['A', '◄']} action="Steer Left" desc="Progressive rack angle with speed-sensitive dampening" />
              <KeyRow keys={['D', '►']} action="Steer Right" desc="High slip angle response for linking sweeping curves" />
              <KeyRow keys={['SPACE']} action="Handbrake / Drift" desc="Locks rear calipers to initiate controlled power slides" />
              <KeyRow keys={['SHIFT']} action="Nitrous Oxide Boost" desc="Injects pressurized N2O for rapid 160% acceleration" />
              <KeyRow keys={['C']} action="Cycle Camera View" desc="Toggles Chase Cam, Cockpit View, and Overhead Radar" />
              <KeyRow keys={['R']} action="Reset Vehicle" desc="Instantly respawns vehicle on centerline after collisions" />
            </div>
          </div>

          {/* Column 2: Mobile Touch & Browser Optimization */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
            {/* Mobile Touch Screen Support */}
            <div className="glass-panel" style={{ padding: '2rem' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem', marginBottom: '1.2rem' }}>
                <Smartphone size={22} color="var(--accent-gold)" />
                <h3 style={{ fontSize: '1.25rem', color: '#fff' }}>MOBILE & TABLET BROWSERS</h3>
              </div>
              <p style={{ color: 'var(--text-sub)', fontSize: '0.88rem', lineHeight: 1.6, marginBottom: '1rem' }}>
                When loaded on mobile browsers (Chrome Android / Safari iOS), GAMEIUM deploys ergonomic on-screen touch pedals:
              </p>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.8rem', fontSize: '0.82rem', fontFamily: 'var(--font-mono)' }}>
                <div style={{ background: 'rgba(255,255,255,0.04)', padding: '0.8rem', borderRadius: '6px' }}>
                  <div style={{ color: 'var(--accent-cyan)', fontWeight: 700 }}>LEFT THUMB</div>
                  <div style={{ color: 'var(--text-sub)' }}>Steer Left / Right Pad</div>
                </div>
                <div style={{ background: 'rgba(255,255,255,0.04)', padding: '0.8rem', borderRadius: '6px' }}>
                  <div style={{ color: 'var(--accent-gold)', fontWeight: 700 }}>RIGHT THUMB</div>
                  <div style={{ color: 'var(--text-sub)' }}>Gas, Brake & Drift Buttons</div>
                </div>
              </div>
            </div>

            {/* Performance Optimization Tips */}
            <div className="glass-panel" style={{ padding: '2rem' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem', marginBottom: '1.2rem' }}>
                <Cpu size={22} color="var(--accent-lime)" />
                <h3 style={{ fontSize: '1.25rem', color: '#fff' }}>60 FPS OPTIMIZATION TIPS</h3>
              </div>
              <ul style={{ listStyle: 'none', display: 'flex', flexDirection: 'column', gap: '0.8rem', fontSize: '0.86rem', color: 'var(--text-sub)' }}>
                <li style={{ display: 'flex', alignItems: 'flex-start', gap: '0.5rem' }}>
                  <Zap size={15} color="var(--accent-cyan)" style={{ flexShrink: 0, marginTop: '3px' }} />
                  <span><strong>Hardware Acceleration:</strong> Ensure GPU hardware acceleration is turned ON in browser system settings.</span>
                </li>
                <li style={{ display: 'flex', alignItems: 'flex-start', gap: '0.5rem' }}>
                  <Zap size={15} color="var(--accent-gold)" style={{ flexShrink: 0, marginTop: '3px' }} />
                  <span><strong>Audio Context:</strong> Click once anywhere inside the game frame to activate the Web Audio engine without browser mute restrictions.</span>
                </li>
                <li style={{ display: 'flex', alignItems: 'flex-start', gap: '0.5rem' }}>
                  <Zap size={15} color="var(--accent-lime)" style={{ flexShrink: 0, marginTop: '3px' }} />
                  <span><strong>Full Screen Mode:</strong> Press F11 or click Fullscreen for unconstrained monitor refresh rates and lower input latency.</span>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

const KeyRow: React.FC<{ keys: string[]; action: string; desc: string }> = ({ keys, action, desc }) => (
  <div style={{
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'space-between',
    padding: '0.65rem 0.8rem',
    background: 'rgba(255, 255, 255, 0.03)',
    borderRadius: '6px',
    border: '1px solid rgba(255, 255, 255, 0.04)'
  }}>
    <div>
      <div style={{ fontSize: '0.88rem', fontWeight: 700, color: '#fff' }}>{action}</div>
      <div style={{ fontSize: '0.72rem', color: 'var(--text-muted)' }}>{desc}</div>
    </div>
    <div style={{ display: 'flex', gap: '0.35rem' }}>
      {keys.map((k, i) => (
        <span
          key={i}
          style={{
            display: 'inline-flex',
            alignItems: 'center',
            justifyContent: 'center',
            minWidth: '28px',
            height: '28px',
            padding: '0 0.45rem',
            background: 'linear-gradient(to bottom, #1e293b, #0f172a)',
            border: '1px solid rgba(0, 210, 255, 0.4)',
            borderRadius: '4px',
            fontSize: '0.75rem',
            fontFamily: 'var(--font-mono)',
            fontWeight: 800,
            color: 'var(--accent-cyan)',
            boxShadow: '0 2px 4px rgba(0,0,0,0.5)'
          }}
        >
          {k}
        </span>
      ))}
    </div>
  </div>
);
