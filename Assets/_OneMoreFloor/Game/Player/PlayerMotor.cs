using UnityEngine;

namespace OneMoreFloor.Game
{
    /// <summary>Moves the character with Unity's CharacterController. Each client moves its own character.</summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour, IPlayerMotor
    {
        // A small push into the ground that keeps the character in contact when walking.
        private const float GroundedDownwardSpeed = -2f;

        [SerializeField]
        private MovementSettings settings;

        private CharacterController _body;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;

        public MovementSettings Settings
        {
            get => settings;
            set => settings = value;
        }

        public float Speed { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool JumpedThisTick { get; private set; }

        private void Awake()
        {
            _body = GetComponent<CharacterController>();
        }

        public void Tick(MoveCommand command, float deltaTime)
        {
            Quaternion cameraYaw = Quaternion.Euler(0f, command.CameraYaw, 0f);
            Vector2 move = Vector2.ClampMagnitude(command.Move, 1f);
            Vector3 desiredVelocity = cameraYaw * new Vector3(move.x, 0f, move.y) * settings.RunSpeed;

            float acceleration = settings.Acceleration * (IsGrounded ? 1f : settings.AirControl);
            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, desiredVelocity, acceleration * deltaTime);

            UpdateVerticalVelocity(command.Jump, deltaTime);

            Vector3 before = transform.position;
            CollisionFlags hits = _body.Move((_horizontalVelocity + Vector3.up * _verticalVelocity) * deltaTime);
            Vector3 moved = transform.position - before;

            moved.y = 0f;
            Speed = deltaTime > 0f ? moved.magnitude / deltaTime : 0f;
            IsGrounded = (hits & CollisionFlags.Below) != 0;

            if ((hits & CollisionFlags.Above) != 0 && _verticalVelocity > 0f)
                _verticalVelocity = 0f;

            Turn(command.FaceCamera, cameraYaw, desiredVelocity, deltaTime);
        }

        private void UpdateVerticalVelocity(bool jump, float deltaTime)
        {
            JumpedThisTick = false;

            if (!IsGrounded)
            {
                _verticalVelocity = Mathf.Max(_verticalVelocity - settings.Gravity * deltaTime, -settings.TerminalFallSpeed);
                return;
            }

            if (jump)
            {
                _verticalVelocity = Mathf.Sqrt(2f * settings.JumpHeight * settings.Gravity);
                JumpedThisTick = true;
            }
            else
            {
                _verticalVelocity = GroundedDownwardSpeed;
            }
        }

        private void Turn(bool faceCamera, Quaternion cameraYaw, Vector3 desiredVelocity, float deltaTime)
        {
            if (faceCamera)
            {
                transform.rotation = cameraYaw;
            }
            else if (desiredVelocity.sqrMagnitude > 0.01f)
            {
                Quaternion runDirection = Quaternion.LookRotation(desiredVelocity);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, runDirection, settings.TurnSpeed * deltaTime);
            }
        }
    }
}
