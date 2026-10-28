using UnityEngine;
using ThegameioRacing.Vehicle;

namespace ThegameioRacing.Audio
{
    /// <summary>
    /// 100% License-free procedural audio synthesizer for GAMEIUM.
    /// Synthesizes combustion engine rumble, countdown beeps, tire screeching,
    /// checkpoint chimes, and race finish fanfare without requiring external audio files.
    /// </summary>
    public static class ProceduralAudio
    {
        private static AudioClip cachedEngineClip;
        private static AudioClip cachedSkidClip;

        /// <summary>
        /// Creates or returns a cached 1-second seamless engine rumble loop.
        /// Synthesizes fundamental 60Hz firing pulse with 2nd, 3rd, and 4th harmonics.
        /// </summary>
        public static AudioClip GetOrCreateEngineClip()
        {
            if (cachedEngineClip != null) return cachedEngineClip;

            int sampleRate = 44100;
            float duration = 1.0f;
            int numSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[numSamples];

            float baseFreq = 62.0f; // Idle combustion rumble

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;

                // Combustion firing pulses (sawtooth/pulse shape with multiple harmonics)
                float wave1 = Mathf.Sin(2f * Mathf.PI * baseFreq * t);
                float wave2 = Mathf.Sin(2f * Mathf.PI * baseFreq * 2f * t) * 0.65f;
                float wave3 = Mathf.Sin(2f * Mathf.PI * baseFreq * 3f * t) * 0.35f;
                float wave4 = Mathf.Sin(2f * Mathf.PI * baseFreq * 4f * t) * 0.20f;
                float waveSub = Mathf.Sin(2f * Mathf.PI * (baseFreq * 0.5f) * t) * 0.45f;

                // Subtle pink noise for mechanical intake rush
                float noise = (Random.value * 2f - 1f) * 0.08f;

                float sample = (wave1 + wave2 + wave3 + wave4 + waveSub + noise) * 0.35f;

                // Soft clip compression
                samples[i] = Mathf.Clamp(sample, -0.95f, 0.95f);
            }

            cachedEngineClip = AudioClip.Create("ProceduralEngineLoop", numSamples, 1, sampleRate, false);
            cachedEngineClip.SetData(samples, 0);
            return cachedEngineClip;
        }

        /// <summary>
        /// Synthesizes clean countdown beeps (440Hz for 3-2-1, 880Hz for GO).
        /// </summary>
        public static AudioClip CreateCountdownBeepClip(float frequency, float duration = 0.22f)
        {
            int sampleRate = 44100;
            int numSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = 1f;

                // Envelope attack (0.01s) and decay
                if (t < 0.015f) env = t / 0.015f;
                else env = Mathf.Exp(-4.5f * (t - 0.015f));

                float wave = Mathf.Sin(2f * Mathf.PI * frequency * t);
                samples[i] = wave * env * 0.45f;
            }

            AudioClip clip = AudioClip.Create("Beep_" + frequency, numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Synthesizes a pleasant ascending checkpoint pass chime chord.
        /// </summary>
        public static AudioClip CreateCheckpointChimeClip()
        {
            int sampleRate = 44100;
            float duration = 0.32f;
            int numSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[numSamples];

            float f1 = 587.33f; // D5
            float f2 = 880.00f; // A5
            float f3 = 1174.66f; // D6

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-7f * t);

                float wave = (Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.4f)
                           + (Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.35f)
                           + (Mathf.Sin(2f * Mathf.PI * f3 * t) * 0.25f);

                samples[i] = wave * env * 0.5f;
            }

            AudioClip clip = AudioClip.Create("CheckpointChime", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Synthesizes high-frequency tire squeal noise for high slip / drift conditions.
        /// </summary>
        public static AudioClip GetOrCreateSkidClip()
        {
            if (cachedSkidClip != null) return cachedSkidClip;

            int sampleRate = 44100;
            float duration = 0.8f;
            int numSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                // Modulated friction shrieks (1200Hz - 2200Hz)
                float mod = Mathf.Sin(2f * Mathf.PI * 45f * t) * 300f;
                float squeal = Mathf.Sin(2f * Mathf.PI * (1650f + mod) * t) * 0.4f;
                float noise = (Random.value * 2f - 1f) * 0.3f;

                samples[i] = (squeal + noise) * 0.28f;
            }

            cachedSkidClip = AudioClip.Create("ProceduralTireSkid", numSamples, 1, sampleRate, false);
            cachedSkidClip.SetData(samples, 0);
            return cachedSkidClip;
        }

        /// <summary>
        /// Automatically attaches and wires a 3D procedural engine audio source to the given vehicle.
        /// </summary>
        public static void AttachEngineAudio(VehicleController vehicle)
        {
            if (vehicle == null) return;

            AudioSource source = vehicle.GetComponent<AudioSource>();
            if (source == null) source = vehicle.gameObject.AddComponent<AudioSource>();

            source.clip = GetOrCreateEngineClip();
            source.loop = true;
            source.playOnAwake = true;
            source.spatialBlend = 0.85f; // 3D spatialized
            source.minDistance = 3f;
            source.maxDistance = 60f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.volume = 0.65f;
            source.pitch = 0.85f;

            if (Application.isPlaying)
            {
                source.Play();
            }

            vehicle.SetEngineAudio(source);
        }
    }
}

