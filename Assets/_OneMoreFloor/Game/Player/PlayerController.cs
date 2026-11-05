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
            _camera = Camera.main.transform;
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

            UpdateCamera(Time.deltaTime);
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
