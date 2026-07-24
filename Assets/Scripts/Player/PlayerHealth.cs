using System;
using UnityEngine;
using ShredToZero.Audio;

namespace ShredToZero.Player
{
    /// <summary>
    /// The hero's hit points. Brief invulnerability after each hit stops a single goon
    /// from melting you in one burst, and gives that Diablo movement a purpose: dodging.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health")]
        public float maxHealth = 100f;

        [Tooltip("Seconds of invulnerability after taking a hit, so overlapping shots don't stack instantly.")]
        public float invulnSeconds = 0.5f;

        [Header("Feedback")]
        public Color hitFlashColor = new(1f, 0.3f, 0.3f);
        public bool verboseLogs = true;

        [Header("Sound")]
        public AudioClip hurtClip;
        public AudioClip deathClip;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;

        public float Health { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsInvulnerable => Time.time - _lastHitTime < invulnSeconds;

        /// <summary>(current, max) — hook a real health bar here on Day 4.</summary>
        public event Action<float, float> OnHealthChanged;
        /// <summary>Fires once when the player dies. RunManager listens.</summary>
        public event Action OnDied;

        private float _lastHitTime = -999f;
        private SpriteRenderer _sprite;
        private Color _baseColor = Color.white;

        private void Awake()
        {
            Health = maxHealth;
            _sprite = GetComponentInChildren<SpriteRenderer>();
            if (_sprite != null) _baseColor = _sprite.color;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || IsInvulnerable) return;

            _lastHitTime = Time.time;
            Health = Mathf.Max(0f, Health - amount);
            OnHealthChanged?.Invoke(Health, maxHealth);

            if (verboseLogs)
                Debug.Log($"[Player] took {amount:0.#} damage | HP {Health:0.#}/{maxHealth:0.#}", this);

            FlashHit();
            AudioManager.Play(hurtClip, sfxVolume);

            if (Health <= 0f) Die();
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            Health = Mathf.Min(maxHealth, Health + amount);
            OnHealthChanged?.Invoke(Health, maxHealth);
        }

        private void FlashHit()
        {
            if (_sprite == null) return;
            _sprite.color = hitFlashColor;
            CancelInvoke(nameof(RestoreColor));
            Invoke(nameof(RestoreColor), 0.12f);
        }

        private void RestoreColor()
        {
            if (_sprite != null) _sprite.color = _baseColor;
        }

        private void Die()
        {
            IsDead = true;
            if (verboseLogs) Debug.Log("[Player] DOWN — the goons got you.", this);
            AudioManager.Play(deathClip, sfxVolume);
            OnDied?.Invoke();
        }

        // Temporary health bar, bottom-left. Day 4 juice replaces this with real UI.
        private void OnGUI()
        {
            const float w = 260f, h = 20f, pad = 20f;
            float y = Screen.height - pad - h;

            GUI.color = new Color(0, 0, 0, 0.5f);
            GUI.DrawTexture(new Rect(pad, y, w, h), Texture2D.whiteTexture);

            float pct = maxHealth > 0f ? Health / maxHealth : 0f;
            GUI.color = Color.Lerp(new Color(1f, 0.25f, 0.2f), new Color(0.4f, 1f, 0.5f), pct);
            GUI.DrawTexture(new Rect(pad, y, w * pct, h), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var style = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(pad, y, w, h), $"HP {Health:0}/{maxHealth:0}", style);
        }
    }
}
