using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using OneMoreFloor.FloorSDK;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OneMoreFloor.Dev
{
    /// <summary>
    /// Lets a developer or an agent drive a game window from outside, for multiplayer tests (a Multiplayer
    /// Play Mode player has no other way in). In the Editor only, while playing, it reads commands from
    /// Temp/FloorTestRemote.txt in that Editor's project folder (each virtual player has its own), runs them,
    /// deletes the file and appends the results to Temp/FloorTestRemote.out.txt. One command per line:
    ///   place x y z yaw                         moves the local player
    ///   call Component Method [arguments...]    calls a public method on the first such component
    ///   call Component get_Property             reads a public property
    ///   shot Temp/name.png                      saves a screenshot of this window
    /// </summary>
    public sealed class FloorTestRemote : MonoBehaviour
    {
        private const string CommandsFile = "Temp/FloorTestRemote.txt";
        private const string ResultsFile = "Temp/FloorTestRemote.out.txt";
        private const float CheckEverySeconds = 0.2f;

        private float _nextCheck;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            if (!Application.isEditor || FindAnyObjectByType<FloorTestRemote>() != null)
                return;
            var go = new GameObject("FloorTestRemote");
            DontDestroyOnLoad(go);
            go.AddComponent<FloorTestRemote>();
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextCheck)
                return;
            _nextCheck = Time.unscaledTime + CheckEverySeconds;
            if (!File.Exists(CommandsFile))
                return;

            string[] commands;
            try
            {
                commands = File.ReadAllLines(CommandsFile);
                File.Delete(CommandsFile);
            }
            catch (IOException)
            {
                return; // Still being written: try again next time.
            }

            var results = new List<string>();
            foreach (string command in commands.Where(line => line.Trim().Length > 0))
            {
                string result;
                try
                {
                    result = Run(command.Trim().Split(' '));
                }
                catch (Exception exception)
                {
                    result = "error: " + (exception.InnerException ?? exception).Message;
                }
                results.Add($"{DateTime.Now:HH:mm:ss.fff} {command.Trim()} => {result}");
            }
            File.AppendAllLines(ResultsFile, results);
        }

        private static string Run(string[] words)
        {
            switch (words[0])
            {
                case "place":
                {
                    IFloorPlayer player = FloorPlayers.Local ?? throw new InvalidOperationException("no local player");
                    float[] n = words.Skip(1).Select(w => float.Parse(w, CultureInfo.InvariantCulture)).ToArray();
                    Component body = player as Component ?? throw new InvalidOperationException("the player isn't a component");
                    MethodInfo placeAt = body.GetType().GetMethod("PlaceAt") ?? throw new InvalidOperationException("the player can't be placed");
                    placeAt.Invoke(body, new object[] { new Vector3(n[0], n[1], n[2]), n[3] });
                    return "placed client " + player.ClientId;
                }
                case "call":
                {
                    Type type = FindType(words[1]);
                    Object target = FindAnyObjectByType(type) ?? throw new InvalidOperationException("no " + words[1] + " in the scene");
                    string[] arguments = words.Skip(3).ToArray();
                    MethodInfo method = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .FirstOrDefault(m => m.Name == words[2] && m.GetParameters().Count(p => !p.IsOptional) <= arguments.Length && m.GetParameters().Length >= arguments.Length)
                        ?? throw new MissingMethodException(words[1], words[2]);
                    ParameterInfo[] parameters = method.GetParameters();
                    var values = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                        values[i] = i < arguments.Length ? Convert.ChangeType(arguments[i], parameters[i].ParameterType, CultureInfo.InvariantCulture) : parameters[i].DefaultValue;
                    object returned = method.Invoke(target, values);
                    return returned is System.Collections.IEnumerable list && !(returned is string)
                        ? "[" + string.Join(",", list.Cast<object>()) + "]"
                        : returned?.ToString() ?? "done";
                }
                case "shot":
                    // A screenshot of this game window, with its on-screen text (path relative to the project).
                    ScreenCapture.CaptureScreenshot(words[1]);
                    return "saving " + words[1];
                default:
                    throw new ArgumentException("unknown command " + words[0]);
            }
        }

        private static Type FindType(string name) =>
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly.GetName().Name.StartsWith("OneMoreFloor"))
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => type.Name == name && typeof(Component).IsAssignableFrom(type))
            ?? throw new TypeLoadException("no component type " + name);
    }
}
