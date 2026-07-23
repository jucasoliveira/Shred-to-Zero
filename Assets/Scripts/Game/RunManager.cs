using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using ShredToZero.Combat;
using ShredToZero.Player;

namespace ShredToZero.Game
{
    public enum RunState { Playing, Won, Lost }

    /// <summary>
    /// Orchestrates a single run: watches the bomb, tracks how many goons are left, and
    /// decides win/lose. On the design doc's terms this is the RunManager — for the jam
    /// slice it also draws the end-of-run overlay and handles restart.
    ///
    ///   • Bomb disarmed  → WIN
    ///   • Bomb detonates → LOSE
    ///   • Press R after a run ends → replay the stage
    /// </summary>
    public class RunManager : MonoBehaviour
    {
        public static RunManager Instance { get; private set; }

        [Header("References (auto-found if left empty)")]
        public BombTimer bomb;

        public RunState State { get; private set; } = RunState.Playing;

        /// <summary>How the run ended, for the result screen ("bomb" or "killed").</summary>
        public string LossReason { get; private set; } = "bomb";

        /// <summary>True once every enemy present at run start has been defeated.</summary>
        public bool AllEnemiesDefeated => _aliveEnemies <= 0;
        public int AliveEnemies => _aliveEnemies;

        /// <summary>Fires once when the run ends, passing Won or Lost.</summary>
        public event Action<RunState> OnRunEnded;

        private int _aliveEnemies;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Time.timeScale = 1f; // in case we came back from a frozen end screen
        }

        private void Start()
        {
            // Count and subscribe to every enemy in the scene at run start.
            var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            _aliveEnemies = enemies.Length;
            foreach (var e in enemies) e.OnDied += HandleEnemyDied;

            if (bomb == null) bomb = FindFirstObjectByType<BombTimer>();
            if (bomb != null)
            {
                bomb.OnDetonated += LoseToBomb;
                bomb.OnDisarmed += Win;
            }

            // Getting shot down ends the run too.
            var health = FindFirstObjectByType<PlayerHealth>();
            if (health != null) health.OnDied += LoseToGoons;

            Debug.Log($"[Run] Started — {_aliveEnemies} goons in the building.");
        }

        private void HandleEnemyDied(Enemy _)
        {
            _aliveEnemies = Mathf.Max(0, _aliveEnemies - 1);
            if (AllEnemiesDefeated) Debug.Log("[Run] All goons down — get to the bomb!");
        }

        private void Update()
        {
            if (State != RunState.Playing &&
                Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                Restart();
            }
        }

        private void Win()
        {
            if (State != RunState.Playing) return;
            State = RunState.Won;
            EndRun();
        }

        private void LoseToBomb()
        {
            if (State != RunState.Playing) return;
            LossReason = "bomb";
            State = RunState.Lost;
            EndRun();
        }

        private void LoseToGoons()
        {
            if (State != RunState.Playing) return;
            LossReason = "killed";
            State = RunState.Lost;
            EndRun();
        }

        private void EndRun()
        {
            Time.timeScale = 0f; // freeze the action on the result screen
            OnRunEnded?.Invoke(State);
            Debug.Log($"[Run] Ended: {State}");
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnGUI()
        {
            if (State == RunState.Playing) return;

            // Dim the screen.
            GUI.color = new Color(0, 0, 0, 0.6f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 54,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            var sub = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter
            };

            if (State == RunState.Won)
            {
                title.normal.textColor = new Color(0.4f, 1f, 0.5f);
                GUI.Label(new Rect(0, Screen.height / 2f - 70, Screen.width, 80), "HOSTAGES RESCUED", title);
                sub.normal.textColor = Color.white;
                GUI.Label(new Rect(0, Screen.height / 2f + 10, Screen.width, 40), "You shredded to zero. Press R to run it back.", sub);
            }
            else
            {
                bool killed = LossReason == "killed";
                title.normal.textColor = new Color(1f, 0.4f, 0.3f);
                GUI.Label(new Rect(0, Screen.height / 2f - 70, Screen.width, 80),
                          killed ? "YOU GOT SHREDDED" : "K A B O O M", title);
                sub.normal.textColor = Color.white;
                GUI.Label(new Rect(0, Screen.height / 2f + 10, Screen.width, 40),
                          killed ? "The goons took you down. Press R to try again."
                                 : "The bomb won. Press R to try again.", sub);
            }
        }
    }
}
