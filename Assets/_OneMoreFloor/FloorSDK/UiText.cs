using System;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace OneMoreFloor.FloorSDK
{
    /// <summary>
    /// On-screen text from the string tables (Unity Localization; English at launch). Never hard-code text:
    /// add a key to Assets/_OneMoreFloor/Localization/Source/&lt;Table&gt;.en.json and run
    /// OneMoreFloor > Setup > Text and Localization.
    /// </summary>
    public static class UiText
    {
        public static string Get(string table, string key, params object[] args)
        {
            string text = null;
            try
            {
                text = LocalizationSettings.StringDatabase.GetLocalizedString(table, key);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Missing text {table}/{key}: {exception.Message}");
            }

            // A missing entry shows its key, so it's easy to spot and fix.
            if (string.IsNullOrEmpty(text))
                text = $"[{table}/{key}]";
            return args != null && args.Length > 0 ? string.Format(text, args) : text;
        }
    }
}
