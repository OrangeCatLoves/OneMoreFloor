using UnityEditor;
using UnityEditor.Localization;

namespace OneMoreFloor.EditorTools
{
    /// <summary>
    /// The game has only English for now, so the Localization package's language menu in the Game view is
    /// off by default. That menu also throws an editor error when Play mode starts in some cases (for example
    /// during PlayMode test runs). Anyone can still turn it back on: Edit > Preferences > Localization.
    /// </summary>
    [InitializeOnLoad]
    public static class LocalizationEditorDefaults
    {
        private const string GameViewMenuPreference = "Localization-ShowLocaleMenuInGameView";

        static LocalizationEditorDefaults()
        {
            if (!EditorPrefs.HasKey(GameViewMenuPreference))
                LocalizationEditorSettings.ShowLocaleMenuInGameView = false;
        }
    }
}
