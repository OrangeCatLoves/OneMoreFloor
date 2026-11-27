using System.Collections.Generic;
using UnityEngine;

namespace OneMoreFloor.Game
{
    /// <summary>
    /// Lights the lift's bulb floor signs with the current floor number, on every player's machine. The lift's
    /// art has one object per segment of each seven-segment digit, named SM_LiftSign_&lt;Out|In&gt;_&lt;Tens|Ones&gt;_&lt;a-g&gt;
    /// (tools/blender/trivia_round_three.py). Unlit segments keep their bulbs, switched off.
    /// </summary>
    public sealed class LiftFloorSigns : MonoBehaviour
    {
        private const string Prefix = "SM_LiftSign_";
        private const int Highest = 99;

        // Which segments are lit for each digit (a top, b top right, c bottom right, d bottom, e bottom left,
        // f top left, g middle).
        private static readonly string[] LitSegments = { "abcdef", "bc", "abdeg", "abcdg", "bcfg", "acdfg", "acdefg", "abc", "abcdefg", "abcdfg" };

        private readonly List<Segment> _segments = new List<Segment>();
        private Material _off;

        private struct Segment
        {
            public Renderer Renderer;
            public bool Tens;
            public char Letter;
            public Material Lit;
        }

        /// <summary>The number the signs show, or -1 before they have been set.</summary>
        public int ShownNumber { get; private set; } = -1;

        public static string LitFor(int digit) => LitSegments[digit];

        private void Start()
        {
            foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if (!renderer.name.StartsWith(Prefix))
                    continue;
                string[] parts = renderer.name.Substring(Prefix.Length).Split('_');
                if (parts.Length < 3 || parts[2].Length != 1)
                    continue;
                _segments.Add(new Segment { Renderer = renderer, Tens = parts[1] == "Tens", Letter = parts[2][0], Lit = renderer.sharedMaterial });
            }
            if (_segments.Count == 0)
                return;

            // A switched-off bulb: the same material without its glow, a little darker.
            _off = new Material(_segments[0].Lit) { name = "LiftSignBulbOff" };
            _off.DisableKeyword("_EMISSION");
            if (_off.HasProperty("_EmissionColor"))
                _off.SetColor("_EmissionColor", Color.black);
            if (_off.HasProperty("_BaseColor"))
                _off.SetColor("_BaseColor", new Color(0.28f, 0.24f, 0.2f));
        }

        private void Update()
        {
            if (_segments.Count == 0)
                return;
            RunManager run = RunManager.Instance;
            int floor = Mathf.Clamp(run != null ? run.FloorNumber : 1, 0, Highest);
            if (floor == ShownNumber)
                return;

            ShownNumber = floor;
            string ones = LitSegments[floor % 10];
            string tens = floor >= 10 ? LitSegments[floor / 10] : string.Empty;
            foreach (Segment segment in _segments)
            {
                bool on = (segment.Tens ? tens : ones).IndexOf(segment.Letter) >= 0;
                segment.Renderer.sharedMaterial = on ? segment.Lit : _off;
            }
        }

        private void OnDestroy()
        {
            if (_off != null)
                Destroy(_off);
        }
    }
}
