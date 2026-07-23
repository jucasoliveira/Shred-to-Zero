using UnityEngine;
using ShredToZero.Player;

namespace ShredToZero.Combat
{
    /// <summary>
    /// A goon's shot. Flies in a straight line and hurts the player on contact.
    /// Deliberately slower than your notes, so it's readable and dodgeable — that's what
    /// makes the click-to-move traversal matter.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EnemyProjectile : MonoBehaviour
    {
        [Header("Flight")]
        public float speed = 5f;
        public float lifetime = 4f;

        [Header("Damage")]
        public float damage = 10f;

        [Header("Look")]
        public Color color = new(1f, 0.4f, 0.2f);

        private Vector2 _direction;

        /// <summary>Arm the shot. Called by EnemyAttack right after Instantiate.</summary>
        public void Fire(Vector2 direction, float damage, float speed)
        {
            _direction = direction.normalized;
            this.damage = damage;
            this.speed = speed;

            var sprite = GetComponentInChildren<SpriteRenderer>();
            if (sprite != null) sprite.color = color;

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
            // Ignore other enemies and their shots — only the player matters.
            var health = other.GetComponentInParent<PlayerHealth>();
            if (health == null) return;

            health.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
