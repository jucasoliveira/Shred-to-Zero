using UnityEngine;

namespace ShredToZero.Audio
{
    /// <summary>
    /// Central one-shot SFX player. A small pool of AudioSources plays overlapping sounds
    /// without cutting each other off (a laser and an explosion can ring at once).
    ///
    /// Everything is routed through the null-safe static <see cref="Play"/>, so gameplay
    /// code can fire sounds without caring whether a manager or clip exists yet — perfect
    /// for building the hooks before you've sourced a single asset.
    ///
    /// Music is NOT handled here — the Conductor owns the song so it stays sample-synced.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Mix")]
        [Range(0f, 1f)] public float masterSfxVolume = 1f;

        [Header("Pool")]
        [Tooltip("How many sounds can overlap at once before the oldest is reused.")]
        public int voices = 12;

        private AudioSource[] _pool;
        private int _next;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _pool = new AudioSource[Mathf.Max(1, voices)];
            for (int i = 0; i < _pool.Length; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f; // 2D — top-down arena doesn't need positional audio
                _pool[i] = src;
            }
        }

        /// <summary>
        /// Play a one-shot. Null-safe: does nothing if there's no manager or no clip.
        /// </summary>
        /// <param name="clip">The sound to play (may be null).</param>
        /// <param name="volume">Per-sound volume (0–1), multiplied by master.</param>
        /// <param name="pitchVariance">Randomizes pitch by ±this amount so repeats don't sound robotic.</param>
        public static void Play(AudioClip clip, float volume = 1f, float pitchVariance = 0.05f)
        {
            if (Instance == null || clip == null) return;
            Instance.PlayInternal(clip, volume, pitchVariance);
        }

        private void PlayInternal(AudioClip clip, float volume, float pitchVariance)
        {
            AudioSource src = _pool[_next];
            _next = (_next + 1) % _pool.Length;

            src.clip = clip;
            src.volume = Mathf.Clamp01(volume) * masterSfxVolume;
            src.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
            src.Play();
        }
    }
}
