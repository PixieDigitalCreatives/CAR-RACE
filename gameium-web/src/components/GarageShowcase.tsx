import React, { useState, useEffect, useRef } from 'react';
import { Volume2, CheckCircle2 } from 'lucide-react';
import { VEHICLES } from '../data/gameData';
import type { VehicleData } from '../data/gameData';
import { audioService } from '../services/audioSynth';

interface GarageShowcaseProps {
  selectedVehicle: VehicleData;
  onSelectVehicle: (v: VehicleData) => void;
  onRaceWithVehicle: (v: VehicleData) => void;
}

export const GarageShowcase: React.FC<GarageShowcaseProps> = ({
  selectedVehicle,
  onSelectVehicle,
  onRaceWithVehicle
}) => {
  const [rotationAngle, setRotationAngle] = useState(0);
  const [isAutoRotating, setIsAutoRotating] = useState(true);
  const [isRevving, setIsRevving] = useState(false);
  const canvasRef = useRef<HTMLCanvasElement | null>(null);

  // Turntable Canvas Animation
  useEffect(() => {
    let animId: number;
    let angle = rotationAngle;

    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const renderTurntable = () => {
      if (isAutoRotating) {
        angle += 0.015;
        setRotationAngle(angle);
      }

      const w = canvas.width;
      const h = canvas.height;
      ctx.clearRect(0, 0, w, h);

      ctx.save();
      ctx.translate(w / 2, h / 2 + 30);

      // 1. Draw glowing circular turntable platform
      ctx.beginPath();
      ctx.ellipse(0, 45, 160, 45, 0, 0, Math.PI * 2);
      ctx.fillStyle = 'rgba(10, 18, 38, 0.9)';
      ctx.fill();
      ctx.lineWidth = 4;
      ctx.strokeStyle = selectedVehicle.accentColor;
      ctx.shadowColor = selectedVehicle.accentColor;
      ctx.shadowBlur = 20;
      ctx.stroke();
      ctx.shadowBlur = 0;

      // Platform grid marks
      for (let i = 0; i < 8; i++) {
        const rad = angle + (i * Math.PI) / 4;
        const px = Math.cos(rad) * 145;
        const py = Math.sin(rad) * 40;
        ctx.fillStyle = 'rgba(255, 255, 255, 0.4)';
        ctx.fillRect(px - 2, py + 43, 4, 4);
      }

      // 2. Draw 3D Silhouette Car Mockup at angle
      ctx.save();
      ctx.scale(1, 0.65); // perspective squash
      ctx.rotate(angle);

      // Neon Underglow
      ctx.beginPath();
      ctx.ellipse(0, 0, 110, 55, 0, 0, Math.PI * 2);
      ctx.fillStyle = `rgba(${selectedVehicle.accentColor === '#00d2ff' ? '0,210,255' : '255,170,0'}, 0.25)`;
      ctx.fill();

      // Shadow
      ctx.fillStyle = 'rgba(0, 0, 0, 0.8)';
      ctx.beginPath();
      ctx.roundRect(-90, -45, 180, 90, 15);
      ctx.fill();

      // Vehicle Chassis
      ctx.fillStyle = selectedVehicle.accentColor;
      ctx.beginPath();
      ctx.roundRect(-85, -40, 170, 80, 12);
      ctx.fill();
      ctx.lineWidth = 3;
      ctx.strokeStyle = '#ffffff';
      ctx.stroke();

      // Cabin / Glass
      ctx.fillStyle = '#060a14';
      ctx.beginPath();
      ctx.roundRect(-30, -28, 70, 56, 8);
      ctx.fill();
      ctx.lineWidth = 2;
      ctx.strokeStyle = 'rgba(255, 255, 255, 0.4)';
      ctx.stroke();

      // Wheels
      ctx.fillStyle = '#1e293b';
      ctx.fillRect(-65, -48, 30, 10);
      ctx.fillRect(35, -48, 30, 10);
      ctx.fillRect(-65, 38, 30, 10);
      ctx.fillRect(35, 38, 30, 10);

      // Headlight Beams
      ctx.fillStyle = 'rgba(0, 210, 255, 0.4)';
      ctx.beginPath();
      ctx.moveTo(85, -25);
      ctx.lineTo(190, -60);
      ctx.lineTo(190, 60);
      ctx.lineTo(85, 25);
      ctx.closePath();
      ctx.fill();

      ctx.restore();
      ctx.restore();

      animId = requestAnimationFrame(renderTurntable);
    };

    animId = requestAnimationFrame(renderTurntable);
    return () => cancelAnimationFrame(animId);
  }, [selectedVehicle, isAutoRotating]);

  const handleRevEngine = () => {
    setIsRevving(true);
    audioService.revEngine(1.3, selectedVehicle.audioPitch);
    setTimeout(() => setIsRevving(false), 1400);
  };

  return (
    <section id="garage" style={{ padding: '80px 0', position: 'relative' }}>
      <div className="container">
        {/* Section Header */}
        <div style={{ textAlign: 'center', marginBottom: '3rem' }}>
          <div style={{
            color: 'var(--accent-gold)',
            fontFamily: 'var(--font-mono)',
            fontSize: '0.85rem',
            fontWeight: 700,
            letterSpacing: '0.12em',
            textTransform: 'uppercase',
            marginBottom: '0.4rem'
          }}>
            3D Vehicle Configurator & Garage
          </div>
          <h2 style={{ fontSize: 'clamp(2rem, 3.5vw, 3rem)', color: '#fff' }}>
            SELECT YOUR MACHINE
          </h2>
          <p style={{ color: 'var(--text-sub)', maxWidth: '640px', margin: '0 auto' }}>
            Five distinct chassis engineered for the coastal circuit. From ultra-lightweight street tuners to twin-turbo hypercars and sand-crushing trophy trucks.
          </p>
        </div>

        {/* 5 Vehicle Selector Tabs */}
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
          gap: '0.8rem',
          marginBottom: '2.5rem'
        }}>
          {VEHICLES.map(v => {
            const isSelected = v.id === selectedVehicle.id;
            return (
              <button
                key={v.id}
                onClick={() => {
                  audioService.playClick(600);
                  onSelectVehicle(v);
                }}
                className="glass-panel"
                style={{
                  padding: '1rem',
                  textAlign: 'left',
                  cursor: 'pointer',
                  border: isSelected ? `2px solid ${v.accentColor}` : '1px solid var(--border-subtle)',
                  background: isSelected ? 'rgba(15, 25, 48, 0.95)' : 'var(--bg-card)',
                  boxShadow: isSelected ? `0 0 20px rgba(0, 210, 255, 0.25)` : 'none'
                }}
              >
                <div style={{ fontSize: '0.68rem', color: isSelected ? v.accentColor : 'var(--text-muted)', fontFamily: 'var(--font-mono)', fontWeight: 700 }}>
                  {v.category.toUpperCase()}
                </div>
                <div style={{ fontSize: '1.05rem', fontWeight: 800, color: '#fff', margin: '0.2rem 0' }}>
                  {v.name}
                </div>
                <div style={{ fontSize: '0.72rem', color: 'var(--text-sub)' }}>
                  {v.specs.topSpeedKmh} KM/H • {v.specs.driveTrain}
                </div>
              </button>
            );
          })}
        </div>

        {/* Selected Vehicle Showcase Display (2-column layout) */}
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(360px, 1fr))',
          gap: '2rem',
          alignItems: 'center'
        }}>
          {/* Left Column: 360 Turntable & Artwork Card */}
          <div className="glass-panel" style={{
            position: 'relative',
            padding: '1.5rem',
            borderRadius: 'var(--radius-lg)',
            border: `1px solid ${selectedVehicle.accentColor}44`,
            overflow: 'hidden'
          }}>
            {/* Top Badge */}
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
              <span style={{
                background: `${selectedVehicle.accentColor}22`,
                color: selectedVehicle.accentColor,
                padding: '0.3rem 0.8rem',
                borderRadius: 'var(--radius-full)',
                fontSize: '0.75rem',
                fontFamily: 'var(--font-mono)',
                fontWeight: 700
              }}>
                {selectedVehicle.category}
              </span>

              <button
                onClick={() => setIsAutoRotating(!isAutoRotating)}
                style={{
                  background: 'rgba(255,255,255,0.06)',
                  border: '1px solid var(--border-subtle)',
                  color: 'var(--text-sub)',
                  fontSize: '0.72rem',
                  padding: '0.3rem 0.7rem',
                  borderRadius: '4px',
                  cursor: 'pointer'
                }}
              >
                {isAutoRotating ? 'PAUSE 360°' : 'ROTATE 360°'}
              </button>
            </div>

            {/* Turntable Canvas */}
            <div style={{ position: 'relative', width: '100%', height: '280px' }}>
              <canvas
                ref={canvasRef}
                width={500}
                height={280}
                style={{ width: '100%', height: '100%', display: 'block' }}
              />
            </div>

            {/* Car Card Preview */}
            <div style={{
              display: 'flex',
              alignItems: 'center',
              gap: '1rem',
              marginTop: '1rem',
              padding: '0.8rem',
              background: 'rgba(5, 10, 22, 0.6)',
              borderRadius: 'var(--radius-md)'
            }}>
              <img
                src={selectedVehicle.cardImage}
                alt={selectedVehicle.name}
                style={{ width: '80px', height: '50px', objectFit: 'cover', borderRadius: '6px' }}
                onError={(e) => {
                  (e.target as HTMLElement).style.display = 'none';
                }}
              />
              <div style={{ flex: 1 }}>
                <div style={{ fontSize: '0.85rem', fontWeight: 700, color: '#fff' }}>
                  {selectedVehicle.tagline}
                </div>
                <div style={{ fontSize: '0.75rem', color: 'var(--text-sub)' }}>
                  Weight: {selectedVehicle.specs.weightKg} kg • Drivetrain: {selectedVehicle.specs.driveTrain}
                </div>
              </div>
            </div>
          </div>

          {/* Right Column: Detailed Performance Telemetry & Controls */}
          <div>
            <div style={{
              fontSize: '0.8rem',
              color: selectedVehicle.accentColor,
              fontFamily: 'var(--font-mono)',
              fontWeight: 700,
              letterSpacing: '0.1em'
            }}>
              CHASSIS SPECIFICATIONS
            </div>
            <h3 style={{ fontSize: '2.4rem', color: '#fff', marginBottom: '0.5rem' }}>
              {selectedVehicle.name}
            </h3>
            <p style={{ color: 'var(--text-sub)', fontSize: '0.95rem', lineHeight: 1.6, marginBottom: '2rem' }}>
              {selectedVehicle.description}
            </p>

            {/* Animated Performance Bars */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1.2rem', marginBottom: '2.5rem' }}>
              <SpecBar
                label="TOP SPEED"
                value={`${selectedVehicle.specs.topSpeedKmh} KM/H`}
                percentage={(selectedVehicle.specs.topSpeedKmh / 350) * 100}
                color={selectedVehicle.accentColor}
              />
              <SpecBar
                label="ACCELERATION (0-100 KM/H)"
                value={`${selectedVehicle.specs.acceleration0to100}s`}
                percentage={((5.0 - selectedVehicle.specs.acceleration0to100) / 3.0) * 100}
                color="var(--accent-lime)"
              />
              <SpecBar
                label="HANDLING & LATERAL G"
                value={`${selectedVehicle.specs.handlingRating}/100`}
                percentage={selectedVehicle.specs.handlingRating}
                color="var(--accent-cyan)"
              />
              <SpecBar
                label="DRIFT SLIP ANGLE"
                value={`${selectedVehicle.specs.driftRating}/100`}
                percentage={selectedVehicle.specs.driftRating}
                color="var(--accent-gold)"
              />
              <SpecBar
                label="NITROUS CAPACITY"
                value={`${selectedVehicle.specs.boostCapacitySec}s`}
                percentage={(selectedVehicle.specs.boostCapacitySec / 7.0) * 100}
                color="var(--accent-crimson)"
              />
            </div>

            {/* Action Buttons */}
            <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
              <button
                onClick={handleRevEngine}
                className="btn-secondary"
                style={{ padding: '0.9rem 1.6rem' }}
              >
                <Volume2 size={18} color="var(--accent-gold)" />
                {isRevving ? 'REVVIING REDLINE...' : 'REV ENGINE SOUND'}
              </button>

              <button
                onClick={() => {
                  audioService.playClick(880);
                  onRaceWithVehicle(selectedVehicle);
                }}
                className="btn-primary"
                style={{
                  background: `linear-gradient(135deg, ${selectedVehicle.accentColor} 0%, #0077aa 100%)`,
                  padding: '0.9rem 2rem'
                }}
              >
                <CheckCircle2 size={18} fill="#03060f" />
                SELECT FOR DOWNTOWN CIRCUIT
              </button>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

const SpecBar: React.FC<{ label: string; value: string; percentage: number; color: string }> = ({
  label,
  value,
  percentage,
  color
}) => (
  <div>
    <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.75rem', fontFamily: 'var(--font-mono)', marginBottom: '0.35rem' }}>
      <span style={{ color: 'var(--text-sub)' }}>{label}</span>
      <span style={{ color: '#fff', fontWeight: 700 }}>{value}</span>
    </div>
    <div style={{ height: '6px', background: 'rgba(255,255,255,0.08)', borderRadius: '3px', overflow: 'hidden' }}>
      <div style={{
        height: '100%',
        width: `${Math.min(100, Math.max(5, percentage))}%`,
        background: color,
        boxShadow: `0 0 10px ${color}`,
        borderRadius: '3px',
        transition: 'width 0.5s ease-out'
      }} />
    </div>
  </div>
);
