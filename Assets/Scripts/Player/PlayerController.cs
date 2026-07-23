using UnityEngine;
using UnityEngine.InputSystem;
using ShredToZero.Combat;

namespace ShredToZero.Player
{
    /// <summary>
    /// Diablo-style top-down control:
    ///   • Hold LEFT MOUSE to walk toward the cursor (release to stop where you are).
    ///   • The enemy under/near the cursor becomes the current TARGET.
    ///   • Firing (keys 1/2/3) is handled by GuitarWeapon and aims at that target — or
    ///     at the bare cursor point if no enemy is under it.
    ///
    /// Uses the Input System's direct device polling so there's no action-asset wiring.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 6f;
        [Tooltip("Stop when within this distance of the click point, so we don't jitter on arrival.")]
        public float stopDistance = 0.15f;

        [Header("Aiming / targeting")]
        [Tooltip("Child transform at the guitar's tip. Notes spawn here and it rotates toward the target.")]
        public Transform muzzle;
        [Tooltip("How close the cursor must be to an enemy to lock it as the target. Bigger = more forgiving.")]
        public float targetRadius = 0.7f;

        // --- Public state the weapon reads ---------------------------------------

        /// <summary>Mouse position in world space, on the 2D plane.</summary>
        public Vector2 CursorWorld { get; private set; }

        /// <summary>Enemy currently under/near the cursor, or null.</summary>
        public Enemy CurrentTarget { get; private set; }

        /// <summary>The point notes should be aimed at: the target if there is one, else the cursor.</summary>
        public Vector2 AimPoint { get; private set; }

        /// <summary>Unit vector from the player toward the aim point.</summary>
        public Vector2 AimDirection { get; private set; } = Vector2.right;

        // --- Internals -----------------------------------------------------------

        private Rigidbody2D _rb;
        private Camera _cam;
        private Vector2 _destination;
        private bool _hasDestination;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;       // top-down: no gravity
            _rb.freezeRotation = true;   // we aim the muzzle, not the body
            _cam = Camera.main;
            _destination = _rb.position;
        }

        private void Update()
        {
            UpdateCursorWorld();
            AcquireTarget();
            UpdateAim();
            ReadMoveInput();
        }

        private void FixedUpdate()
        {
            MoveTowardDestination();
        }

        private void UpdateCursorWorld()
        {
            if (_cam == null || Mouse.current == null) return;

            Vector3 screen = Mouse.current.position.ReadValue();
            screen.z = -_cam.transform.position.z;        // distance from camera to the 2D plane
            CursorWorld = _cam.ScreenToWorldPoint(screen);
        }

        private void AcquireTarget()
        {
            // Find the nearest enemy within targetRadius of the cursor.
            Collider2D[] hits = Physics2D.OverlapCircleAll(CursorWorld, targetRadius);
            Enemy best = null;
            float bestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                var enemy = hit.GetComponentInParent<Enemy>();
                if (enemy == null) continue;

                float d = Vector2.Distance(CursorWorld, enemy.transform.position);
                if (d < bestDist) { bestDist = d; best = enemy; }
            }

            CurrentTarget = best;
        }

        private void UpdateAim()
        {
            AimPoint = CurrentTarget != null ? (Vector2)CurrentTarget.transform.position : CursorWorld;

            Vector2 dir = AimPoint - _rb.position;
            if (dir.sqrMagnitude > 0.0001f) AimDirection = dir.normalized;

            if (muzzle != null)
            {
                float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
                muzzle.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private void ReadMoveInput()
        {
            // Hold left mouse to keep walking toward the cursor (classic ARPG move).
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                _destination = CursorWorld;
                _hasDestination = true;
            }
        }

        private void MoveTowardDestination()
        {
            if (!_hasDestination)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 toDest = _destination - _rb.position;
            if (toDest.magnitude <= stopDistance)
            {
                _rb.linearVelocity = Vector2.zero;
                _hasDestination = false;
                return;
            }

            _rb.linearVelocity = toDest.normalized * moveSpeed;
        }
    }
}
