using System.Collections;
using NUnit.Framework;
using OneMoreFloor.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace OneMoreFloor.Tests.PlayMode
{
    public class PlayerMotorTests
    {
        private const float TickSeconds = 1f / 60f;

        private GameObject _ground;
        private GameObject _player;
        private GameObject _wall;
        private PlayerMotor _motor;
        private MovementSettings _settings;

        private static MoveCommand Idle => new MoveCommand(Vector2.zero, false, 0f, false);
        private static MoveCommand Forward => new MoveCommand(Vector2.up, false, 0f, false);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _ground.transform.position = new Vector3(0f, -0.5f, 0f);
            _ground.transform.localScale = new Vector3(1000f, 1f, 1000f);

            _player = new GameObject("TestPlayer");
            _player.transform.position = new Vector3(0f, 0.05f, 0f);

            var body = _player.AddComponent<CharacterController>();
            body.height = 1.8f;
            body.radius = 0.4f;
            body.center = new Vector3(0f, 0.9f, 0f);

            _settings = ScriptableObject.CreateInstance<MovementSettings>();
            _motor = _player.AddComponent<PlayerMotor>();
            _motor.Settings = _settings;

            Physics.SyncTransforms();
            yield return null;

            Step(Idle, 30);
            Assert.That(_motor.IsGrounded, Is.True, "The player should have settled on the ground.");
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_player);
            Object.Destroy(_ground);
            Object.Destroy(_settings);
            if (_wall != null)
                Object.Destroy(_wall);
        }

        private void Step(MoveCommand command, int ticks)
        {
            for (int i = 0; i < ticks; i++)
                _motor.Tick(command, TickSeconds);
        }

        [Test]
        public void Running_ReachesRunSpeed()
        {
            Step(Forward, 120);

            Assert.That(_motor.Speed, Is.EqualTo(_settings.RunSpeed).Within(0.05f));
        }

        [Test]
        public void Running_FollowsTheCameraDirection()
        {
            Step(new MoveCommand(Vector2.up, false, 90f, false), 60);

            Assert.That(_player.transform.position.x, Is.GreaterThan(2f));
            Assert.That(Mathf.Abs(_player.transform.position.z), Is.LessThan(0.01f));
        }

        [Test]
        public void DiagonalInput_IsNoFasterThanStraight()
        {
            Step(new MoveCommand(new Vector2(1f, 1f), false, 0f, false), 120);

            Assert.That(_motor.Speed, Is.EqualTo(_settings.RunSpeed).Within(0.05f));
        }

        [Test]
        public void ReleasingInput_Stops()
        {
            Step(Forward, 60);

            Step(Idle, 60);

            Assert.That(_motor.Speed, Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void Jumping_ReachesTheJumpHeightAndLands()
        {
            float startHeight = _player.transform.position.y;
            float peak = startHeight;

            _motor.Tick(new MoveCommand(Vector2.zero, true, 0f, false), TickSeconds);
            Assert.That(_motor.JumpedThisTick, Is.True);

            for (int i = 0; i < 300 && !_motor.IsGrounded; i++)
            {
                _motor.Tick(Idle, TickSeconds);
                peak = Mathf.Max(peak, _player.transform.position.y);
            }

            Assert.That(peak - startHeight, Is.EqualTo(_settings.JumpHeight).Within(_settings.JumpHeight * 0.1f));
            Assert.That(_motor.IsGrounded, Is.True);
        }

        [Test]
        public void JumpingInTheAir_DoesNothing()
        {
            _motor.Tick(new MoveCommand(Vector2.zero, true, 0f, false), TickSeconds);
            Step(Idle, 5);
            Assert.That(_motor.IsGrounded, Is.False);

            _motor.Tick(new MoveCommand(Vector2.zero, true, 0f, false), TickSeconds);

            Assert.That(_motor.JumpedThisTick, Is.False);
        }

        [Test]
        public void AirControl_SteersMoreSlowlyThanOnTheGround()
        {
            Step(Forward, 3);
            float groundSpeed = _motor.Speed;
            Step(Idle, 60);

            _motor.Tick(new MoveCommand(Vector2.zero, true, 0f, false), TickSeconds);
            Step(Forward, 3);

            Assert.That(_motor.Speed, Is.LessThan(groundSpeed));
        }

        [Test]
        public void WithNoGround_ThePlayerFallsAtALimitedSpeed()
        {
            Object.DestroyImmediate(_ground);
            Physics.SyncTransforms();

            float before = _player.transform.position.y;
            Step(Idle, 600);
            float lastBefore = _player.transform.position.y;
            Step(Idle, 60);
            float fallSpeed = lastBefore - _player.transform.position.y;

            Assert.That(_motor.IsGrounded, Is.False);
            Assert.That(_player.transform.position.y, Is.LessThan(before));
            Assert.That(fallSpeed, Is.EqualTo(_settings.TerminalFallSpeed).Within(0.1f));
        }

        [Test]
        public void Walls_StopThePlayer()
        {
            _wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _wall.transform.position = new Vector3(0f, 2f, 2f);
            _wall.transform.localScale = new Vector3(10f, 4f, 1f);
            Physics.SyncTransforms();

            Step(Forward, 180);

            Assert.That(_player.transform.position.z, Is.LessThan(1.5f));
            Assert.That(_motor.Speed, Is.LessThan(0.5f), "Speed is measured from real movement, so the run animation stops at a wall.");
        }

        [Test]
        public void ThirdPerson_TurnsTowardsTheWayItRuns()
        {
            Step(new MoveCommand(Vector2.right, false, 0f, false), 60);

            Assert.That(Mathf.DeltaAngle(_player.transform.eulerAngles.y, 90f), Is.EqualTo(0f).Within(1f));
        }

        [Test]
        public void FirstPerson_TurnsWithTheCamera()
        {
            _motor.Tick(new MoveCommand(Vector2.zero, false, 135f, true), TickSeconds);

            Assert.That(Mathf.DeltaAngle(_player.transform.eulerAngles.y, 135f), Is.EqualTo(0f).Within(0.01f));
        }
    }
}
