/**
 * GAMEIUM Audio Engine — Web Audio API Synthesizer
 * Provides instant, zero-latency procedural audio effects for browser vehicle preview and UI.
 */
class GameiumAudioEngine {
  private ctx: AudioContext | null = null;
  private engineOsc: OscillatorNode | null = null;
  private engineGain: GainNode | null = null;
  private isEngineRunning: boolean = false;
  private isMuted: boolean = false;

  private initContext() {
    if (!this.ctx) {
      const AudioCtx = window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext;
      this.ctx = new AudioCtx();
    }
    if (this.ctx.state === 'suspended') {
      this.ctx.resume();
    }
  }

  public unlockAudio(): boolean {
    try {
      this.initContext();
      return this.ctx?.state === 'running';
    } catch (e) {
      console.warn('[GAMEIUM Audio] Unlock error:', e);
      return false;
    }
  }

  public toggleMute(): boolean {
    this.isMuted = !this.isMuted;
    if (this.isMuted && this.engineGain) {
      this.engineGain.gain.setValueAtTime(0, this.ctx!.currentTime);
    }
    return this.isMuted;
  }

  public getMuted(): boolean {
    return this.isMuted;
  }

  /**
   * Synthesize a UI tick or button click
   */
  public playClick(freq = 880) {
    if (this.isMuted) return;
    try {
      this.initContext();
      if (!this.ctx) return;

      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();

      osc.type = 'triangle';
      osc.frequency.setValueAtTime(freq, this.ctx.currentTime);
      osc.frequency.exponentialRampToValueAtTime(freq * 0.5, this.ctx.currentTime + 0.06);

      gain.gain.setValueAtTime(0.15, this.ctx.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.001, this.ctx.currentTime + 0.06);

      osc.connect(gain);
      gain.connect(this.ctx.destination);

      osc.start();
      osc.stop(this.ctx.currentTime + 0.07);
    } catch {
      // Audio autoplay policy fallback
    }
  }

  /**
   * Synthesize a checkpoint chime
   */
  public playCheckpoint() {
    if (this.isMuted) return;
    try {
      this.initContext();
      if (!this.ctx) return;

      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();

      osc.type = 'sine';
      osc.frequency.setValueAtTime(587.33, this.ctx.currentTime); // D5
      osc.frequency.setValueAtTime(880.00, this.ctx.currentTime + 0.08); // A5
      osc.frequency.setValueAtTime(1174.66, this.ctx.currentTime + 0.16); // D6

      gain.gain.setValueAtTime(0.2, this.ctx.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.001, this.ctx.currentTime + 0.35);

      osc.connect(gain);
      gain.connect(this.ctx.destination);

      osc.start();
      osc.stop(this.ctx.currentTime + 0.36);
    } catch {}
  }

  /**
   * Synthesize tire screech for drift
   */
  public playTireScreech() {
    if (this.isMuted) return;
    try {
      this.initContext();
      if (!this.ctx) return;

      // Filtered white noise
      const bufferSize = this.ctx.sampleRate * 0.2;
      const buffer = this.ctx.createBuffer(1, bufferSize, this.ctx.sampleRate);
      const data = buffer.getChannelData(0);
      for (let i = 0; i < bufferSize; i++) {
        data[i] = Math.random() * 2 - 1;
      }

      const noise = this.ctx.createBufferSource();
      noise.buffer = buffer;

      const filter = this.ctx.createBiquadFilter();
      filter.type = 'bandpass';
      filter.frequency.setValueAtTime(1200, this.ctx.currentTime);
      filter.Q.setValueAtTime(3.0, this.ctx.currentTime);

      const gain = this.ctx.createGain();
      gain.gain.setValueAtTime(0.08, this.ctx.currentTime);
      gain.gain.linearRampToValueAtTime(0.001, this.ctx.currentTime + 0.2);

      noise.connect(filter);
      filter.connect(gain);
      gain.connect(this.ctx.destination);

      noise.start();
    } catch {}
  }

  /**
   * Synthesize Nitro Burst
   */
  public playNitroBurst() {
    if (this.isMuted) return;
    try {
      this.initContext();
      if (!this.ctx) return;

      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();

      osc.type = 'sawtooth';
      osc.frequency.setValueAtTime(120, this.ctx.currentTime);
      osc.frequency.exponentialRampToValueAtTime(540, this.ctx.currentTime + 0.4);

      gain.gain.setValueAtTime(0.2, this.ctx.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.01, this.ctx.currentTime + 0.4);

      osc.connect(gain);
      gain.connect(this.ctx.destination);

      osc.start();
      osc.stop(this.ctx.currentTime + 0.42);
    } catch {}
  }

  /**
   * Interactive Engine Rev (e.g. for garage preview)
   */
  public revEngine(duration = 1.2, maxPitch = 380, basePitch = 90) {
    if (this.isMuted) return;
    try {
      this.initContext();
      if (!this.ctx) return;

      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();

      osc.type = 'sawtooth';
      const now = this.ctx.currentTime;

      // Rev up and down
      osc.frequency.setValueAtTime(basePitch, now);
      osc.frequency.exponentialRampToValueAtTime(maxPitch, now + duration * 0.45);
      osc.frequency.exponentialRampToValueAtTime(basePitch * 1.1, now + duration);

      gain.gain.setValueAtTime(0.18, now);
      gain.gain.linearRampToValueAtTime(0.25, now + duration * 0.45);
      gain.gain.exponentialRampToValueAtTime(0.001, now + duration);

      osc.connect(gain);
      gain.connect(this.ctx.destination);

      osc.start();
      osc.stop(now + duration + 0.05);
    } catch {}
  }

  /**
   * Continuous engine simulation for the Arcade Test Drive
   */
  public setArcadeEngineRPM(rpmNormalized: number) { // 0.0 to 1.0
    if (this.isMuted) {
      if (this.engineGain) this.engineGain.gain.setValueAtTime(0, this.ctx?.currentTime || 0);
      return;
    }

    try {
      this.initContext();
      if (!this.ctx) return;

      if (!this.isEngineRunning) {
        this.engineOsc = this.ctx.createOscillator();
        this.engineGain = this.ctx.createGain();

        this.engineOsc.type = 'sawtooth';
        this.engineOsc.frequency.setValueAtTime(75, this.ctx.currentTime);
        this.engineGain.gain.setValueAtTime(0.08, this.ctx.currentTime);

        this.engineOsc.connect(this.engineGain);
        this.engineGain.connect(this.ctx.destination);
        this.engineOsc.start();
        this.isEngineRunning = true;
      }

      if (this.engineOsc && this.engineGain) {
        const targetFreq = 75 + rpmNormalized * 280; // 75Hz idle to 355Hz redline
        const now = this.ctx.currentTime;
        this.engineOsc.frequency.setTargetAtTime(targetFreq, now, 0.08);
        this.engineGain.gain.setTargetAtTime(0.05 + rpmNormalized * 0.12, now, 0.08);
      }
    } catch {}
  }

  public stopArcadeEngine() {
    if (this.isEngineRunning && this.engineOsc) {
      try {
        this.engineOsc.stop();
        this.engineOsc.disconnect();
      } catch {}
      this.engineOsc = null;
      this.engineGain = null;
      this.isEngineRunning = false;
    }
  }
}

export const audioService = new GameiumAudioEngine();
