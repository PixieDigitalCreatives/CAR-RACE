import React, { useState, useEffect, useRef } from 'react';
import { Play, RotateCcw, Flame, Flag, Activity, Monitor } from 'lucide-react';
import confetti from 'canvas-confetti';
import { audioService } from '../services/audioSynth';
import { VEHICLES } from '../data/gameData';
import type { VehicleData } from '../data/gameData';

interface WebPlayerSectionProps {
  selectedVehicle?: VehicleData;
}

type ViewMode = 'ARCADE_SIMULATOR' | 'UNITY_WEBGL' | 'TELEMETRY';

export const WebPlayerSection: React.FC<WebPlayerSectionProps> = ({ selectedVehicle = VEHICLES[0] }) => {
  const [viewMode, setViewMode] = useState<ViewMode>('ARCADE_SIMULATOR');
  const [isPlaying, setIsPlaying] = useState(false);
  const [speedKmh, setSpeedKmh] = useState(0);
  const [rpm, setRpm] = useState(1000);
  const [gear, setGear] = useState('N');
  const [currentLap, setCurrentLap] = useState(1);
  const [totalLaps] = useState(3);
  const [lapTimeMs, setLapTimeMs] = useState(0);
  const [bestLapTimeMs, setBestLapTimeMs] = useState<number | null>(null);
  const [checkpointIndex, setCheckpointIndex] = useState(0);
  const [totalCheckpoints] = useState(14);
  const [fps, setFps] = useState(60);
  const [cameraMode, setCameraMode] = useState<'CHASE' | 'COCKPIT' | 'TOP'>('CHASE');
  const [isDrifting, setIsDrifting] = useState(false);
  const [nitroAmount, setNitroAmount] = useState(100);
  const [isNitroActive, setIsNitroActive] = useState(false);
  const [unityBuildFound, setUnityBuildFound] = useState(false);

  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const containerRef = useRef<HTMLDivElement | null>(null);

  // Car Physics Simulation State
  const carState = useRef({
    x: 0,
    y: 0,
    angle: 0,
    speed: 0,
    maxSpeed: selectedVehicle.specs.topSpeedKmh / 3.6 * 0.45, // scaled for canvas
    accel: 25,
    brake: 45,
    turnSpeed: 2.8,
    driftFriction: 0.94,
    skidmarks: [] as { x: number; y: number; alpha: number }[],
    keys: {
      w: false,
      s: false,
      a: false,
      d: false,
      space: false,
      shift: false
    }
  });

  // Track Checkpoints along Downtown Circuit
  const checkpoints = useRef([
    { x: 0, y: -200, name: 'Gate A Finish Line' },
    { x: 350, y: -200, name: 'Harbor Turn 1' },
    { x: 600, y: -50, name: 'Coast Boulevard Sweeper' },
    { x: 650, y: 300, name: 'Skyscraper Underpass' },
    { x: 450, y: 550, name: 'Metropolis Hairpin East' },
    { x: 100, y: 580, name: 'Avenue Straightaway' },
    { x: -250, y: 550, name: 'Central Plaza Chicane' },
    { x: -500, y: 400, name: 'Market District Arch' },
    { x: -600, y: 150, name: 'Neon Alleyway' },
    { x: -650, y: -100, name: 'Overpass S-Bend' },
    { x: -500, y: -350, name: 'Port Highway Entry' },
    { x: -250, y: -450, name: 'Ocean View Straight' },
    { x: -100, y: -350, name: 'Final Chicane Entry' },
    { x: -20, y: -260, name: 'Championship Straight' }
  ]);

  // Update vehicle specs when car changes
  useEffect(() => {
    carState.current.maxSpeed = selectedVehicle.specs.topSpeedKmh / 3.6 * 0.45;
  }, [selectedVehicle]);

  // Check if Unity WebGL build exists
  useEffect(() => {
    fetch('/Builds/WebGL/index.html')
      .then(res => {
        if (res.ok) setUnityBuildFound(true);
      })
      .catch(() => setUnityBuildFound(false));
  }, []);

  // Keyboard Event Handlers
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      const k = e.key.toLowerCase();
      if (['arrowup', 'w'].includes(k)) carState.current.keys.w = true;
      if (['arrowdown', 's'].includes(k)) carState.current.keys.s = true;
      if (['arrowleft', 'a'].includes(k)) carState.current.keys.a = true;
      if (['arrowright', 'd'].includes(k)) carState.current.keys.d = true;
      if (e.code === 'Space') {
        carState.current.keys.space = true;
        e.preventDefault();
      }
      if (e.key === 'Shift') carState.current.keys.shift = true;
      if (k === 'c') cycleCamera();
      if (k === 'r') resetCar();
    };

    const handleKeyUp = (e: KeyboardEvent) => {
      const k = e.key.toLowerCase();
      if (['arrowup', 'w'].includes(k)) carState.current.keys.w = false;
      if (['arrowdown', 's'].includes(k)) carState.current.keys.s = false;
      if (['arrowleft', 'a'].includes(k)) carState.current.keys.a = false;
      if (['arrowright', 'd'].includes(k)) carState.current.keys.d = false;
      if (e.code === 'Space') carState.current.keys.space = false;
      if (e.key === 'Shift') carState.current.keys.shift = false;
    };

    window.addEventListener('keydown', handleKeyDown);
    window.addEventListener('keyup', handleKeyUp);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
      window.removeEventListener('keyup', handleKeyUp);
    };
  }, []);

  // Canvas Physics & Render Loop
  useEffect(() => {
    if (!isPlaying || viewMode !== 'ARCADE_SIMULATOR') {
      audioService.stopArcadeEngine();
      return;
    }

    let animationFrameId: number;
    let lastTime = performance.now();
    let frameCount = 0;
    let fpsTimer = performance.now();

    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    // Start engine sound
    audioService.setArcadeEngineRPM(0.1);

    const loop = (currentTime: number) => {
      const dt = Math.min((currentTime - lastTime) / 1000, 0.1);
      lastTime = currentTime;

      // FPS tracking
      frameCount++;
      if (currentTime - fpsTimer >= 500) {
        setFps(Math.round((frameCount * 1000) / (currentTime - fpsTimer)));
        frameCount = 0;
        fpsTimer = currentTime;
      }

      // Physics Simulation
      const state = carState.current;
      const keys = state.keys;

      // Nitro handling
      let accelMultiplier = 1.0;
      if (keys.shift && nitroAmount > 5) {
        accelMultiplier = 1.6;
        setNitroAmount(prev => Math.max(0, prev - dt * 30));
        setIsNitroActive(true);
      } else {
        setIsNitroActive(false);
        setNitroAmount(prev => Math.min(100, prev + dt * 6));
      }

      // Acceleration / Braking
      if (keys.w) {
        state.speed = Math.min(state.maxSpeed * (isNitroActive ? 1.25 : 1.0), state.speed + state.accel * accelMultiplier * dt);
      } else if (keys.s) {
        if (state.speed > 2) {
          state.speed = Math.max(0, state.speed - state.brake * dt);
        } else {
          state.speed = Math.max(-10, state.speed - state.accel * 0.5 * dt); // reverse
        }
      } else {
        // Natural friction decay
        state.speed *= Math.pow(0.97, dt * 60);
        if (Math.abs(state.speed) < 0.2) state.speed = 0;
      }

      // Steering with drift dynamics
      const driftActive = keys.space && Math.abs(state.speed) > 5;
      setIsDrifting(driftActive);

      let steerRate = state.turnSpeed;
      if (driftActive) {
        steerRate *= 1.45;
        state.speed *= 0.985; // drift friction
        if (Math.random() < 0.3) audioService.playTireScreech();

        // Add skidmarks
        state.skidmarks.push({
          x: state.x,
          y: state.y,
          alpha: 0.8
        });
      }

      const turnDirection = (keys.a ? -1 : 0) + (keys.d ? 1 : 0);
      if (state.speed !== 0) {
        state.angle += turnDirection * steerRate * (state.speed / state.maxSpeed) * dt;
      }

      // Move car
      state.x += Math.cos(state.angle) * state.speed * 60 * dt;
      state.y += Math.sin(state.angle) * state.speed * 60 * dt;

      // Fade skidmarks
      for (let i = state.skidmarks.length - 1; i >= 0; i--) {
        state.skidmarks[i].alpha -= dt * 0.4;
        if (state.skidmarks[i].alpha <= 0) state.skidmarks.splice(i, 1);
      }

      // Checkpoint detection
      const currentCp = checkpoints.current[checkpointIndex];
      const distToCp = Math.hypot(state.x - currentCp.x, state.y - currentCp.y);
      if (distToCp < 90) {
        audioService.playCheckpoint();
        const nextIndex = (checkpointIndex + 1) % checkpoints.current.length;
        setCheckpointIndex(nextIndex);

        // Completed a full lap
        if (nextIndex === 0) {
          if (!bestLapTimeMs || lapTimeMs < bestLapTimeMs) {
            setBestLapTimeMs(lapTimeMs);
          }
          if (currentLap < totalLaps) {
            setCurrentLap(prev => prev + 1);
            setLapTimeMs(0);
          } else {
            // Race finished!
            confetti({
              particleCount: 80,
              spread: 70,
              origin: { y: 0.6 }
            });
          }
        }
      }

      // Update HUD metrics
      const currentKmh = Math.round(Math.abs(state.speed) / (state.maxSpeed / selectedVehicle.specs.topSpeedKmh));
      setSpeedKmh(currentKmh);

      // Compute Gear & RPM
      let calculatedGear = 'N';
      let normalizedRpm = 0.1;
      if (state.speed < 0) {
        calculatedGear = 'R';
        normalizedRpm = Math.min(1.0, Math.abs(state.speed) / 10);
      } else if (currentKmh < 45) {
        calculatedGear = '1';
        normalizedRpm = currentKmh / 45;
      } else if (currentKmh < 85) {
        calculatedGear = '2';
        normalizedRpm = (currentKmh - 40) / 45;
      } else if (currentKmh < 135) {
        calculatedGear = '3';
        normalizedRpm = (currentKmh - 80) / 55;
      } else if (currentKmh < 185) {
        calculatedGear = '4';
        normalizedRpm = (currentKmh - 130) / 55;
      } else if (currentKmh < 240) {
        calculatedGear = '5';
        normalizedRpm = (currentKmh - 180) / 60;
      } else {
        calculatedGear = '6';
        normalizedRpm = Math.min(1.0, (currentKmh - 235) / 100);
      }

      setGear(calculatedGear);
      const computedRpmVal = Math.round(1000 + normalizedRpm * 7500);
      setRpm(computedRpmVal);
      audioService.setArcadeEngineRPM(normalizedRpm);

      // Lap Timer update
      setLapTimeMs(prev => prev + Math.round(dt * 1000));

      // RENDER GRAPHICS
      renderCanvas(ctx, canvas.width, canvas.height, state);

      animationFrameId = requestAnimationFrame(loop);
    };

    animationFrameId = requestAnimationFrame(loop);
    return () => {
      cancelAnimationFrame(animationFrameId);
      audioService.stopArcadeEngine();
    };
  }, [isPlaying, viewMode, checkpointIndex, currentLap, selectedVehicle, isNitroActive, nitroAmount, cameraMode]);

  // Render 2.5D / Canvas World
  const renderCanvas = (
    ctx: CanvasRenderingContext2D,
    width: number,
    height: number,
    state: typeof carState.current
  ) => {
    ctx.clearRect(0, 0, width, height);

    // Camera view setup
    ctx.save();
    const centerX = width / 2;
    const centerY = height / 2;

    if (cameraMode === 'CHASE') {
      // Camera follows behind car with rotation
      ctx.translate(centerX, centerY + 80);
      ctx.rotate(-state.angle - Math.PI / 2);
      ctx.translate(-state.x, -state.y);
    } else if (cameraMode === 'COCKPIT') {
      ctx.translate(centerX, centerY + 140);
      ctx.rotate(-state.angle - Math.PI / 2);
      ctx.translate(-state.x, -state.y);
    } else {
      // Top-down overhead map
      ctx.translate(centerX, centerY);
      ctx.scale(0.55, 0.55);
      ctx.translate(-state.x, -state.y);
    }

    // 1. Draw Night Asphalt Surface & Circuit Boundary
    drawCircuitTrack(ctx);

    // 2. Draw Skidmarks
    ctx.lineWidth = 6;
    for (const sm of state.skidmarks) {
      ctx.fillStyle = `rgba(15, 15, 20, ${sm.alpha * 0.7})`;
      ctx.fillRect(sm.x - 3, sm.y - 3, 6, 6);
    }

    // 3. Draw Checkpoints
    checkpoints.current.forEach((cp, idx) => {
      const isTarget = idx === checkpointIndex;
      ctx.beginPath();
      ctx.arc(cp.x, cp.y, isTarget ? 65 : 40, 0, Math.PI * 2);
      ctx.fillStyle = isTarget ? 'rgba(0, 210, 255, 0.18)' : 'rgba(255, 170, 0, 0.05)';
      ctx.fill();
      ctx.lineWidth = isTarget ? 4 : 2;
      ctx.strokeStyle = isTarget ? 'var(--accent-cyan)' : 'rgba(255, 170, 0, 0.3)';
      ctx.stroke();

      if (isTarget) {
        ctx.fillStyle = '#00d2ff';
        ctx.font = 'bold 12px Orbitron';
        ctx.textAlign = 'center';
        ctx.fillText(`GATE ${idx + 1}`, cp.x, cp.y - 45);
      }
    });

    // 4. Draw Player Vehicle
    ctx.save();
    ctx.translate(state.x, state.y);
    ctx.rotate(state.angle);

    // Vehicle Shadow
    ctx.fillStyle = 'rgba(0, 0, 0, 0.6)';
    ctx.beginPath();
    ctx.roundRect(-24, -14, 48, 28, 6);
    ctx.fill();

    // Body chassis
    ctx.fillStyle = selectedVehicle.accentColor;
    ctx.beginPath();
    ctx.roundRect(-22, -12, 44, 24, 5);
    ctx.fill();
    ctx.lineWidth = 2;
    ctx.strokeStyle = '#ffffff';
    ctx.stroke();

    // Windshield & Roof
    ctx.fillStyle = '#0a101f';
    ctx.beginPath();
    ctx.roundRect(-8, -9, 20, 18, 3);
    ctx.fill();

    // Headlights (High-intensity neon cones)
    ctx.fillStyle = 'rgba(0, 210, 255, 0.35)';
    ctx.beginPath();
    ctx.moveTo(22, -8);
    ctx.lineTo(95, -35);
    ctx.lineTo(95, 35);
    ctx.lineTo(22, 8);
    ctx.closePath();
    ctx.fill();

    // Taillights
    ctx.fillStyle = isDrifting ? '#ff0033' : '#ff4444';
    ctx.fillRect(-22, -10, 3, 5);
    ctx.fillRect(-22, 5, 3, 5);

    // Nitrous exhaust flame
    if (isNitroActive) {
      ctx.fillStyle = '#00f0ff';
      ctx.beginPath();
      ctx.moveTo(-24, -4);
      ctx.lineTo(-45 - Math.random() * 15, 0);
      ctx.lineTo(-24, 4);
      ctx.closePath();
      ctx.fill();
    }

    ctx.restore();
    ctx.restore();
  };

  // Draw procedural track ribbon
  const drawCircuitTrack = (ctx: CanvasRenderingContext2D) => {
    ctx.lineWidth = 140;
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';

    // Dark asphalt base
    ctx.strokeStyle = '#121826';
    ctx.beginPath();
    const pts = checkpoints.current;
    ctx.moveTo(pts[0].x, pts[0].y);
    for (let i = 1; i < pts.length; i++) {
      ctx.lineTo(pts[i].x, pts[i].y);
    }
    ctx.closePath();
    ctx.stroke();

    // Track curb edges (Cyan & Gold alternating neon barriers)
    ctx.lineWidth = 6;
    ctx.strokeStyle = 'rgba(0, 210, 255, 0.6)';
    ctx.stroke();

    // Center dashed white line
    ctx.setLineDash([20, 20]);
    ctx.lineWidth = 3;
    ctx.strokeStyle = 'rgba(255, 255, 255, 0.25)';
    ctx.stroke();
    ctx.setLineDash([]);
  };

  const startRace = () => {
    audioService.unlockAudio();
    setIsPlaying(true);
    audioService.playClick(880);
  };

  const resetCar = () => {
    carState.current.x = 0;
    carState.current.y = -200;
    carState.current.angle = 0;
    carState.current.speed = 0;
    carState.current.skidmarks = [];
    setCheckpointIndex(0);
    setLapTimeMs(0);
    audioService.playClick(440);
  };

  const cycleCamera = () => {
    const modes: ('CHASE' | 'COCKPIT' | 'TOP')[] = ['CHASE', 'COCKPIT', 'TOP'];
    const next = modes[(modes.indexOf(cameraMode) + 1) % modes.length];
    setCameraMode(next);
    audioService.playClick(650);
  };

  const formatTime = (ms: number) => {
    const min = Math.floor(ms / 60000);
    const sec = Math.floor((ms % 60000) / 1000);
    const hund = Math.floor((ms % 1000) / 10);
    return `${min}:${sec < 10 ? '0' : ''}${sec}.${hund < 10 ? '0' : ''}${hund}`;
  };

  return (
    <section id="play" ref={containerRef} style={{ padding: '40px 0', position: 'relative' }}>
      <div className="container">
        {/* Section Heading & Mode Selector */}
        <div style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          flexWrap: 'wrap',
          gap: '1rem',
          marginBottom: '1.4rem'
        }}>
          <div>
            <div style={{
              display: 'inline-flex',
              alignItems: 'center',
              gap: '0.5rem',
              color: 'var(--accent-cyan)',
              fontFamily: 'var(--font-mono)',
              fontSize: '0.8rem',
              fontWeight: 700,
              textTransform: 'uppercase',
              letterSpacing: '0.08em'
            }}>
              <Flag size={14} /> Playable Route 1 Prototype
            </div>
            <h2 style={{ fontSize: 'clamp(1.6rem, 3vw, 2.4rem)', color: 'var(--text-highlight)' }}>
              DOWNTOWN CIRCUIT RUNTIME
            </h2>
          </div>

          {/* Mode Switcher Tabs */}
          <div style={{
            display: 'flex',
            alignItems: 'center',
            background: 'var(--bg-obsidian)',
            padding: '4px',
            borderRadius: 'var(--radius-sm)',
            border: '1px solid var(--border-subtle)'
          }}>
            <button
              onClick={() => { audioService.playClick(600); setViewMode('ARCADE_SIMULATOR'); }}
              style={{
                ...tabBtnStyle,
                background: viewMode === 'ARCADE_SIMULATOR' ? 'var(--accent-cyan)' : 'transparent',
                color: viewMode === 'ARCADE_SIMULATOR' ? '#03060f' : 'var(--text-sub)'
              }}
            >
              <Play size={13} /> Browser Arcade Drive
            </button>

            <button
              onClick={() => { audioService.playClick(600); setViewMode('UNITY_WEBGL'); }}
              style={{
                ...tabBtnStyle,
                background: viewMode === 'UNITY_WEBGL' ? 'var(--accent-gold)' : 'transparent',
                color: viewMode === 'UNITY_WEBGL' ? '#03060f' : 'var(--text-sub)'
              }}
            >
              <Monitor size={13} /> Unity WebGL Loader
            </button>

            <button
              onClick={() => { audioService.playClick(600); setViewMode('TELEMETRY'); }}
              style={{
                ...tabBtnStyle,
                background: viewMode === 'TELEMETRY' ? 'var(--accent-lime)' : 'transparent',
                color: viewMode === 'TELEMETRY' ? '#03060f' : 'var(--text-sub)'
              }}
            >
              <Activity size={13} /> Web Telemetry
            </button>
          </div>
        </div>

        {/* Viewport Frame (16:9 Cinema Aspect) */}
        <div className="glass-panel" style={{
          position: 'relative',
          borderRadius: 'var(--radius-md)',
          overflow: 'hidden',
          aspectRatio: '16/9',
          minHeight: '440px',
          maxHeight: '740px',
          border: '1px solid var(--border-active)',
          boxShadow: '0 12px 40px rgba(0, 0, 0, 0.6), 0 0 30px rgba(0, 210, 255, 0.15)'
        }}>
          {/* MODE 1: BROWSER ARCADE SIMULATOR */}
          {viewMode === 'ARCADE_SIMULATOR' && (
            <>
              <canvas
                ref={canvasRef}
                width={1280}
                height={720}
                style={{
                  width: '100%',
                  height: '100%',
                  display: 'block',
                  background: '#070b16'
                }}
              />

              {/* Start Engine Overlay (if not started) */}
              {!isPlaying && (
                <div style={{
                  position: 'absolute',
                  inset: 0,
                  background: 'rgba(3, 6, 15, 0.88)',
                  backdropFilter: 'blur(10px)',
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center',
                  textAlign: 'center',
                  padding: '2rem',
                  zIndex: 20
                }}>
                  <div style={{
                    width: '68px',
                    height: '68px',
                    borderRadius: '50%',
                    background: 'rgba(0, 210, 255, 0.12)',
                    border: '2px solid var(--accent-cyan)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    marginBottom: '1.2rem',
                    boxShadow: '0 0 25px rgba(0, 210, 255, 0.4)'
                  }}>
                    <Play size={32} color="var(--accent-cyan)" fill="var(--accent-cyan)" />
                  </div>
                  <h3 style={{ fontSize: '1.8rem', marginBottom: '0.4rem', color: '#fff' }}>
                    READY TO RACE {selectedVehicle.name.toUpperCase()}
                  </h3>
                  <p style={{ color: 'var(--text-sub)', maxWidth: '480px', marginBottom: '1.8rem', fontSize: '0.92rem' }}>
                    Click below to ignite the engine and unlock real-time Web Audio synthesis. Drive using WASD or Arrow Keys, drift with Space, and cycle cameras with C.
                  </p>
                  <button 
                    onClick={startRace}
                    className="btn-primary" 
                    style={{ fontSize: '1.05rem', padding: '0.95rem 2.5rem' }}
                  >
                    <Flame size={18} fill="#03060f" /> START ENGINE & RACE
                  </button>
                </div>
              )}

              {/* RUNTIME HUD OVERLAY */}
              {isPlaying && (
                <div style={{ position: 'absolute', inset: 0, pointerEvents: 'none', padding: '1.2rem', zIndex: 10 }}>
                  {/* Top Bar: Lap Timer & Checkpoint Progress */}
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                    {/* Lap & Split Timing */}
                    <div className="glass-panel" style={{ padding: '0.6rem 1.1rem', pointerEvents: 'auto' }}>
                      <div style={{ fontSize: '0.7rem', color: 'var(--accent-gold)', fontFamily: 'var(--font-mono)' }}>
                        LAP {currentLap} / {totalLaps}
                      </div>
                      <div style={{ fontSize: '1.4rem', fontWeight: 800, fontFamily: 'var(--font-mono)', color: '#fff' }}>
                        {formatTime(lapTimeMs)}
                      </div>
                      {bestLapTimeMs && (
                        <div style={{ fontSize: '0.68rem', color: 'var(--accent-lime)', fontFamily: 'var(--font-mono)' }}>
                          BEST: {formatTime(bestLapTimeMs)}
                        </div>
                      )}
                    </div>

                    {/* Checkpoint Counter */}
                    <div className="glass-panel" style={{ padding: '0.6rem 1.1rem', textAlign: 'center', pointerEvents: 'auto' }}>
                      <div style={{ fontSize: '0.68rem', color: 'var(--text-sub)', fontFamily: 'var(--font-mono)' }}>
                        SECTOR CHECKPOINT
                      </div>
                      <div style={{ fontSize: '1.2rem', fontWeight: 800, color: 'var(--accent-cyan)', fontFamily: 'var(--font-mono)' }}>
                        {checkpointIndex + 1} <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>/ {totalCheckpoints}</span>
                      </div>
                    </div>

                    {/* Camera Mode & Actions */}
                    <div style={{ display: 'flex', gap: '0.5rem', pointerEvents: 'auto' }}>
                      <button 
                        onClick={cycleCamera} 
                        style={hudBtnStyle} 
                        title="Change Camera Angle (C)"
                      >
                        CAM: {cameraMode}
                      </button>
                      <button 
                        onClick={resetCar} 
                        style={hudBtnStyle} 
                        title="Reset Vehicle to Track (R)"
                      >
                        <RotateCcw size={14} /> RESET
                      </button>
                    </div>
                  </div>

                  {/* Bottom Right: Digital Tachometer & Speedometer HUD */}
                  <div style={{
                    position: 'absolute',
                    bottom: '1.2rem',
                    right: '1.2rem',
                    display: 'flex',
                    alignItems: 'flex-end',
                    gap: '1rem',
                    pointerEvents: 'auto'
                  }}>
                    {/* Nitro Tank Meter */}
                    <div className="glass-panel" style={{
                      padding: '0.6rem',
                      display: 'flex',
                      flexDirection: 'column',
                      alignItems: 'center',
                      gap: '0.3rem'
                    }}>
                      <div style={{
                        height: '75px',
                        width: '12px',
                        background: 'rgba(255,255,255,0.1)',
                        borderRadius: '6px',
                        display: 'flex',
                        flexDirection: 'column',
                        justifyContent: 'flex-end',
                        overflow: 'hidden'
                      }}>
                        <div style={{
                          height: `${nitroAmount}%`,
                          background: isNitroActive ? 'var(--accent-cyan)' : 'var(--accent-crimson)',
                          boxShadow: '0 0 10px var(--accent-cyan)',
                          transition: 'height 0.1s linear'
                        }} />
                      </div>
                      <span style={{ fontSize: '0.6rem', fontFamily: 'var(--font-mono)', color: 'var(--accent-cyan)' }}>
                        NOS
                      </span>
                    </div>

                    {/* Main Cluster */}
                    <div className="glass-panel" style={{
                      padding: '0.8rem 1.4rem',
                      display: 'flex',
                      alignItems: 'baseline',
                      gap: '0.8rem',
                      background: 'rgba(7, 13, 28, 0.85)'
                    }}>
                      {/* Gear */}
                      <div style={{
                        fontSize: '2.4rem',
                        fontWeight: 900,
                        color: 'var(--accent-gold)',
                        fontFamily: 'var(--font-mono)',
                        lineHeight: 1
                      }}>
                        {gear}
                      </div>

                      {/* Speedometer */}
                      <div>
                        <div style={{ display: 'flex', alignItems: 'baseline', gap: '0.3rem' }}>
                          <span style={{
                            fontSize: '2.6rem',
                            fontWeight: 900,
                            fontFamily: 'var(--font-mono)',
                            color: '#ffffff',
                            lineHeight: 1
                          }}>
                            {speedKmh}
                          </span>
                          <span style={{ fontSize: '0.75rem', fontFamily: 'var(--font-mono)', color: 'var(--accent-cyan)' }}>
                            KM/H
                          </span>
                        </div>
                        <div style={{ fontSize: '0.7rem', color: 'var(--text-sub)', fontFamily: 'var(--font-mono)' }}>
                          {rpm} RPM {isDrifting ? '• [DRIFT SLIP]' : ''}
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Bottom Left: Touch Steering Controls for Mobile / Tablet */}
                  <div style={{
                    position: 'absolute',
                    bottom: '1.2rem',
                    left: '1.2rem',
                    display: 'flex',
                    gap: '0.5rem',
                    pointerEvents: 'auto'
                  }}>
                    <button
                      onMouseDown={() => { carState.current.keys.a = true; }}
                      onMouseUp={() => { carState.current.keys.a = false; }}
                      onTouchStart={() => { carState.current.keys.a = true; }}
                      onTouchEnd={() => { carState.current.keys.a = false; }}
                      style={touchBtnStyle}
                    >
                      ◀ STEER
                    </button>
                    <button
                      onMouseDown={() => { carState.current.keys.d = true; }}
                      onMouseUp={() => { carState.current.keys.d = false; }}
                      onTouchStart={() => { carState.current.keys.d = true; }}
                      onTouchEnd={() => { carState.current.keys.d = false; }}
                      style={touchBtnStyle}
                    >
                      STEER ▶
                    </button>
                    <button
                      onMouseDown={() => { carState.current.keys.w = true; }}
                      onMouseUp={() => { carState.current.keys.w = false; }}
                      onTouchStart={() => { carState.current.keys.w = true; }}
                      onTouchEnd={() => { carState.current.keys.w = false; }}
                      style={{ ...touchBtnStyle, background: 'rgba(0, 210, 255, 0.25)', borderColor: 'var(--accent-cyan)' }}
                    >
                      GAS ▲
                    </button>
                    <button
                      onMouseDown={() => { carState.current.keys.s = true; }}
                      onMouseUp={() => { carState.current.keys.s = false; }}
                      onTouchStart={() => { carState.current.keys.s = true; }}
                      onTouchEnd={() => { carState.current.keys.s = false; }}
                      style={{ ...touchBtnStyle, background: 'rgba(255, 45, 85, 0.25)', borderColor: 'var(--accent-crimson)' }}
                    >
                      BRAKE ▼
                    </button>
                    <button
                      onMouseDown={() => { carState.current.keys.space = true; }}
                      onMouseUp={() => { carState.current.keys.space = false; }}
                      onTouchStart={() => { carState.current.keys.space = true; }}
                      onTouchEnd={() => { carState.current.keys.space = false; }}
                      style={{ ...touchBtnStyle, background: 'rgba(255, 170, 0, 0.25)', borderColor: 'var(--accent-gold)' }}
                    >
                      DRIFT
                    </button>
                  </div>
                </div>
              )}
            </>
          )}

          {/* MODE 2: UNITY WEBGL BUILD LOADER */}
          {viewMode === 'UNITY_WEBGL' && (
            <div style={{
              width: '100%',
              height: '100%',
              background: '#050914',
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              justifyContent: 'center',
              padding: '2.5rem',
              textAlign: 'center'
            }}>
              {unityBuildFound ? (
                <iframe
                  src="/Builds/WebGL/index.html"
                  title="GAMEIUM Unity WebGL Player"
                  style={{ width: '100%', height: '100%', border: 'none' }}
                  allow="autoplay; fullscreen; gamepad"
                />
              ) : (
                <div style={{ maxWidth: '640px' }}>
                  <div style={{
                    width: '64px',
                    height: '64px',
                    borderRadius: '16px',
                    background: 'rgba(255, 170, 0, 0.1)',
                    border: '1px solid var(--accent-gold)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    margin: '0 auto 1.4rem auto'
                  }}>
                    <Monitor size={32} color="var(--accent-gold)" />
                  </div>
                  <h3 style={{ fontSize: '1.6rem', color: '#fff', marginBottom: '0.6rem' }}>
                    UNITY WEBGL BUILD RECEPTACLE
                  </h3>
                  <p style={{ color: 'var(--text-sub)', fontSize: '0.92rem', lineHeight: 1.6, marginBottom: '1.5rem' }}>
                    The Web Player is configured to stream and mount the Unity 6 WebGL output from <code style={{ color: 'var(--accent-cyan)' }}>Builds/WebGL/index.html</code>.
                    Settings are primed: <strong>256MB WebAssembly Initial Heap</strong>, <strong>Linear URP Color Space</strong>, and <strong>Wasm Linker Target</strong>.
                  </p>
                  <div className="glass-panel" style={{ padding: '1rem', textAlign: 'left', marginBottom: '1.5rem', fontSize: '0.8rem', fontFamily: 'var(--font-mono)' }}>
                    <div style={{ color: 'var(--accent-cyan)', marginBottom: '0.3rem' }}>[WEBGL COMPATIBILITY PROFILE]</div>
                    <div>• Memory Allocation: 256 MB (Zero allocation pause)</div>
                    <div>• Compression: Disabled for instant static HTTP streaming</div>
                    <div>• Audio: Web Audio API context unlock bridge active</div>
                    <div>• Active Scenes: MainMenu, DowntownCircuit, SampleStreet_GateA</div>
                  </div>
                  <button 
                    onClick={() => setViewMode('ARCADE_SIMULATOR')}
                    className="btn-primary"
                  >
                    <Play size={16} fill="#03060f" /> LAUNCH IN-BROWSER ARCADE DRIVE
                  </button>
                </div>
              )}
            </div>
          )}

          {/* MODE 3: TELEMETRY & DIAGNOSTICS */}
          {viewMode === 'TELEMETRY' && (
            <div style={{
              width: '100%',
              height: '100%',
              background: '#040711',
              padding: '2rem',
              overflowY: 'auto'
            }}>
              <h3 style={{ fontSize: '1.4rem', color: 'var(--accent-lime)', marginBottom: '1rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <Activity size={20} /> WEBGL ENGINE TELEMETRY & SYSTEM GAUGES
              </h3>

              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: '1rem', marginBottom: '1.5rem' }}>
                <div className="glass-panel" style={{ padding: '1rem' }}>
                  <div style={{ fontSize: '0.72rem', color: 'var(--text-sub)', fontFamily: 'var(--font-mono)' }}>REFRESH RATE / FPS</div>
                  <div style={{ fontSize: '1.8rem', fontWeight: 800, color: fps > 50 ? 'var(--accent-lime)' : 'var(--accent-gold)', fontFamily: 'var(--font-mono)' }}>
                    {fps} FPS
                  </div>
                  <div style={{ fontSize: '0.7rem', color: 'var(--text-muted)' }}>Target: 60 FPS VSync</div>
                </div>

                <div className="glass-panel" style={{ padding: '1rem' }}>
                  <div style={{ fontSize: '0.72rem', color: 'var(--text-sub)', fontFamily: 'var(--font-mono)' }}>HEAP ALLOCATION</div>
                  <div style={{ fontSize: '1.8rem', fontWeight: 800, color: 'var(--accent-cyan)', fontFamily: 'var(--font-mono)' }}>
                    256 MB
                  </div>
                  <div style={{ fontSize: '0.7rem', color: 'var(--text-muted)' }}>Max Limit: 2048 MB Dynamic</div>
                </div>

                <div className="glass-panel" style={{ padding: '1rem' }}>
                  <div style={{ fontSize: '0.72rem', color: 'var(--text-sub)', fontFamily: 'var(--font-mono)' }}>COLOR PIPELINE</div>
                  <div style={{ fontSize: '1.8rem', fontWeight: 800, color: 'var(--accent-gold)', fontFamily: 'var(--font-mono)' }}>
                    LINEAR URP
                  </div>
                  <div style={{ fontSize: '0.7rem', color: 'var(--text-muted)' }}>PBR Metallic/Roughness</div>
                </div>

                <div className="glass-panel" style={{ padding: '1rem' }}>
                  <div style={{ fontSize: '0.72rem', color: 'var(--text-sub)', fontFamily: 'var(--font-mono)' }}>AUDIO API STATE</div>
                  <div style={{ fontSize: '1.8rem', fontWeight: 800, color: 'var(--accent-lime)', fontFamily: 'var(--font-mono)' }}>
                    SYNTH V1
                  </div>
                  <div style={{ fontSize: '0.7rem', color: 'var(--text-muted)' }}>Zero-latency procedural</div>
                </div>
              </div>

              <div className="glass-panel" style={{ padding: '1.2rem', fontFamily: 'var(--font-mono)', fontSize: '0.82rem', lineHeight: 1.7 }}>
                <div style={{ color: 'var(--accent-cyan)', fontWeight: 700, marginBottom: '0.5rem' }}>[BROWSER COMPATIBILITY REPORT]</div>
                <div>• WebGL 2.0 Context: Supported (Chrome / Edge / Firefox / Safari 16.4+)</div>
                <div>• WebAssembly SIMD: Supported</div>
                <div>• User Agent: {navigator.userAgent.substring(0, 95)}...</div>
                <div>• Platform Orientation: Landscape Recommended (Auto-lock via CSS)</div>
                <div>• Storage: localStorage + IndexedDB Progressive Cache Supported</div>
              </div>
            </div>
          )}
        </div>
      </div>
    </section>
  );
};

