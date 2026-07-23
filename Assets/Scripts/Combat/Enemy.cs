using System;
using UnityEngine;

namespace ShredToZero.Combat
{
    /// <summary>
    /// A building goon. Has health and a note affinity: it takes bonus damage from the
    /// note it's WEAK to and reduced damage from the note it RESISTS. That's what makes
    /// the player read each enemy and pick the right note/riff instead of spamming one.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Enemy : MonoBehaviour
    {
        [Header("Health")]
        public float maxHealth = 30f;

        [Header("Affinity")]
        [Tooltip("Takes bonus damage from this note type.")]
        public NoteType weakTo = NoteType.Power;
        [Tooltip("Damage multiplier when hit by its weakness (e.g. 2 = double).")]
        public float weakMultiplier = 2f;

        [Tooltip("Takes reduced damage from this note type.")]
        public NoteType resistTo = NoteType.Bass;
        [Tooltip("Damage multiplier when hit by what it resists (e.g. 0.5 = half).")]
        public float resistMultiplier = 0.5f;

        [Header("Movement (optional Day-2 life)")]
        [Tooltip("If true, drifts toward the player so the room isn't static.")]
        public bool chasePlayer = true;
        public float moveSpeed = 1.5f;

        [Header("Feedback")]
        [Tooltip("Tinted to show its weakness colour so players can read it at a glance.")]
        public bool tintToWeakness = true;

        /// <summary>Fired when this enemy dies. RunManager/riff-drop logic hooks here on Day 3.</summary>
        public event Action<Enemy> OnDied;

        public float Health { get; private set; }

        private SpriteRenderer _sprite;
        private Transform _player;
        private Rigidbody2D _rb;

        private void Awake()
        {
            Health = maxHealth;
            _sprite = GetComponentInChildren<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();

            if (tintToWeakness && _sprite != null)
                _sprite.color = weakTo.ToColor();

            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) _player = playerObj.transform;
        }

        private void FixedUpdate()
        {
            if (!chasePlayer || _player == null) return;

            Vector2 toPlayer = ((Vector2)_player.position - (Vector2)transform.position).normalized;
            Vector2 step = toPlayer * (moveSpeed * Time.fixedDeltaTime);

            if (_rb != null) _rb.MovePosition(_rb.position + step);
            else transform.position += (Vector3)step;
        }

        /// <summary>
        /// Apply incoming damage of a given note type, scaled by this enemy's affinity.
        /// Returns the actual damage dealt (handy for damage numbers later).
        /// </summary>
        public float TakeDamage(NoteType type, float rawDamage)
        {
            float multiplier = 1f;
            if (type == weakTo) multiplier = weakMultiplier;
            else if (type == resistTo) multiplier = resistMultiplier;

            float dealt = rawDamage * multiplier;
            Health -= dealt;

            FlashHit(multiplier);

            if (Health <= 0f) Die();
            return dealt;
        }

        private void FlashHit(float multiplier)
        {
            if (_sprite == null) return;
            // Bright white on a super-effective hit, dim on a resisted one — instant feedback.
            Color baseColor = tintToWeakness ? weakTo.ToColor() : Color.white;
            _sprite.color = multiplier > 1f ? Color.white
                          : multiplier < 1f ? baseColor * 0.5f
                          : baseColor;
            CancelInvoke(nameof(RestoreColor));
            Invoke(nameof(RestoreColor), 0.08f);
        }

        private void RestoreColor()
        {
            if (_sprite != null)
                _sprite.color = tintToWeakness ? weakTo.ToColor() : Color.white;
        }

        private void Die()
        {
            OnDied?.Invoke(this);
            // Day 3 hook: roll a riff drop here before destroying.
            Destroy(gameObject);
        }
    }
}
