using System;
using System.Collections.Generic;
using UnityEngine;
using ThegameioRacing.Vehicle;

namespace ThegameioRacing.Race
{
    [Serializable]
    public class RacerProgress
    {
        public VehicleController vehicle;
        public int currentLap = 1;
        public int nextCheckpointIndex = 0;
        public float currentLapTime = 0f;
        public float bestLapTime = float.MaxValue;
        public float totalRaceTime = 0f;
        public bool isFinished = false;
        public int finalRank = 0;
        public float raceProgressScore = 0f;
        public bool isWrongWay = false;
    }

    /// <summary>
    /// Coordinates track progression, anti-cheat checkpoint sequencing,
    /// split timings, lap completion, and real-time racer position ranking.
    /// </summary>
    public class CheckpointManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int totalLaps = 2;
        [SerializeField] private List<Checkpoint> checkpoints = new List<Checkpoint>();

        private readonly Dictionary<VehicleController, RacerProgress> racerProgressMap = new Dictionary<VehicleController, RacerProgress>();
        private readonly List<RacerProgress> leaderboard = new List<RacerProgress>();
        private int finishedRacersCount = 0;

        public event Action<VehicleController, int> OnCheckpointPassed;
        public event Action<VehicleController, int, float> OnLapCompleted;
        public event Action<VehicleController, int, float> OnRaceFinished;
        public event Action<VehicleController, bool> OnWrongWayChanged;

        public int TotalLaps => totalLaps;
        public int CheckpointCount => checkpoints.Count;
        public IReadOnlyList<RacerProgress> Leaderboard => leaderboard;

        public void SetTotalLaps(int laps)
        {
            totalLaps = Mathf.Max(1, laps);
        }

        public void SetCheckpoints(List<Checkpoint> newCheckpoints)
        {
            checkpoints = newCheckpoints;
            for (int i = 0; i < checkpoints.Count; i++)
            {
                bool isFinish = (i == 0);
                checkpoints[i].Initialize(i, isFinish, this);
            }
        }

        public void RegisterRacer(VehicleController vehicle)
        {
            if (vehicle == null || racerProgressMap.ContainsKey(vehicle)) return;

            RacerProgress progress = new RacerProgress
            {
                vehicle = vehicle,
                currentLap = 1,
                nextCheckpointIndex = 1,
                currentLapTime = 0f,
                bestLapTime = float.MaxValue,
                totalRaceTime = 0f,
                isFinished = false,
                finalRank = 0
            };

            racerProgressMap[vehicle] = progress;
            leaderboard.Add(progress);
        }

        public void UnregisterRacer(VehicleController vehicle)
        {
            if (vehicle == null) return;
            if (racerProgressMap.TryGetValue(vehicle, out RacerProgress p))
            {
                racerProgressMap.Remove(vehicle);
                leaderboard.Remove(p);
            }
        }

        public RacerProgress GetRacerProgress(VehicleController vehicle)
        {
            if (vehicle != null && racerProgressMap.TryGetValue(vehicle, out RacerProgress p))
            {
                return p;
            }
            return null;
        }

        public void UpdateTimers(float deltaTime, bool raceActive)
        {
            if (!raceActive) return;

            for (int i = 0; i < leaderboard.Count; i++)
            {
                RacerProgress p = leaderboard[i];
                if (!p.isFinished)
                {
                    p.currentLapTime += deltaTime;
                    p.totalRaceTime += deltaTime;
                }
            }

            UpdateRacePositions();
        }

        public void OnVehicleHitCheckpoint(VehicleController vehicle, Checkpoint cp, bool isWrongWay)
        {
            if (vehicle == null || cp == null) return;
            if (!racerProgressMap.TryGetValue(vehicle, out RacerProgress p)) return;
            if (p.isFinished) return;

            // Handle wrong way warning state
            if (p.isWrongWay != isWrongWay)
            {
                p.isWrongWay = isWrongWay;
                OnWrongWayChanged?.Invoke(vehicle, isWrongWay);
            }

            if (isWrongWay) return; // Do not advance if driving backwards through trigger

            int cpIndex = cp.Index;

            // Normal progression check
            if (cpIndex == p.nextCheckpointIndex)
            {
                OnCheckpointPassed?.Invoke(vehicle, cpIndex);

                if (vehicle.CompareTag("Player"))
                {
                    AudioClip chime = ThegameioRacing.Audio.ProceduralAudio.CreateCheckpointChimeClip();
                    AudioSource.PlayClipAtPoint(chime, cp.transform.position, 0.85f);
                }

                // If hit finish line as the target checkpoint, complete the lap
                if (cp.IsFinishLine)
                {
                    CompleteLap(vehicle, p);
                }

                // Advance sequentially to next checkpoint (wraps 1 -> ... -> 11 -> 0 -> 1)
                p.nextCheckpointIndex = (p.nextCheckpointIndex + 1) % checkpoints.Count;
            }
        }

        private void CompleteLap(VehicleController vehicle, RacerProgress p)
        {
            float completedLapTime = p.currentLapTime;
            if (completedLapTime < p.bestLapTime)
            {
                p.bestLapTime = completedLapTime;
            }

            OnLapCompleted?.Invoke(vehicle, p.currentLap, completedLapTime);

            p.currentLap++;
            p.currentLapTime = 0f;

            if (p.currentLap > totalLaps)
            {
                p.isFinished = true;
                finishedRacersCount++;
                p.finalRank = finishedRacersCount;
                OnRaceFinished?.Invoke(vehicle, p.finalRank, p.totalRaceTime);
            }
        }

        private void UpdateRacePositions()
        {
            if (checkpoints.Count == 0) return;

            for (int i = 0; i < leaderboard.Count; i++)
            {
                RacerProgress p = leaderboard[i];
                if (p.vehicle == null) continue;

                if (p.isFinished)
                {
                    // Finished racers keep their lock rank
                    p.raceProgressScore = 1000000f - p.finalRank;
                    continue;
                }

                Checkpoint targetCp = checkpoints[p.nextCheckpointIndex];
                float distToTarget = Vector3.Distance(p.vehicle.transform.position, targetCp.transform.position);

                int effectiveCpProgress = (p.nextCheckpointIndex == 0) ? checkpoints.Count : p.nextCheckpointIndex;
                // Score formula: Higher is further ahead
                p.raceProgressScore = (p.currentLap * 10000f) + (effectiveCpProgress * 100f) - Mathf.Clamp(distToTarget, 0f, 500f);
            }

            // Sort descending by score
            leaderboard.Sort((a, b) => b.raceProgressScore.CompareTo(a.raceProgressScore));
        }

        public int GetVehiclePosition(VehicleController vehicle)
        {
            for (int i = 0; i < leaderboard.Count; i++)
            {
                if (leaderboard[i].vehicle == vehicle)
                {
                    return i + 1;
                }
            }
            return 1;
        }

        public Checkpoint GetNextCheckpoint(VehicleController vehicle)
        {
            if (vehicle != null && racerProgressMap.TryGetValue(vehicle, out RacerProgress p))
            {
                if (checkpoints.Count > 0 && p.nextCheckpointIndex < checkpoints.Count)
                {
                    return checkpoints[p.nextCheckpointIndex];
                }
            }
            return null;
        }
    }
}
