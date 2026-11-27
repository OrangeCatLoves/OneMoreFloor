using System;
using OneMoreFloor.Game;
using UnityEngine;

namespace OneMoreFloor.Dev
{
    /// <summary>
    /// Starts a floor test when the game starts, if one was asked for: in the Editor, by the Floor Tester
    /// window; in a development build, on the command line, e.g.
    /// <c>OneMoreFloor.exe -floorTest trivia -level 2 -unlimitedHearts -seed 1234</c>.
    /// This assembly is left out of release builds.
    /// </summary>
    public static class FloorTestBoot
    {
        /// <summary>Where the Floor Tester window leaves its request for the next Play (Editor only).</summary>
        public const string SessionKey = "OneMoreFloor.FloorTest.Request";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReadRequest() => FloorTest.Current = Read();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ShowPanel()
        {
            if (FloorTest.Current != null)
                FloorTestPanel.Create();
        }

        /// <summary>Remembers a changed level for the next Play from the Floor Tester (Editor only).</summary>
        public static void Save(FloorTestRequest request)
        {
#if UNITY_EDITOR
            UnityEditor.SessionState.SetString(SessionKey, JsonUtility.ToJson(request));
#endif
        }

        private static FloorTestRequest Read()
        {
#if UNITY_EDITOR
            string json = UnityEditor.SessionState.GetString(SessionKey, string.Empty);
            return string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<FloorTestRequest>(json);
#else
            string[] args = Environment.GetCommandLineArgs();
            string floor = Argument(args, "-floorTest");
            if (string.IsNullOrEmpty(floor))
                return null;
            var request = new FloorTestRequest
            {
                floorId = floor,
                unlimitedHearts = Array.IndexOf(args, "-unlimitedHearts") >= 0,
                skipTitleCard = Array.IndexOf(args, "-titleCard") < 0,
            };
            if (int.TryParse(Argument(args, "-level"), out int level))
                request.level = Math.Max(1, level);
            if (int.TryParse(Argument(args, "-seed"), out int seed))
                request.seed = seed;
            return request;
#endif
        }

        private static string Argument(string[] args, string name)
        {
            int index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }
    }
}
