using System.Linq;
using FishNet.Component.Transforming;
using FishNet.Object;
using NUnit.Framework;
using OneMoreFloor.Game;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OneMoreFloor.Tests.EditMode
{
    public class PlayerAssetTests
    {
        private const string PlayerPrefabPath = "Assets/_OneMoreFloor/Game/Player/Player.prefab";
        private const string ControlsPath = "Assets/_OneMoreFloor/Game/Player/PlayerControls.inputactions";
        private const string ScenePath = "Assets/_OneMoreFloor/Game/Scenes/MovementTest.unity";
        private const string DataFolder = "Assets/_OneMoreFloor/Data/";

        // The owner tunes these assets, so this checks that their values are usable, not what they are.
        [Test]
        public void MovementSettingsAsset_ExistsWithUsableValues()
        {
            var settings = AssetDatabase.LoadAssetAtPath<MovementSettings>(DataFolder + "MovementSettings.asset");

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.RunSpeed, Is.GreaterThan(0f));
            Assert.That(settings.Gravity, Is.GreaterThan(0f));
            Assert.That(settings.JumpHeight, Is.GreaterThan(0f));
        }

        [Test]
        public void CameraSettingsAsset_AllowsAThirdPersonRange()
        {
            var settings = AssetDatabase.LoadAssetAtPath<PlayerCameraSettings>(DataFolder + "PlayerCameraSettings.asset");

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.ClosestThirdPersonDistance, Is.GreaterThan(0f));
            Assert.That(settings.FarthestDistance, Is.GreaterThanOrEqualTo(settings.ClosestThirdPersonDistance));
        }

        [Test]
        public void PlayerPrefab_IsSetUpForMovementAndNetworking()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<CharacterController>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<NetworkObject>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<NetworkTransform>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<PlayerController>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<PlayerMotor>().Settings, Is.Not.Null);
            Assert.That(prefab.GetComponentInChildren<PlayerAnimator>(), Is.Not.Null);
            Assert.That(LayerMask.LayerToName(prefab.layer), Is.EqualTo("Player"));
        }

        [TestCase("Move")]
        [TestCase("Look")]
        [TestCase("Jump")]
        [TestCase("Zoom")]
        [TestCase("ToggleView")]
        public void Controls_HaveAKeyboardOrMouseBinding(string actionName)
        {
            var controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);

            InputAction action = controls.FindAction(actionName, throwIfNotFound: true);

            Assert.That(action.bindings.Any(b => b.path.Contains("<Keyboard>") || b.path.Contains("<Mouse>") || b.isComposite), Is.True);
        }

        [Test]
        public void MovementTestScene_IsInTheBuild()
        {
            Assert.That(EditorBuildSettings.scenes.Any(scene => scene.path == ScenePath && scene.enabled), Is.True);
        }
    }
}
