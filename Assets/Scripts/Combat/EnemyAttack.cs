using UnityEngine;
using ShredToZero.Rhythm;

namespace ShredToZero.Combat
{
    /// <summary>
    /// Makes a goon shoot at the player.
    ///
    /// By default it fires ON THE BEAT (every N beats, driven by the Conductor). That does
    /// two lovely things: the room's gunfire becomes part of the music, and it teaches the
    /// player where the beat is — which is exactly the beat they want to fire on for their
    /// own empowered notes. If there's no Conductor in the scene it falls back to a plain
    /// timed interval so it still works.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class EnemyAttack : MonoBehaviour
    {
        [Header("Projectile")]
        public EnemyProjectile projectilePrefab;
        [Tooltip("Where the shot spawns. Leave empty to fire from the enemy's centre.")]
        public Transform muzzle;

        [Header("Attack")]
        public float damage = 10f;
        public float projectileSpeed = 5f;
        [Tooltip("Only shoots when the player is within this distance.")]
        public float range = 9f;

        [Header("Timing")]
        [Tooltip("Fire in time with the song. Turn off for a plain cooldown.")]
        public bool fireOnBeat = true;
        [Tooltip("Fire every N beats. 2 = every other beat, 4 = once a bar.")]
        public int beatsBetweenShots = 2;
        [Tooltip("Seconds between shots when NOT firing on the beat (or if no Conductor exists).")]
        public float fireInterval = 2f;

        [Header("Debug")]
        public bool verboseLogs = false;

        private Transform _player;
        private float _lastFireTime = -999f;
        private int _beatCounter;
        private bool _subscribed;

        private void Start()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) _player = playerObj.transform;

            if (fireOnBeat && Conductor.Instance != null)
            {
                Conductor.Instance.OnBeat += HandleBeat;
                _subscribed = true;
            }
        }

        private void OnDestroy()
        {
            if (_subscribed && Conductor.Instance != null)
                Conductor.Instance.OnBeat -= HandleBeat;
        }

        private void HandleBeat(int beat)
        {
            _beatCounter++;
            if (beatsBetweenShots <= 0 || _beatCounter % beatsBetweenShots != 0) return;
            TryFire();
        }

        private void Update()
        {
            // Fallback path: no beat subscription (no Conductor, or fireOnBeat off).
            if (_subscribed) return;
            if (Time.time - _lastFireTime < fireInterval) return;
            TryFire();
        }

        private void TryFire()
        {
            if (projectilePrefab == null || _player == null) return;

            Vector2 toPlayer = (Vector2)_player.position - (Vector2)transform.position;
            if (toPlayer.magnitude > range) return;

            _lastFireTime = Time.time;

            Vector3 origin = muzzle != null ? muzzle.position : transform.position;
            EnemyProjectile shot = Instantiate(projectilePrefab, origin, Quaternion.identity);
            shot.Fire(toPlayer.normalized, damage, projectileSpeed);

            if (verboseLogs) Debug.Log($"[{name}] fired at the player.", this);
        }
    }
}
