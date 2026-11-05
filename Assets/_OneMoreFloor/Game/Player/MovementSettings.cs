using UnityEngine;

namespace OneMoreFloor.Game
{
    /// <summary>How the player moves. Edit these numbers in the MovementSettings asset in the Inspector.</summary>
    [CreateAssetMenu(menuName = "OneMoreFloor/Movement Settings", fileName = "MovementSettings")]
    public sealed class MovementSettings : ScriptableObject
    {
        [SerializeField, Min(0f), Tooltip("Metres per second.")]
        private float runSpeed = 5.3f;

        [SerializeField, Min(0.01f), Tooltip("How quickly the player reaches run speed or stops, in metres per second per second.")]
        private float acceleration = 30f;

        [SerializeField, Range(0f, 1f), Tooltip("How much steering the player keeps in the air. 0 is none, 1 is the same as on the ground.")]
        private float airControl = 0.4f;

        [SerializeField, Min(0f), Tooltip("How high a jump goes, in metres.")]
        private float jumpHeight = 1.2f;

        [SerializeField, Min(0.01f), Tooltip("How strongly the player falls, in metres per second per second. Higher feels snappier.")]
        private float gravity = 15f;

        [SerializeField, Min(0.01f), Tooltip("The fastest the player can fall, in metres per second.")]
        private float terminalFallSpeed = 50f;

        [SerializeField, Min(0f), Tooltip("How fast the body turns towards the way it is running in third person, in degrees per second.")]
        private float turnSpeed = 720f;

        public float RunSpeed => runSpeed;
        public float Acceleration => acceleration;
        public float AirControl => airControl;
        public float JumpHeight => jumpHeight;
        public float Gravity => gravity;
        public float TerminalFallSpeed => terminalFallSpeed;
        public float TurnSpeed => turnSpeed;
    }
}
