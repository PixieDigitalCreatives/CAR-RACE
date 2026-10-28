import React from 'react';
import { X, Terminal, Cpu, Database, Cloud, ShieldCheck } from 'lucide-react';

interface ArchitectureModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const ArchitectureModal: React.FC<ArchitectureModalProps> = ({ isOpen, onClose }) => {
  if (!isOpen) return null;

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
        maxWidth: '860px',
        maxHeight: '90vh',
        overflowY: 'auto',
        borderRadius: 'var(--radius-lg)',
        border: '1px solid var(--border-active)',
        padding: '2.5rem',
        position: 'relative',
        background: '#070d1e'
      }}>
        {/* Close Button */}
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

        {/* Modal Header */}
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.8rem', marginBottom: '0.6rem' }}>
          <Terminal size={24} color="var(--accent-cyan)" />
          <h2 style={{ fontSize: '1.8rem', color: '#fff' }}>
            GAMEIUM WEB ARCHITECTURE & ENGINE BRIDGE
          </h2>
        </div>
        <p style={{ color: 'var(--text-sub)', fontSize: '0.9rem', marginBottom: '2rem' }}>
          Detailed technical specification for WebGL embedding, progressive asset streaming, JavaScript-to-C# IPC, and player data persistence.
        </p>

        {/* 4 Architectural Pillars */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '2rem' }}>
          {/* Pillar 1: WebGL Host & Embedding */}
          <div className="glass-panel" style={{ padding: '1.5rem', background: 'rgba(12, 20, 40, 0.6)' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem', color: 'var(--accent-cyan)', marginBottom: '0.6rem' }}>
              <Cpu size={20} />
              <h3 style={{ fontSize: '1.1rem' }}>1. WEBGL HOSTING & RUNTIME EMBEDDING</h3>
            </div>
            <p style={{ color: 'var(--text-sub)', fontSize: '0.86rem', lineHeight: 1.6, marginBottom: '0.8rem' }}>
              Unity 6 compiles C# code to WebAssembly (WASM) via IL2CPP and Emscripten. The React frontend mounts the WebGL instance within a responsive 16:9 canvas container:
            </p>
            <div style={{ background: '#03060f', padding: '1rem', borderRadius: '6px', fontFamily: 'var(--font-mono)', fontSize: '0.78rem', color: '#a5b4fc', lineHeight: 1.6 }}>
              {`// Frontend WebGL loader instantiation
createUnityInstance(canvas, {
  dataUrl: "/Builds/WebGL/Build/WebGL.data",
  frameworkUrl: "/Builds/WebGL/Build/WebGL.framework.js",
  codeUrl: "/Builds/WebGL/Build/WebGL.wasm",
  streamingAssetsUrl: "StreamingAssets",
  companyName: "GAMEIUM",
  productName: "GAMEIUM",
  productVersion: "1.0.0"
}).then(instance => {
  window.unityInstance = instance;
});`}
            </div>
          </div>

          {/* Pillar 2: Bidirectional Communication */}
          <div className="glass-panel" style={{ padding: '1.5rem', background: 'rgba(12, 20, 40, 0.6)' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem', color: 'var(--accent-gold)', marginBottom: '0.6rem' }}>
              <Cloud size={20} />
              <h3 style={{ fontSize: '1.1rem' }}>2. JAVASCRIPT &lt;--&gt; C# INTER-PROCESS COMMUNICATION</h3>
            </div>
            <p style={{ color: 'var(--text-sub)', fontSize: '0.86rem', lineHeight: 1.6, marginBottom: '0.8rem' }}>
              The web app and the Unity engine communicate seamlessly through two complementary channels:
            </p>
            <ul style={{ listStyle: 'none', display: 'flex', flexDirection: 'column', gap: '0.6rem', fontSize: '0.84rem', color: 'var(--text-sub)' }}>
              <li>
                <strong style={{ color: '#fff' }}>Web to Unity (SendMessage):</strong> React sends selected vehicle, graphics settings, or user session data via <code>window.unityInstance.SendMessage('GameManager', 'SelectVehicle', 'cheetah-gt')</code>.
              </li>
              <li>
                <strong style={{ color: '#fff' }}>Unity to Web (.jslib plugin):</strong> When a race finishes or a lap checkpoint is crossed, Unity calls native JavaScript functions exported in <code>Assets/Plugins/WebGL/GameiumBridge.jslib</code>:
              </li>
            </ul>
            <div style={{ background: '#03060f', padding: '1rem', borderRadius: '6px', fontFamily: 'var(--font-mono)', fontSize: '0.78rem', color: '#fde047', marginTop: '0.6rem' }}>
              {`mergeInto(LibraryManager.library, {
  OnLapCompleted: function(lapNumber, lapTimeMs) {
    window.dispatchEvent(new CustomEvent('GAMEIUM_LAP', { detail: { lapNumber, lapTimeMs } }));
  },
  OnRaceFinished: function(totalTimeMs, checksum) {
    window.dispatchEvent(new CustomEvent('GAMEIUM_FINISH', { detail: { totalTimeMs, checksum } }));
  }
});`}
            </div>
          </div>

          {/* Pillar 3: Progressive Streaming & Memory Management */}
          <div className="glass-panel" style={{ padding: '1.5rem', background: 'rgba(12, 20, 40, 0.6)' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem', color: 'var(--accent-lime)', marginBottom: '0.6rem' }}>
              <Database size={20} />
              <h3 style={{ fontSize: '1.1rem' }}>3. PROGRESSIVE ASSET STREAMING & 256MB HEAP TUNING</h3>
            </div>
            <p style={{ color: 'var(--text-sub)', fontSize: '0.86rem', lineHeight: 1.6 }}>
              <strong>Zero Garbage Collection Stutters:</strong> Initial WebGL memory is pinned at <strong>256MB</strong> (<code style={{ color: 'var(--accent-cyan)' }}>webGLInitialMemorySize: 256</code>) in ProjectSettings.asset. This avoids browser WebAssembly heap reallocation pauses that would cause frame drops during fast cornering.
              <br /><br />
              <strong>Static Asset Delivery:</strong> Compression format is configured to <strong>Disabled (Raw)</strong>. This allows the build output to be hosted on standard static servers (Vite dev server, Vercel, Netlify, Cloudflare Pages) without requiring server-side Brotli/Gzip Content-Encoding header configurations.
            </p>
          </div>

          {/* Pillar 4: Player Data Storage & Anti-Cheat */}
          <div className="glass-panel" style={{ padding: '1.5rem', background: 'rgba(12, 20, 40, 0.6)' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem', color: 'var(--accent-crimson)', marginBottom: '0.6rem' }}>
              <ShieldCheck size={20} />
              <h3 style={{ fontSize: '1.1rem' }}>4. DATA PERSISTENCE & LEADERBOARD ANTI-CHEAT</h3>
            </div>
            <p style={{ color: 'var(--text-sub)', fontSize: '0.86rem', lineHeight: 1.6 }}>
              <strong>Client Storage:</strong> Unity's <code>PlayerPrefs</code> in WebGL automatically maps to browser <code>IndexedDB</code> (`/idbfs/`), storing player controls, audio volume, and unlocked vehicles locally across browser sessions.
              <br /><br />
              <strong>Anti-Cheat Verification:</strong> Official leaderboard records are signed with an HMAC-SHA256 checksum generated from checkpoint delta timestamps to prevent artificial memory modification.
            </p>
          </div>
        </div>

        {/* Modal Close CTA */}
        <div style={{ textAlign: 'right', marginTop: '2rem' }}>
          <button onClick={onClose} className="btn-primary" style={{ padding: '0.75rem 1.8rem' }}>
            CLOSE ARCHITECTURE GUIDE
          </button>
        </div>
      </div>
    </div>
  );
};
