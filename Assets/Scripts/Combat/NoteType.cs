namespace ShredToZero.Combat
{
    /// <summary>
    /// The "colour" of a note. Enemies are weak to one type and resist another, so the
    /// player has to fire the RIGHT note at the RIGHT goon rather than mashing one key.
    ///
    /// Three types = the three villain archetypes from the design doc. Add more later,
    /// but three is the sweet spot for a readable weakness triangle:
    ///   Power  &gt; Bass  &gt; Lead  &gt; Power   (each beats the next)
    /// </summary>
    public enum NoteType
    {
        Power,  // heavy crunchy chord
        Bass,   // low thumping groove
        Lead    // screaming high solo
    }

    public static class NoteTypeExtensions
    {
        /// <summary>A display colour per type, so projectiles/enemies read at a glance.</summary>
        public static UnityEngine.Color ToColor(this NoteType type) => type switch
        {
            NoteType.Power => new UnityEngine.Color(1.0f, 0.35f, 0.25f), // red-orange
            NoteType.Bass  => new UnityEngine.Color(0.35f, 0.55f, 1.0f), // blue
            NoteType.Lead  => new UnityEngine.Color(1.0f, 0.85f, 0.30f), // yellow
            _ => UnityEngine.Color.white
        };
    }
}
