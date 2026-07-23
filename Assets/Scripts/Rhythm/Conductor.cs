using System;
using UnityEngine;

namespace ShredToZero.Rhythm
{
    /// <summary>
    /// The single source of musical time for the whole game.
    ///
    /// EVERYTHING that needs to know "where are we in the song?" asks the Conductor.
    /// Nothing in the rhythm layer should ever read Time.deltaTime for timing — frame
    /// time drifts from audio and will make your judging feel wrong.
    ///
    /// We sync to <see cref="AudioSettings.dspTime"/>, which is the audio hardware's
    /// sample clock (it advances in lockstep with the samples actually leaving your
    /// speakers). Combined with <see cref="AudioSource.PlayScheduled"/> for a
    /// sample-accurate start, this gives us a rock-steady musical clock. This is the
    /// "sync to samples, not frames" requirement from the design doc.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class Conductor : MonoBehaviour
    {
        public static Conductor Instance { get; private set; }

        [Header("Song")]
        [Tooltip("The song for this stage. Can be left empty to run a silent clock for testing.")]
        public AudioClip song;

        [Tooltip("Beats per minute of the song. MUST match the track or nothing lines up.")]
        public float bpm = 120f;

        [Tooltip("Seconds of silence/intro before beat 0 lands. If your song has a lead-in, put it here.")]
        public float firstBeatOffset = 0f;

        [Header("Start")]
        [Tooltip("Play the song automatically when the scene starts.")]
        public bool playOnStart = true;

        [Tooltip("How far ahead we schedule playback so the audio system starts precisely. 0.1–0.3 is safe.")]
        public float scheduleAheadSeconds = 0.2f;

        [Header("Calibration")]
        [Tooltip("Positive = you feel 'late', so we shift judged input EARLIER. Tune this until Perfect hits feel honest. " +
                 "Hardware/output latency varies by machine; expose this even if hardcoded for the jam.")]
        public float inputLatencySeconds = 0f;

        [Header("Metronome (testing aid)")]
        [Tooltip("Play a click on every whole beat. Great for feeling timing before you have a real song.")]
        public bool metronome = false;
        public AudioClip metronomeClick;
        [Range(0f, 1f)] public float metronomeVolume = 0.5f;

        // --- Public read-only musical state --------------------------------------

        /// <summary>Seconds since beat 0. Negative during the count-in before the song starts.</summary>
        public double SongPositionSeconds { get; private set; }

        /// <summary>Song position expressed in beats (e.g. 4.5 = halfway through the 5th beat).</summary>
        public float SongPositionInBeats { get; private set; }

        /// <summary>Length of one beat in seconds (60 / bpm). Cached each frame.</summary>
        public float SecPerBeat { get; private set; }

        /// <summary>True once playback has been scheduled/started.</summary>
        public bool HasStarted { get; private set; }

        /// <summary>Fires once per whole beat, passing the beat index (0, 1, 2, …). Hook juice/metronome here.</summary>
        public event Action<int> OnBeat;

        // --- Internals -----------------------------------------------------------

        private AudioSource _source;
        private double _dspStartTime;   // dspTime at which beat 0 occurs
        private int _lastWholeBeat = -1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;
            if (song != null) _source.clip = song;
        }

        private void Start()
        {
            if (playOnStart) StartSong();
        }

        /// <summary>
        /// Schedules the song to begin a fraction of a second from now, so the audio
        /// system can start it sample-accurately. Beat 0 is anchored to that scheduled
        /// instant (plus firstBeatOffset), which is what every other system reads against.
        /// </summary>
        public void StartSong()
        {
            SecPerBeat = 60f / bpm;

            double now = AudioSettings.dspTime;
            _dspStartTime = now + scheduleAheadSeconds;

            if (_source.clip != null)
            {
                _source.PlayScheduled(_dspStartTime);
            }
            // If there's no clip we still run the clock silently — the highway will
            // scroll so you can test movement and (with the metronome) input feel.

            HasStarted = true;
            _lastWholeBeat = -1;
        }

        private void Update()
        {
            if (!HasStarted) return;

            SecPerBeat = 60f / bpm; // allow live-tuning bpm in the Inspector while playing

            // The whole game clock, straight from the audio hardware's sample time.
            // Subtract the scheduled start so beat 0 == the moment the song begins,
            // then subtract the intro offset so beat 0 lines up with the first real beat.
            SongPositionSeconds = (AudioSettings.dspTime - _dspStartTime) - firstBeatOffset;
            SongPositionInBeats = (float)(SongPositionSeconds / SecPerBeat);

            FireBeatEvents();
        }

        private void FireBeatEvents()
        {
            int wholeBeat = Mathf.FloorToInt(SongPositionInBeats);
            if (wholeBeat > _lastWholeBeat && wholeBeat >= 0)
            {
                _lastWholeBeat = wholeBeat;
                OnBeat?.Invoke(wholeBeat);

                if (metronome && metronomeClick != null)
                    _source.PlayOneShot(metronomeClick, metronomeVolume);
            }
        }

        // --- On-beat check (used by the guitar for the empowered-shot bonus) ------

        /// <summary>
        /// Signed milliseconds from the nearest whole beat, corrected for latency.
        /// Negative = you fired early, positive = late, ~0 = dead on the beat.
        /// </summary>
        public float NearestBeatErrorMs()
        {
            if (!HasStarted) return float.MaxValue;
            float beats = JudgeTimeBeats;
            float nearest = Mathf.Round(beats);
            return (beats - nearest) * SecPerBeat * 1000f;
        }

        /// <summary>True if we're within <paramref name="windowMs"/> of a beat right now.</summary>
        public bool IsOnBeat(float windowMs) => Mathf.Abs(NearestBeatErrorMs()) <= windowMs;

        /// <summary>
        /// The song position that input should be judged against, corrected for output
        /// latency. Judge notes against THIS, not raw SongPositionSeconds.
        /// </summary>
        public double JudgeTimeSeconds => SongPositionSeconds + inputLatencySeconds;

        /// <summary>Same as JudgeTimeSeconds but in beats — convenient for note comparisons.</summary>
        public float JudgeTimeBeats => (float)(JudgeTimeSeconds / SecPerBeat);

        /// <summary>Converts a beat number to its absolute song time in seconds.</summary>
        public double BeatToSeconds(float beat) => beat * SecPerBeat;
    }
}
