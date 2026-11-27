using UnityEngine;

namespace OneMoreFloor.Game
{
    /// <summary>The run's tunable numbers. Edit them in the GameRules asset in the Inspector.</summary>
    [CreateAssetMenu(menuName = "OneMoreFloor/Game Rules", fileName = "GameRules")]
    public sealed class GameRules : ScriptableObject
    {
        [SerializeField, Min(1), Tooltip("Starting hearts = this x the number of players.")]
        private int heartsPerPlayer = 10;

        [SerializeField, Min(0f), Tooltip("Seconds the elevator ride lasts.")]
        private float elevatorRideSeconds = 7f;

        [SerializeField, Min(0f), Tooltip("Seconds the title card shows the floor name and goal.")]
        private float introSeconds = 5f;

        [SerializeField, Min(0f), Tooltip("Seconds of countdown after the title card.")]
        private float countdownSeconds = 3f;

        [SerializeField, Min(0f), Tooltip("Seconds before a player respawns. A floor's spec can use its own value.")]
        private float respawnDelaySeconds = 2f;

        [SerializeField, Min(0f), Tooltip("Seconds the 'floor cleared' banner shows before the next floor.")]
        private float floorClearedSeconds = 3f;

        [SerializeField, Min(0f), Tooltip("Seconds the 'game over' banner shows before the results.")]
        private float gameOverSeconds = 3f;

        public int HeartsPerPlayer => heartsPerPlayer;
        public float ElevatorRideSeconds => elevatorRideSeconds;
        public float IntroSeconds => introSeconds;
        public float CountdownSeconds => countdownSeconds;
        public float RespawnDelaySeconds => respawnDelaySeconds;
        public float FloorClearedSeconds => floorClearedSeconds;
        public float GameOverSeconds => gameOverSeconds;
    }
}
