using UnityEngine;

namespace OneMoreFloor.Game
{
    /// <summary>
    /// Feeds movement into the character's Animator. It sits on the same object as the Animator so
    /// that it also receives the animation events the clips call.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public sealed class PlayerAnimator : MonoBehaviour
    {
        // Brief drops, such as walking down a step, shouldn't play the falling animation.
        private const float FreeFallDelay = 0.15f;

        private static readonly int SpeedParameter = Animator.StringToHash("Speed");
        private static readonly int MotionSpeedParameter = Animator.StringToHash("MotionSpeed");
        private static readonly int GroundedParameter = Animator.StringToHash("Grounded");
        private static readonly int JumpParameter = Animator.StringToHash("Jump");
        private static readonly int FreeFallParameter = Animator.StringToHash("FreeFall");

        private Animator _animator;
        private float _airTime;
        private bool _jumping;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Apply(IPlayerMotor motor, float deltaTime)
        {
            if (motor.IsGrounded)
            {
                _airTime = 0f;
                _jumping = false;
            }
            else
            {
                _airTime += deltaTime;
            }

            if (motor.JumpedThisTick)
                _jumping = true;

            _animator.SetFloat(SpeedParameter, motor.Speed);
            _animator.SetFloat(MotionSpeedParameter, 1f);
            _animator.SetBool(GroundedParameter, motor.IsGrounded);
            _animator.SetBool(JumpParameter, _jumping);
            _animator.SetBool(FreeFallParameter, !motor.IsGrounded && _airTime > FreeFallDelay);
        }

        // The package's animation clips call these. Footstep and landing sounds will go here.
        public void OnFootstep(AnimationEvent animationEvent)
        {
        }

        public void OnLand(AnimationEvent animationEvent)
        {
        }
    }
}
