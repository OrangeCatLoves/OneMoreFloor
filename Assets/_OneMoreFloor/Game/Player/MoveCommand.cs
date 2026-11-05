using UnityEngine;

namespace OneMoreFloor.Game
{
    /// <summary>What the player wants to do this frame.</summary>
    public readonly struct MoveCommand
    {
        public MoveCommand(Vector2 move, bool jump, float cameraYaw, bool faceCamera)
        {
            Move = move;
            Jump = jump;
            CameraYaw = cameraYaw;
            FaceCamera = faceCamera;
        }

        /// <summary>x is right and y is forward, relative to the camera.</summary>
        public Vector2 Move { get; }

        public bool Jump { get; }

        /// <summary>The way the camera faces, in degrees.</summary>
        public float CameraYaw { get; }

        /// <summary>True turns the body with the camera, as in first person. False turns it towards the way it is running.</summary>
        public bool FaceCamera { get; }
    }
}
