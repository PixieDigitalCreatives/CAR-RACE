using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThegameioRacing.Vehicle;
using ThegameioRacing.Input;
using ThegameioRacing.UI;
using ThegameioRacing.AI;

namespace ThegameioRacing.Race
{
    public enum RaceState
    {
        PreRace,
        Countdown,
        Racing,
        Paused,
        Finished
    }

    /// <summary>
    /// Master race director managing state transitions, countdown sequences,
    /// grid vehicle spawning, timers, pause controls, and results handoff.
    /// </summary>
    public class RaceManager : MonoBehaviour
    {
        public static RaceManager Instance { get; private set; }

        [Header("Race Configuration")]
        [SerializeField] private int totalLaps = 2;
        [SerializeField] private float countdownDuration = 3f;

        [Header("Scene References")]
        [SerializeField] private CheckpointManager checkpointManager;
        [SerializeField] private VehicleController playerVehicle;
        [SerializeField] private List<VehicleController> aiVehicles = new List<VehicleController>();

        private RaceState currentState = RaceState.PreRace;
        private float countdownTimer = 0f;

        public event Action<RaceState> OnStateChanged;
        public event Action<int> OnCountdownTick; // 3, 2, 1, 0 (GO)
        public event Action<RacerProgress> OnPlayerFinished;

        public RaceState CurrentState => currentState;
        public CheckpointManager Checkpoints => checkpointManager;
        public VehicleController PlayerVehicle => playerVehicle;
        public int TotalLaps => totalLaps;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (checkpointManager == null)
            {
                checkpointManager = GetComponentInChildren<CheckpointManager>() ?? FindFirstObjectByType<CheckpointManager>();
            }
        }

        private void Start()
        {
            InitializeRace();
        }

        public void RegisterPlayer(VehicleController player)
        {
            playerVehicle = player;
            if (checkpointManager != null && player != null)
            {
                checkpointManager.RegisterRacer(player);
            }
        }

        public void RegisterAI(VehicleController ai)
        {
            if (ai != null && !aiVehicles.Contains(ai))
            {
                aiVehicles.Add(ai);
                if (checkpointManager != null)
                {
                    checkpointManager.RegisterRacer(ai);
                }
            }
        }

        public void InitializeRace()
        {
            SetState(RaceState.PreRace);

            // Register any already present vehicles
            if (playerVehicle != null) checkpointManager?.RegisterRacer(playerVehicle);
            foreach (var ai in aiVehicles)
            {
                if (ai != null) checkpointManager?.RegisterRacer(ai);
            }

            // Lock all vehicle inputs
            LockAllVehicles(true);

            if (checkpointManager != null)
            {
                checkpointManager.OnRaceFinished += HandleVehicleFinished;
            }

            StartCoroutine(StartCountdownRoutine());
        }

        private void OnDestroy()
        {
            if (checkpointManager != null)
            {
                checkpointManager.OnRaceFinished -= HandleVehicleFinished;
            }
        }

        private void Update()
        {
            if (currentState == RaceState.Racing)
            {
                checkpointManager?.UpdateTimers(Time.deltaTime, true);

                // Check for pause shortcut (Escape / P)
                if (UnityEngine.Input.GetKeyDown(KeyCode.Escape) || UnityEngine.Input.GetKeyDown(KeyCode.P))
                {
                    TogglePause();
                }
            }
        }

        private IEnumerator StartCountdownRoutine()
        {
            yield return new WaitForSeconds(0.8f);
            SetState(RaceState.Countdown);

            countdownTimer = countdownDuration;
            int lastSecond = -1;

            while (countdownTimer > 0f)
            {
                int currentSecond = Mathf.CeilToInt(countdownTimer);
                if (currentSecond != lastSecond)
                {
                    lastSecond = currentSecond;
                    OnCountdownTick?.Invoke(currentSecond);
                }

                countdownTimer -= Time.deltaTime;
                yield return null;
            }

            // GO!
            OnCountdownTick?.Invoke(0);
            yield return new WaitForSeconds(0.4f);

            SetState(RaceState.Racing);
            LockAllVehicles(false);
        }

        private void LockAllVehicles(bool locked)
        {
            if (playerVehicle != null)
            {
                VehicleInputReader input = playerVehicle.GetComponent<VehicleInputReader>();
                if (input != null) input.SetInputEnabled(!locked);
            }

            foreach (var ai in aiVehicles)
            {
                if (ai != null)
                {
                    VehicleInputReader input = ai.GetComponent<VehicleInputReader>();
                    if (input != null) input.SetInputEnabled(!locked);

                    AIRacingController aiInput = ai.GetComponent<AIRacingController>();
                    if (aiInput != null) aiInput.SetInputEnabled(!locked);
                }
            }
        }

        private void HandleVehicleFinished(VehicleController vehicle, int rank, float time)
        {
            if (vehicle == playerVehicle)
            {
                SetState(RaceState.Finished);
                RacerProgress p = checkpointManager?.GetRacerProgress(vehicle);
                OnPlayerFinished?.Invoke(p);
            }
        }

        public void SetState(RaceState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        public void TogglePause()
        {
            if (currentState == RaceState.Racing)
            {
                SetState(RaceState.Paused);
                Time.timeScale = 0f;
            }
            else if (currentState == RaceState.Paused)
            {
                SetState(RaceState.Racing);
                Time.timeScale = 1f;
            }
        }

        public void RestartRace()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadScene(string sceneName)
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}
