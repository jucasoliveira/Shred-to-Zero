using UnityEngine;

namespace ShredToZero.Game
{
    /// <summary>
    /// The "get to the bomb and defuse it" objective. A trigger around the bomb: while the
    /// player stands inside it, a disarm meter fills; when full, the bomb is defused (win).
    /// By default you must clear every goon first, so the room fight actually matters.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DisarmZone : MonoBehaviour
    {
        [Header("References (auto-found if empty)")]
        public BombTimer bomb;

        [Header("Rules")]
        [Tooltip("Seconds the player must stand on the bomb to defuse it.")]
        public float disarmSeconds = 3f;
        [Tooltip("Require every enemy defeated before the bomb can be disarmed.")]
        public bool requireAllEnemiesDefeated = true;

        private bool _playerInside;
        private float _progress;

        private void Reset()
        {
            // Make the attached collider a trigger the moment the component is added.
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Awake()
        {
            if (bomb == null) bomb = GetComponentInParent<BombTimer>();
            if (bomb == null) bomb = FindFirstObjectByType<BombTimer>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) _playerInside = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInside = false;
                _progress = 0f; // step off and the defuse resets — no cheesing it in bits
            }
        }

        private void Update()
        {
            if (!_playerInside || bomb == null || bomb.IsDisarmed || bomb.HasDetonated) return;
            if (requireAllEnemiesDefeated && RunManager.Instance != null && !RunManager.Instance.AllEnemiesDefeated)
                return;

            _progress += Time.deltaTime;
            if (_progress >= disarmSeconds)
                bomb.Disarm();
        }

        private float Progress01 => Mathf.Clamp01(_progress / disarmSeconds);

        private bool Blocked =>
            requireAllEnemiesDefeated && RunManager.Instance != null && !RunManager.Instance.AllEnemiesDefeated;

        private void OnGUI()
        {
            if (!_playerInside || bomb == null || bomb.IsDisarmed || bomb.HasDetonated) return;

            var style = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter };

            if (Blocked)
            {
                style.normal.textColor = new Color(1f, 0.6f, 0.3f);
                GUI.Label(new Rect(0, Screen.height - 90, Screen.width, 30),
                          $"Clear the goons first! ({RunManager.Instance.AliveEnemies} left)", style);
                return;
            }

            // Simple disarm progress bar near the bottom.
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(0, Screen.height - 90, Screen.width, 30), "DEFUSING…", style);

            float w = 300f, h = 18f;
            float x = (Screen.width - w) / 2f, y = Screen.height - 55f;
            GUI.color = new Color(1, 1, 1, 0.25f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
            GUI.color = new Color(0.4f, 1f, 0.5f);
            GUI.DrawTexture(new Rect(x, y, w * Progress01, h), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
