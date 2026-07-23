using UnityEngine;

namespace ShredToZero.Combat
{
    /// <summary>
    /// A note fired from the guitar. It flies in a straight line, and on contact with an
    /// enemy deals its typed damage (the enemy applies weakness/resistance). "Empowered"
    /// notes — fired on the beat — hit harder, fly faster, and look bigger.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class NoteProjectile : MonoBehaviour
    {
        [Header("Debug")]
        [Tooltip("Log when the note overlaps something that isn't an enemy. Noisy — off by default.")]
        public bool verboseLogs = false;

        [Header("Flight")]
        public float speed = 12f;
        [Tooltip("Seconds before the note fizzles if it hits nothing.")]
        public float lifetime = 2f;
        [Tooltip("Extra speed multiplier when the shot was on-beat.")]
        public float empoweredSpeedMultiplier = 1.4f;
        [Tooltip("Visual scale-up when the shot was on-beat.")]
        public float empoweredScale = 1.6f;

        public NoteType Type { get; private set; }
        public float Damage { get; private set; }
        public bool Empowered { get; private set; }

        private Vector2 _direction;
        private SpriteRenderer _sprite;

        /// <summary>Called by the guitar right after Instantiate to arm the note.</summary>
        public void Fire(NoteType type, Vector2 direction, float damage, bool empowered)
        {
            Type = type;
            Damage = damage;
            Empowered = empowered;
            _direction = direction.normalized;

            _sprite = GetComponentInChildren<SpriteRenderer>();
            if (_sprite != null) _sprite.color = type.ToColor();

            if (empowered)
            {
                speed *= empoweredSpeedMultiplier;
                transform.localScale *= empoweredScale;
                if (_sprite != null) _sprite.color = Color.Lerp(type.ToColor(), Color.white, 0.4f);
            }

            // Point the sprite along its travel direction.
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            transform.position += (Vector3)(_direction * (speed * Time.deltaTime));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var enemy = other.GetComponentInParent<Enemy>();
            if (enemy == null)
            {
                if (verboseLogs)
                    Debug.Log($"[Note] {Type} note overlapped '{other.name}' (not an Enemy) — ignoring.", this);
                return; // flew into a wall, the player, or an enemy shot; let lifetime clean it up
            }

            enemy.TakeDamage(Type, Damage);
            Destroy(gameObject);
        }
    }
}
