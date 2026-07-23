using System;
using UnityEngine;
using UnityEngine.InputSystem;
using ShredToZero.Combat;
using ShredToZero.Rhythm;

namespace ShredToZero.Player
{
    /// <summary>
    /// The guitar. Each fire key plays a different note TYPE, launched from the player's
    /// muzzle toward the aim direction. If the key is pressed ON the beat (per the
    /// Conductor), the note is EMPOWERED: bonus damage + a faster, bigger, brighter shot.
    /// That on-beat bonus is where rhythm lives in this design.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class GuitarWeapon : MonoBehaviour
    {
        [Serializable]
        public struct FireBinding
        {
            public Key key;
            public NoteType type;
        }

        [Header("Projectile")]
        public NoteProjectile notePrefab;

        [Header("Damage")]
        public float baseDamage = 10f;
        [Tooltip("Damage multiplier when the shot lands on the beat.")]
        public float onBeatBonus = 2f;

        [Header("On-beat window")]
        [Tooltip("How many milliseconds around a beat still counts as 'on beat'. Generous is good.")]
        public float onBeatWindowMs = 110f;

        [Header("Fire rate")]
        [Tooltip("Minimum seconds between shots, so a held/mashed key can't spray infinitely.")]
        public float fireCooldown = 0.12f;

        [Header("Debug")]
        [Tooltip("Print firing diagnostics to the Console. Turn off once it feels right.")]
        public bool verboseLogs = true;

        [Header("Bindings (key → note type)")]
        public FireBinding[] bindings =
        {
            new FireBinding { key = Key.Digit1, type = NoteType.Power },
            new FireBinding { key = Key.Digit2, type = NoteType.Bass  },
            new FireBinding { key = Key.Digit3, type = NoteType.Lead  },
        };

        /// <summary>Fires on every shot: (type, wasOnBeat). Combo/juice/score subscribe here.</summary>
        public event Action<NoteType, bool> OnFired;

        private PlayerController _player;
        private float _lastFireTime = -999f;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
        }

        private void Update()
        {
            var k = Keyboard.current;
            if (k == null) return;
            if (Time.time - _lastFireTime < fireCooldown) return;

            foreach (var binding in bindings)
            {
                if (k[binding.key].wasPressedThisFrame)
                {
                    if (verboseLogs) Debug.Log($"[Guitar] Key '{binding.key}' detected → firing {binding.type}", this);
                    Fire(binding.type);
                    break; // one note per frame
                }
            }
        }

        private void Fire(NoteType type)
        {
            if (notePrefab == null)
            {
                Debug.LogWarning("[GuitarWeapon] No Note Prefab assigned — drag the NoteProjectile prefab into the 'Note Prefab' field.", this);
                return;
            }
            if (_player.muzzle == null)
            {
                Debug.LogWarning("[GuitarWeapon] PlayerController has no Muzzle assigned — drag the child 'Muzzle' object into PlayerController's 'Muzzle' field.", this);
                return;
            }

            _lastFireTime = Time.time;

            bool onBeat = Conductor.Instance != null && Conductor.Instance.IsOnBeat(onBeatWindowMs);
            float damage = baseDamage * (onBeat ? onBeatBonus : 1f);

            NoteProjectile note = Instantiate(notePrefab, _player.muzzle.position, Quaternion.identity);
            note.Fire(type, _player.AimDirection, damage, onBeat);

            if (verboseLogs)
            {
                string target = _player.CurrentTarget != null ? _player.CurrentTarget.name : "(cursor, no target)";
                Debug.Log($"[Guitar] Fired {type} | onBeat={onBeat} | dmg={damage:0.#} → {target}", this);
            }

            OnFired?.Invoke(type, onBeat);
        }
    }
}
