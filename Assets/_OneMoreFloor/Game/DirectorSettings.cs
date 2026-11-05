using OneMoreFloor.Core;
using UnityEngine;

namespace OneMoreFloor.Game
{
    /// <summary>The director's tunable numbers. Edit them in the DirectorSettings asset in the Inspector.</summary>
    [CreateAssetMenu(menuName = "OneMoreFloor/Director Settings", fileName = "DirectorSettings")]
    public sealed class DirectorSettings : ScriptableObject
    {
        [SerializeField, Min(1), Tooltip("The difficulty level goes up by 1 every this many floors.")]
        private int floorsPerDifficultyLevel = 5;

        [SerializeField, Tooltip("The first floor of each visual tier, in order. The first entry must be 1.")]
        private int[] tierStartFloors = { 1, 11, 21 };

        public DirectorRules ToRules() => new DirectorRules(floorsPerDifficultyLevel, tierStartFloors);
    }
}
