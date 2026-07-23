using UnityEngine;
using UnityEngine.InputSystem;

namespace ShredToZero.Player
{
    /// <summary>
    /// Top-down player: WASD/arrows to move, mouse to aim. It rotates a child "muzzle"
    /// transform to point at the cursor; the GuitarWeapon fires notes out of that muzzle.
    ///
    /// Uses the Input System's direct device polling (Keyboard.current / Mouse.current)
    /// so there's no action-asset wiring to do for the jam.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 6f;

        [Header("Aiming")]
        [Tooltip("Child transform at the guitar's tip. Notes spawn here and it rotates toward the cursor.")]
        public Transform muzzle;

        /// <summary>Current aim direction (unit vector from player toward the cursor).</summary>
        public Vector2 AimDirection { get; private set; } = Vector2.right;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private Camera _cam;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;              // top-down: no gravity
            _rb.freezeRotation = true;          // we aim the muzzle, not the body
            _cam = Camera.main;
        }

        private void Update()
        {
            ReadMoveInput();
            UpdateAim();
        }

        private void FixedUpdate()
        {
            _rb.linearVelocity = _moveInput * moveSpeed;
        }

        private void ReadMoveInput()
        {
            var k = Keyboard.current;
            if (k == null) { _moveInput = Vector2.zero; return; }

            float x = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1f : 0f)
                    - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1f : 0f);
            float y = (k.wKey.isPressed || k.upArrowKey.isPressed ? 1f : 0f)
                    - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1f : 0f);

            _moveInput = new Vector2(x, y).normalized; // normalize so diagonals aren't faster
        }

        private void UpdateAim()
        {
            if (_cam == null || Mouse.current == null) return;

            Vector3 mouseScreen = Mouse.current.position.ReadValue();
            mouseScreen.z = -_cam.transform.position.z; // distance from camera to the 2D plane
            Vector3 mouseWorld = _cam.ScreenToWorldPoint(mouseScreen);

            Vector2 dir = ((Vector2)mouseWorld - (Vector2)transform.position);
            if (dir.sqrMagnitude > 0.0001f) AimDirection = dir.normalized;

            if (muzzle != null)
            {
                float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
                muzzle.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }
}