const tabBtnStyle: React.CSSProperties = {
  display: 'inline-flex',
  alignItems: 'center',
  gap: '0.45rem',
  padding: '0.45rem 0.95rem',
  borderRadius: '4px',
  border: 'none',
  fontSize: '0.78rem',
  fontWeight: 700,
  fontFamily: 'var(--font-mono)',
  letterSpacing: '0.04em',
  cursor: 'pointer',
  transition: 'all 0.2s ease'
};

const hudBtnStyle: React.CSSProperties = {
  background: 'rgba(7, 13, 28, 0.75)',
  border: '1px solid var(--border-subtle)',
  color: 'var(--text-main)',
  padding: '0.45rem 0.85rem',
  borderRadius: '6px',
  fontSize: '0.72rem',
  fontFamily: 'var(--font-mono)',
  cursor: 'pointer',
  display: 'flex',
  alignItems: 'center',
  gap: '0.35rem',
  backdropFilter: 'blur(8px)'
};

const touchBtnStyle: React.CSSProperties = {
  background: 'rgba(13, 22, 42, 0.75)',
  border: '1px solid var(--border-subtle)',
  color: 'var(--text-main)',
  padding: '0.5rem 0.75rem',
  borderRadius: '6px',
  fontSize: '0.7rem',
  fontFamily: 'var(--font-mono)',
  fontWeight: 700,
  cursor: 'pointer',
  backdropFilter: 'blur(8px)',
  userSelect: 'none'
};
