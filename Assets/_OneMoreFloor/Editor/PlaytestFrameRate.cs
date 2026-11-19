using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OneMoreFloor.EditorTools
{
    /// <summary>
    /// Paces Play mode in the Editor. Players get VSync: one frame per screen refresh. The Editor's Game
    /// view ignores that setting and draws as fast as it can, so motion judders. By default this turns on
    /// the Game view's own VSync when Play starts.
    /// A machine watched through a stream (the home PC, through Parsec) is better capped to the stream's
    /// frame rate, so each streamed frame shows the same amount of movement. That choice is saved per
    /// machine: menu OneMoreFloor > Playtest Frame Rate.
    /// </summary>
    [InitializeOnLoad]
    public static class PlaytestFrameRate
    {
        public const string StreamFrameRateKey = "OneMoreFloor.PlaytestStreamFrameRate";

        private const string MenuRoot = "OneMoreFloor/Playtest Frame Rate/";
        private const string ScreenMenu = MenuRoot + "Match the Screen (VSync)";
        private const string Stream60Menu = MenuRoot + "Parsec Stream at 60 fps";
        private const string Stream120Menu = MenuRoot + "Parsec Stream at 120 fps";

        static PlaytestFrameRate()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.update += KeepStreamRate;
        }

        public static int StreamFrameRate => EditorPrefs.GetInt(StreamFrameRateKey, 0);

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (Application.isBatchMode)
                return;

            if (change == PlayModeStateChange.EnteredPlayMode)
                Apply();
            else if (change == PlayModeStateChange.ExitingPlayMode)
                Application.targetFrameRate = -1;
        }

        // FishNet sets its own frame rate cap when the network starts (500 by default), after Play has begun.
        // Built games use VSync, which overrides that cap; in the Editor, the stream rate is put back.
        private static void KeepStreamRate()
        {
            if (!EditorApplication.isPlaying || Application.isBatchMode)
                return;

            int streamRate = StreamFrameRate;
            if (streamRate > 0 && Application.targetFrameRate != streamRate)
                Application.targetFrameRate = streamRate;
        }

        private static void Apply()
        {
            int streamRate = StreamFrameRate;
            SetGameViewVSync(streamRate <= 0);
            Application.targetFrameRate = streamRate > 0 ? streamRate : -1;
        }

        private static void SetGameViewVSync(bool on)
        {
            // The Game view's VSync switch has no public API.
            Type gameView = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            PropertyInfo vSync = gameView?.GetProperty("vSyncEnabled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (vSync == null)
            {
                Debug.LogWarning("Couldn't set the Game view's VSync: this Unity version has no GameView.vSyncEnabled. Tick VSync in the Game view's toolbar instead.");
                return;
            }

            foreach (Object view in Resources.FindObjectsOfTypeAll(gameView))
                vSync.SetValue(view, on);
        }

        private static void SetStreamRate(int rate)
        {
            EditorPrefs.SetInt(StreamFrameRateKey, rate);
            if (EditorApplication.isPlaying)
                Apply();
        }

        [MenuItem(ScreenMenu, priority = 100)] private static void UseScreen() => SetStreamRate(0);
        [MenuItem(Stream60Menu, priority = 101)] private static void UseStream60() => SetStreamRate(60);
        [MenuItem(Stream120Menu, priority = 102)] private static void UseStream120() => SetStreamRate(120);

        [MenuItem(ScreenMenu, true)]
        private static bool ShowChecks()
        {
            int rate = StreamFrameRate;
            Menu.SetChecked(ScreenMenu, rate <= 0);
            Menu.SetChecked(Stream60Menu, rate == 60);
            Menu.SetChecked(Stream120Menu, rate == 120);
            return true;
        }
    }
}
