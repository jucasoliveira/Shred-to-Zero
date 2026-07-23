using System;
using UnityEngine;
using TMPro;

namespace ShredToZero.Game
{
    /// <summary>
    /// The bomb. A single ticking countdown shown at the top of the screen — the game's
    /// "COUNT DOWN". Reach zero and it detonates (lose). Reach the bomb and call
    /// <see cref="Disarm"/> to win.
    ///
    /// Day 2 keeps this as a plain real-time timer (the design doc's fallback), fully
    /// decoupled from the music so the loop is shippable. It exposes hooks so a miss/combo
    /// system can later add or subtract time.
    /// </summary>
    public class BombTimer : MonoBehaviour
    {
        [Header("Timer")]
        [Tooltip("Starting time on the bomb, in seconds.")]
        public float startSeconds = 90f;

        [Tooltip("Tick the clock down automatically. Turn off to freeze during intros/menus.")]
        public bool running = true;

        [Header("Display (optional)")]
        [Tooltip("Assign a TextMeshpro label to show the timer. If empty, a fallback readout is drawn on screen.")]
        public TMP_Text label;
        [Tooltip("Below this many seconds the label turns red and the panic escalation kicks in (Day 4 juice).")]
        public float panicThreshold = 10f;

        public float TimeRemaining { get; private set; }
        public bool IsDisarmed { get; private set; }
        public bool HasDetonated { get; private set; }

        /// <summary>Fired once when the timer hits zero (lose).</summary>
        public event Action OnDetonated;
        /// <summary>Fired once when the bomb is disarmed (win).</summary>
        public event Action OnDisarmed;

        private void Awake()
        {
            TimeRemaining = startSeconds;
        }

        private void Update()
        {
            if (!running || IsDisarmed || HasDetonated) return;

            TimeRemaining -= Time.deltaTime;
            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                Detonate();
            }

            UpdateLabel();
        }

        /// <summary>Add (or, with a negative value, subtract) seconds. A missed note could call AddTime(-2).</summary>
        public void AddTime(float seconds)
        {
            if (IsDisarmed || HasDetonated) return;
            TimeRemaining = Mathf.Max(0f, TimeRemaining + seconds);
        }

        /// <summary>Win: the player reached the bomb and defused it.</summary>
        public void Disarm()
        {
            if (IsDisarmed || HasDetonated) return;
            IsDisarmed = true;
            running = false;
            OnDisarmed?.Invoke();
            Debug.Log("BOMB DISARMED — hostages rescued!");
        }

        private void Detonate()
        {
            HasDetonated = true;
            OnDetonated?.Invoke();
            Debug.Log("BOOM. Run over.");
        }

        private void UpdateLabel()
        {
            if (label == null) return;
            label.text = FormatTime(TimeRemaining);
            label.color = TimeRemaining <= panicThreshold ? Color.red : Color.white;
        }

        private static string FormatTime(float t)
        {
            int minutes = Mathf.FloorToInt(t / 60f);
            float seconds = t - minutes * 60f;
            return $"{minutes:0}:{seconds:00.0}";
        }

        // Fallback on-screen readout so you can see the timer before wiring any UI.
        private void OnGUI()
        {
            if (label != null) return; // a real label is assigned; no need for the debug draw

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 40,
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold
            };
            style.normal.textColor = TimeRemaining <= panicThreshold ? Color.red : Color.white;

            string text = IsDisarmed ? "DISARMED" : HasDetonated ? "BOOM" : FormatTime(TimeRemaining);
            GUI.Label(new Rect(0, 10, Screen.width, 60), text, style);
        }
    }
}
