using System;
using System.Collections;
using System.Linq;
using FishNet;
using NUnit.Framework;
using OneMoreFloor.Game;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace OneMoreFloor.Tests.PlayMode
{
    /// <summary>Runs the real scene as a local host, driven by simulated keyboard and mouse input.</summary>
    public class MovementSceneTests
    {
        private const string SceneName = "MovementTest";
        private const float SpawnTimeoutSeconds = 15f;

        private Keyboard _keyboard;
        private Mouse _mouse;
        private InputSettings.BackgroundBehavior _backgroundBehavior;
        private InputSettings.EditorInputBehaviorInPlayMode _editorBehavior;
        private PlayerController _player;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Lets simulated input reach the game with no focused window.
            _backgroundBehavior = InputSystem.settings.backgroundBehavior;
            _editorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;

            _keyboard = InputSystem.AddDevice<Keyboard>();
            _mouse = InputSystem.AddDevice<Mouse>();

            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);

            float deadline = Time.realtimeSinceStartup + SpawnTimeoutSeconds;
            while (FindLocalPlayer() == null && Time.realtimeSinceStartup < deadline)
                yield return null;

            _player = FindLocalPlayer();
            Assert.That(_player, Is.Not.Null, "The host should have spawned a local player.");

            // Lets the character settle on the ground.
            yield return new WaitForSeconds(0.5f);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            InputSystem.RemoveDevice(_keyboard);
            InputSystem.RemoveDevice(_mouse);
            InputSystem.settings.backgroundBehavior = _backgroundBehavior;
            InputSystem.settings.editorInputBehaviorInPlayMode = _editorBehavior;

            if (InstanceFinder.NetworkManager != null)
            {
                GameObject network = InstanceFinder.NetworkManager.gameObject;
                InstanceFinder.ClientManager.StopConnection();
                InstanceFinder.ServerManager.StopConnection(true);
                Object.Destroy(network);
            }

            // Other tests must not find this scene's ground and walls still loaded.
            Scene empty = SceneManager.CreateScene("TestEmptyScene" + Guid.NewGuid());
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(SceneName);
        }

        private static PlayerController FindLocalPlayer()
        {
            return Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).FirstOrDefault(player => player.IsLocalPlayer);
        }

        private void Hold(params Key[] keys)
        {
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(keys));
        }

        private IEnumerator Tap(Key key)
        {
            Hold(key);
            yield return null;
            yield return null;
            Hold();
            yield return null;
        }

        private IEnumerator ScrollIn(float notches)
        {
            InputSystem.QueueStateEvent(_mouse, new MouseState { scroll = new Vector2(0f, notches) });
            yield return null;
            InputSystem.QueueStateEvent(_mouse, new MouseState());
            yield return null;
        }

        [Test]
        public void TheHost_RunsAsServerAndClient()
        {
            Assert.That(InstanceFinder.NetworkManager.IsServerStarted, Is.True);
            Assert.That(InstanceFinder.NetworkManager.IsClientStarted, Is.True);
            Assert.That(Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
        }

        [Test]
        public void TheCamera_FollowsTheLocalPlayerInThirdPerson()
        {
            var virtualCamera = Object.FindAnyObjectByType<CinemachineCamera>();

            Assert.That(virtualCamera.Follow, Is.EqualTo(_player.CameraTarget));
            Assert.That(_player.Zoom.IsFirstPerson, Is.False);
            Assert.That(_player.IsBodyVisible, Is.True);
        }

        [UnityTest]
        public IEnumerator HoldingW_MovesThePlayer()
        {
            Vector3 start = _player.transform.position;

            Hold(Key.W);
            yield return new WaitForSeconds(0.5f);
            Hold();
            yield return null;

            Vector3 moved = _player.transform.position - start;
            moved.y = 0f;
            Assert.That(moved.magnitude, Is.GreaterThan(1f));
        }

        [UnityTest]
        public IEnumerator MovingTheMouse_TurnsTheCamera()
        {
            float startYaw = _player.CameraTarget.eulerAngles.y;

            InputSystem.QueueStateEvent(_mouse, new MouseState { delta = new Vector2(200f, 0f) });
            yield return null;
            yield return null;
            InputSystem.QueueStateEvent(_mouse, new MouseState());
            yield return null;

            Assert.That(Mathf.Abs(Mathf.DeltaAngle(startYaw, _player.CameraTarget.eulerAngles.y)), Is.GreaterThan(1f));
        }

        [UnityTest]
        public IEnumerator Escape_FreesTheCursorAndStopsInput_UntilAClick()
        {
            yield return Tap(Key.Escape);

            Vector3 start = _player.transform.position;
            Hold(Key.W);
            yield return new WaitForSeconds(0.3f);
            Hold();
            yield return null;
            Assert.That(Vector3.Distance(start, _player.transform.position), Is.LessThan(0.05f), "Input should stop after Escape.");

            InputSystem.QueueStateEvent(_mouse, new MouseState().WithButton(MouseButton.Left));
            yield return null;
            yield return null;
            InputSystem.QueueStateEvent(_mouse, new MouseState());
            yield return null;

            Hold(Key.W);
            yield return new WaitForSeconds(0.3f);
            Hold();
            yield return null;
            Assert.That(Vector3.Distance(start, _player.transform.position), Is.GreaterThan(0.3f), "A click should bring input back.");
        }

        [UnityTest]
        public IEnumerator PressingSpace_MakesThePlayerJump()
        {
            float startHeight = _player.transform.position.y;
            float peak = startHeight;

            Hold(Key.Space);
            float end = Time.realtimeSinceStartup + 0.4f;
            while (Time.realtimeSinceStartup < end)
            {
                peak = Mathf.Max(peak, _player.transform.position.y);
                yield return null;
            }
            Hold();

            Assert.That(peak - startHeight, Is.GreaterThan(0.3f));
        }

        [UnityTest]
        public IEnumerator PressingV_SwitchesToFirstPersonAndHidesTheBody()
        {
            yield return Tap(Key.V);
            Assert.That(_player.Zoom.IsFirstPerson, Is.True);

            yield return new WaitForSeconds(1f);
            Assert.That(_player.IsBodyVisible, Is.False);

            yield return Tap(Key.V);
            Assert.That(_player.Zoom.IsFirstPerson, Is.False);

            yield return new WaitForSeconds(1f);
            Assert.That(_player.IsBodyVisible, Is.True);
        }

        [UnityTest]
        public IEnumerator ScrollingAllTheWayIn_BecomesFirstPerson()
        {
            float start = _player.Zoom.Distance;

            yield return ScrollIn(1f);
            Assert.That(_player.Zoom.Distance, Is.LessThan(start));

            for (int i = 0; i < 10; i++)
                yield return ScrollIn(1f);

            Assert.That(_player.Zoom.IsFirstPerson, Is.True);
        }

        [UnityTest]
        public IEnumerator InFirstPerson_TheCameraReachesThePlayersEyes()
        {
            yield return Tap(Key.V);
            yield return new WaitForSeconds(1f);

            float distance = Vector3.Distance(Camera.main.transform.position, _player.CameraTarget.position);

            Assert.That(distance, Is.LessThan(0.1f));
        }

        [UnityTest]
        public IEnumerator InThirdPerson_AWallPushesTheCameraInInsteadOfClippingThroughIt()
        {
            // A wall right behind the player would hide the view if the camera went through it.
            Vector3 behind = _player.transform.position - Vector3.forward * 1.2f + Vector3.up;
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = behind;
            wall.transform.localScale = new Vector3(20f, 6f, 0.5f);
            Physics.SyncTransforms();

            yield return new WaitForSeconds(1f);

            float cameraZ = Camera.main.transform.position.z;
            Object.Destroy(wall);

            Assert.That(cameraZ, Is.GreaterThan(behind.z), "The camera should stay on the player's side of the wall.");
        }
    }
}
