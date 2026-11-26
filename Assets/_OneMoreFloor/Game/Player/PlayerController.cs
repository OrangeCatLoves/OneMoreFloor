using FishNet.Object;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace OneMoreFloor.Game
{
    /// <summary>
    /// Connects a player's input to their character and camera. Only the player who owns this
    /// character runs it. Everyone else sees the character through its network components.
    /// </summary>
    [RequireComponent(typeof(PlayerMotor))]
    public class PlayerController : NetworkBehaviour
    {
        /// <summary>Physics layer of the players' characters.</summary>
        public const string PlayerLayer = "Player";

        /// <summary>Physics layer of invisible colliders that stop players but not the camera.</summary>
        public const string PlayerBlockerLayer = "PlayerBlocker";

        [SerializeField]
        private InputActionAsset controls;

        [SerializeField]
        private PlayerCameraSettings cameraSettings;

        [SerializeField]
        private Transform cameraTarget;

        [SerializeField]
        private PlayerAnimator playerAnimator;

        [SerializeField]
        private Renderer[] bodyRenderers;

        private InputActionMap _map;
        private InputAction _move;
        private InputAction _look;
        private InputAction _jump;
        private InputAction _zoom;
        private InputAction _toggleView;
        private InputAction _releaseCursor;
        private InputAction _captureCursor;

        private PlayerMotor _motor;
        private CinemachineThirdPersonFollow _follow;
        private Transform _camera;

        private float _yaw;
        private float _pitch;
        private float _zoomVelocity;
        private bool _inputActive;

        // Step smoothing: how far the camera currently sits below (or above) where it should be.
        private Vector3 _cameraTargetRest;
        private float _lastHeight;
        private bool _wasGrounded;
        private float _stepOffset;
        private float _stepOffsetVelocity;

        public bool IsLocalPlayer { get; private set; }
        public bool IsBodyVisible { get; private set; } = true;
        public ViewZoom Zoom { get; private set; }
        public Transform CameraTarget => cameraTarget;

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!base.IsOwner)
                return;

            IsLocalPlayer = true;
            _motor = GetComponent<PlayerMotor>();
            _yaw = transform.eulerAngles.y;
            _cameraTargetRest = cameraTarget.localPosition;
            _lastHeight = transform.position.y;

            Zoom = new ViewZoom(
                cameraSettings.StartDistance,
                cameraSettings.ClosestThirdPersonDistance,
                cameraSettings.FarthestDistance,
                cameraSettings.ZoomPerNotch);

            BindInput();
            BindCamera();
            SetInputActive(true);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            if (!IsLocalPlayer)
                return;

            _map.Disable();
            SetInputActive(false);
            IsLocalPlayer = false;
        }

        private void BindInput()
        {
            _map = controls.FindActionMap("Player", throwIfNotFound: true);
            _move = _map.FindAction("Move", throwIfNotFound: true);
            _look = _map.FindAction("Look", throwIfNotFound: true);
            _jump = _map.FindAction("Jump", throwIfNotFound: true);
            _zoom = _map.FindAction("Zoom", throwIfNotFound: true);
            _toggleView = _map.FindAction("ToggleView", throwIfNotFound: true);
            _releaseCursor = _map.FindAction("ReleaseCursor", throwIfNotFound: true);
            _captureCursor = _map.FindAction("CaptureCursor", throwIfNotFound: true);
            _map.Enable();
        }

        private void BindCamera()
        {
            var virtualCamera = FindAnyObjectByType<CinemachineCamera>();
            _follow = virtualCamera.GetComponent<CinemachineThirdPersonFollow>();
            virtualCamera.Follow = cameraTarget;
            _follow.CameraDistance = Zoom.Distance;

            ConfigureCameraCollision(virtualCamera);

            _camera = Camera.main.transform;
        }

        // The camera keeps walls and solid props out of the view, but ignores the player and the invisible
        // blockers that only keep players out of places (layer PlayerBlocker).
        // Cinemachine's Deoccluder replaces the third-person rig's own avoidance, which snapped the camera
        // instantly whenever its shoulder point touched a door frame (a 2 m jump walking out of the lift).
        // The Deoccluder ignores brief blocking and eases in and out.
        private void ConfigureCameraCollision(CinemachineCamera virtualCamera)
        {
            CinemachineThirdPersonFollow.ObstacleSettings builtIn = _follow.AvoidObstacles;
            builtIn.Enabled = false;
            _follow.AvoidObstacles = builtIn;

            if (!virtualCamera.TryGetComponent(out CinemachineDeoccluder deoccluder))
                deoccluder = virtualCamera.gameObject.AddComponent<CinemachineDeoccluder>();
            deoccluder.CollideAgainst = ~LayerMask.GetMask(PlayerLayer, "Ignore Raycast", PlayerBlockerLayer);
            deoccluder.IgnoreTag = string.Empty;
            deoccluder.TransparentLayers = 0;
            deoccluder.MinimumDistanceFromTarget = cameraSettings.CollisionMinimumDistance;

            CinemachineDeoccluder.ObstacleAvoidance avoidance = deoccluder.AvoidObstacles;
            avoidance.Enabled = true;
            avoidance.DistanceLimit = 0f;
            avoidance.MinimumOcclusionTime = cameraSettings.IgnoreBriefBlocking;
            avoidance.CameraRadius = cameraSettings.CameraCollisionRadius;
            avoidance.Strategy = CinemachineDeoccluder.ObstacleAvoidance.ResolutionStrategy.PullCameraForward;
            avoidance.MaximumEffort = 4;
            avoidance.SmoothingTime = cameraSettings.CollisionHoldTime;
            avoidance.Damping = cameraSettings.CollisionEaseOut;
            avoidance.DampingWhenOccluded = cameraSettings.CollisionEaseIn;
            deoccluder.AvoidObstacles = avoidance;

            CinemachineDeoccluder.QualityEvaluation quality = deoccluder.ShotQualityEvaluation;
            quality.Enabled = false;
            deoccluder.ShotQualityEvaluation = quality;
        }

        private void Update()
        {
            if (!IsLocalPlayer)
                return;

            UpdateCursorCapture();

            Vector2 moveInput = Vector2.zero;
            bool jump = false;

            if (_inputActive)
            {
                Look();
                ChangeView();
                moveInput = _move.ReadValue<Vector2>();
                jump = _jump.WasPressedThisFrame();
            }

            _motor.Tick(new MoveCommand(moveInput, jump, _yaw, Zoom.IsFirstPerson), Time.deltaTime);
            playerAnimator.Apply(_motor, Time.deltaTime);

            SmoothSteps(Time.deltaTime);
            UpdateCamera(Time.deltaTime);
        }

        /// <summary>Moves the local player instantly, for example to a floor's start. The camera follows without easing.</summary>
        public void PlaceAt(Vector3 position, float yaw)
        {
            var body = GetComponent<CharacterController>();
            body.enabled = false;
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            body.enabled = true;

            _yaw = yaw;
            if (cameraTarget != null)
                cameraTarget.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

            _lastHeight = position.y;
            _wasGrounded = false;
            _stepOffset = 0f;
            _stepOffsetVelocity = 0f;
        }

        // The character controller climbs a step in a single frame, which jolts the camera.
        // While walking on the ground, the camera keeps its height through that jump and eases after it.
        // Jumps and falls aren't eased: they are already smooth.
        private void SmoothSteps(float deltaTime)
        {
            float height = transform.position.y;
            float change = height - _lastHeight;
            _lastHeight = height;

            if (_wasGrounded && _motor.IsGrounded && Mathf.Abs(change) <= cameraSettings.MaxSmoothedStep)
                _stepOffset = Mathf.Clamp(_stepOffset - change, -cameraSettings.MaxSmoothedStep, cameraSettings.MaxSmoothedStep);
            _wasGrounded = _motor.IsGrounded;

            _stepOffset = Mathf.SmoothDamp(_stepOffset, 0f, ref _stepOffsetVelocity, cameraSettings.StepSmoothTime, Mathf.Infinity, deltaTime);
            cameraTarget.localPosition = _cameraTargetRest + Vector3.up * _stepOffset;
        }

        private void UpdateCursorCapture()
        {
            if (_inputActive && _releaseCursor.WasPressedThisFrame())
                SetInputActive(false);
            else if (!_inputActive && _captureCursor.WasPressedThisFrame())
                SetInputActive(true);
        }

        private void SetInputActive(bool active)
        {
            _inputActive = active;
            Cursor.lockState = active ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !active;
        }

        private void Look()
        {
            Vector2 lookInput = _look.ReadValue<Vector2>() * cameraSettings.LookSensitivity;
            _yaw = Mathf.Repeat(_yaw + lookInput.x, 360f);
            _pitch = Mathf.Clamp(_pitch - lookInput.y, cameraSettings.LookUpLimit, cameraSettings.LookDownLimit);
            cameraTarget.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        private void ChangeView()
        {
            // Mice report whole notches and trackpads report fractions. Some systems report much
            // larger numbers per notch, so this keeps one frame's scroll to a notch at most.
            Zoom.Scroll(Mathf.Clamp(_zoom.ReadValue<float>(), -1f, 1f));

            if (_toggleView.WasPressedThisFrame())
                Zoom.ToggleView();
        }

        private void UpdateCamera(float deltaTime)
        {
            _follow.CameraDistance = Mathf.SmoothDamp(
                _follow.CameraDistance,
                Zoom.Distance,
                ref _zoomVelocity,
                cameraSettings.ZoomSmoothTime,
                Mathf.Infinity,
                deltaTime);

            // The shoulder offset and soft follow fade out as the camera reaches the player's eyes.
            float thirdPersonAmount = Mathf.InverseLerp(0f, cameraSettings.ClosestThirdPersonDistance, _follow.CameraDistance);
            _follow.ShoulderOffset = Vector3.Lerp(Vector3.zero, cameraSettings.ShoulderOffset, thirdPersonAmount);
            _follow.Damping = Vector3.Lerp(Vector3.zero, cameraSettings.FollowDamping, thirdPersonAmount);

            // This also covers a wall pushing the camera in close, not just zooming in.
            float cameraToEyes = Vector3.Distance(_camera.position, cameraTarget.position);
            SetBodyVisible(cameraToEyes >= cameraSettings.HideBodyDistance);
        }

        private void SetBodyVisible(bool visible)
        {
            if (visible == IsBodyVisible)
                return;

            IsBodyVisible = visible;
            ShadowCastingMode mode = visible ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly;
            foreach (Renderer body in bodyRenderers)
                body.shadowCastingMode = mode;
        }
    }
}
