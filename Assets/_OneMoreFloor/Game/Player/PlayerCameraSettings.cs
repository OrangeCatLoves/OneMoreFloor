using UnityEngine;

namespace OneMoreFloor.Game
{
    /// <summary>How the player's camera behaves. Edit these numbers in the PlayerCameraSettings asset in the Inspector.</summary>
    [CreateAssetMenu(menuName = "OneMoreFloor/Player Camera Settings", fileName = "PlayerCameraSettings")]
    public sealed class PlayerCameraSettings : ScriptableObject
    {
        [SerializeField, Min(0f), Tooltip("Degrees the view turns per pixel of mouse movement.")]
        private float lookSensitivity = 0.1f;

        [SerializeField, Range(-89f, 0f), Tooltip("How far up the player can look, in degrees.")]
        private float lookUpLimit = -70f;

        [SerializeField, Range(0f, 89f), Tooltip("How far down the player can look, in degrees.")]
        private float lookDownLimit = 80f;

        [SerializeField, Min(0f), Tooltip("Height of the player's eyes above their feet, in metres.")]
        private float eyeHeight = 1.6f;

        [SerializeField, Min(0.01f), Tooltip("Camera distance behind the player when a run starts, in metres.")]
        private float startDistance = 4f;

        [SerializeField, Min(0.01f), Tooltip("The closest third-person distance. Scrolling in past this becomes first person.")]
        private float closestThirdPersonDistance = 1.5f;

        [SerializeField, Min(0.01f), Tooltip("The farthest the camera can zoom out, in metres.")]
        private float farthestDistance = 7f;

        [SerializeField, Min(0.01f), Tooltip("Metres of zoom per notch of the scroll wheel.")]
        private float zoomPerNotch = 0.75f;

        [SerializeField, Min(0f), Tooltip("Seconds the camera takes to settle after zooming. Higher is smoother.")]
        private float zoomSmoothTime = 0.12f;

        [SerializeField, Tooltip("Where the third-person camera sits relative to the player's head, in metres.")]
        private Vector3 shoulderOffset = new Vector3(0.4f, 0f, 0f);

        [SerializeField, Tooltip("How softly the third-person camera follows the player. 0 is locked on. First person is always locked on.")]
        private Vector3 followDamping = new Vector3(0.1f, 0.25f, 0.25f);

        [SerializeField, Min(0f), Tooltip("The player's own body hides when the camera is closer to their head than this, in metres.")]
        private float hideBodyDistance = 0.8f;

        public float LookSensitivity => lookSensitivity;
        public float LookUpLimit => lookUpLimit;
        public float LookDownLimit => lookDownLimit;
        public float EyeHeight => eyeHeight;
        public float StartDistance => startDistance;
        public float ClosestThirdPersonDistance => closestThirdPersonDistance;
        public float FarthestDistance => farthestDistance;
        public float ZoomPerNotch => zoomPerNotch;
        public float ZoomSmoothTime => zoomSmoothTime;
        public Vector3 ShoulderOffset => shoulderOffset;
        public Vector3 FollowDamping => followDamping;
        public float HideBodyDistance => hideBodyDistance;
    }
}
