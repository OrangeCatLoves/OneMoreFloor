using System;
using UnityEngine;

namespace OneMoreFloor.Floors.Trivia
{
    /// <summary>A flat screen in the room: where its middle is, which way it faces and how big it is.</summary>
    public readonly struct ScreenSurface
    {
        public ScreenSurface(Vector3 centre, Vector3 facing, float width, float height)
        {
            Centre = centre;
            Facing = facing;
            Width = width;
            Height = height;
        }

        public Vector3 Centre { get; }

        /// <summary>Points out of the screen, towards the people watching it.</summary>
        public Vector3 Facing { get; }

        public float Width { get; }
        public float Height { get; }

        /// <summary>The rotation that makes a world-space canvas on this surface face the viewers.</summary>
        public Quaternion CanvasRotation => Quaternion.LookRotation(-Facing, Vector3.up);
    }

    /// <summary>
    /// Finds Trivia's podiums and screens in the loaded room by their markers: MK_Objective_0n is where a
    /// player stands at podium n and COL_Trivia_Podium_0n is its collider (from the art); MK_Screen_Podium_0n,
    /// MK_Screen_Question (the big screen) and MK_Screen_Players (the players' TV) are made from the screen
    /// meshes when the room's scene is built (TriviaReviewSceneBuilder.AddScreenMarkers): centre, forward out
    /// of the screen, scale = width and height.
    /// </summary>
    public sealed class TriviaRoom
    {
        public const int PodiumCount = 4;

        private TriviaRoom(Vector3[] stands, float[] standYaw, ScreenSurface[] podiumScreens, ScreenSurface questionScreen, ScreenSurface playerScreen)
        {
            Stands = stands;
            StandYaw = standYaw;
            PodiumScreens = podiumScreens;
            QuestionScreen = questionScreen;
            PlayerScreen = playerScreen;
        }

        /// <summary>Where a player stands at each podium (on the floor).</summary>
        public Vector3[] Stands { get; }

        /// <summary>Which way a player at each podium faces: towards the podium and the audience.</summary>
        public float[] StandYaw { get; }

        public ScreenSurface[] PodiumScreens { get; }
        public ScreenSurface QuestionScreen { get; }
        public ScreenSurface PlayerScreen { get; }

        public static TriviaRoom Find()
        {
            var stands = new Vector3[PodiumCount];
            var yaw = new float[PodiumCount];
            var podiumScreens = new ScreenSurface[PodiumCount];
            for (int i = 0; i < PodiumCount; i++)
            {
                string number = (i + 1).ToString("00");
                Transform stand = Required("MK_Objective_" + number).transform;
                Collider podium = Required("COL_Trivia_Podium_" + number).GetComponent<Collider>();
                Vector3 podiumCentre = podium != null ? podium.bounds.center : Required("SM_Trivia_Podium_" + number).transform.position;
                Vector3 towardsPodium = podiumCentre - stand.position;
                towardsPodium.y = 0f;
                stands[i] = stand.position;
                yaw[i] = Quaternion.LookRotation(towardsPodium.normalized).eulerAngles.y;
                podiumScreens[i] = Surface(Required("MK_Screen_Podium_" + number));
            }

            return new TriviaRoom(stands, yaw, podiumScreens, Surface(Required("MK_Screen_Question")), Surface(Required("MK_Screen_Players")));
        }

        /// <summary>The podium whose stand is closest to a position, within reach (flat distance), or -1.</summary>
        public int PodiumWithin(Vector3 position, float reach)
        {
            int best = -1;
            float bestDistance = reach;
            for (int i = 0; i < Stands.Length; i++)
            {
                Vector3 offset = position - Stands[i];
                offset.y = 0f;
                float distance = offset.magnitude;
                if (distance <= bestDistance && Mathf.Abs(position.y - Stands[i].y) < 1.5f)
                {
                    best = i;
                    bestDistance = distance;
                }
            }
            return best;
        }

        private static GameObject Required(string name)
        {
            GameObject found = GameObject.Find(name);
            if (found == null)
                throw new InvalidOperationException($"The Trivia room has no object named {name}. Is the Trivia room in the scene?");
            return found;
        }

        private static ScreenSurface Surface(GameObject marker)
        {
            Transform t = marker.transform;
            Vector3 size = t.lossyScale;
            return new ScreenSurface(t.position, t.forward, Mathf.Abs(size.x), Mathf.Abs(size.y));
        }
    }
}
