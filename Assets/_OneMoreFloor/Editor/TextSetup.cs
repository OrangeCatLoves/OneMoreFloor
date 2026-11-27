using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace OneMoreFloor.EditorTools
{
    /// <summary>
    /// Sets up on-screen text: TextMesh Pro's essential resources (default font and settings, in Assets/TextMesh Pro)
    /// and Unity Localization with English. String tables are made from the JSON files in
    /// Assets/_OneMoreFloor/Localization/Source, named &lt;Table&gt;.&lt;locale&gt;.json, each a flat
    /// "key": "text" object. Those files are the master copy: edit them, then run this again.
    /// </summary>
    public static class TextSetup
    {
        private const string Folder = "Assets/_OneMoreFloor/Localization";
        private const string SourceFolder = Folder + "/Source";
        private const string TablesFolder = Folder + "/Tables";
        private const string SettingsPath = Folder + "/LocalizationSettings.asset";
        private const string LocalesFolder = Folder + "/Locales";

        [MenuItem("OneMoreFloor/Setup/Text and Localization")]
        public static void Run()
        {
            ImportTextMeshProEssentials();
            LocalizationSettings settings = EnsureSettings();
            foreach (string path in Directory.GetFiles(SourceFolder, "*.json").OrderBy(p => p))
                SyncTable(path.Replace('\\', '/'));
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log("TEXT_SETUP_DONE");
        }

        private static void ImportTextMeshProEssentials()
        {
            if (Resources.Load<TMP_Settings>("TMP Settings") != null)
                return;
            TMP_PackageResourceImporter.ImportResources(importEssentials: true, importExamples: false, interactive: false);
            AssetDatabase.Refresh();
        }

        private static LocalizationSettings EnsureSettings()
        {
            LocalizationSettings settings = LocalizationEditorSettings.ActiveLocalizationSettings;
            if (settings == null)
            {
                settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(SettingsPath);
                if (settings == null)
                {
                    settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                    settings.name = "LocalizationSettings";
                    AssetDatabase.CreateAsset(settings, SettingsPath);
                }
                LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            }

            // Text is looked up as it's needed, so the tables must be ready straight away.
            LocalizationSettings.InitializeSynchronously = true;

            foreach (string code in Directory.GetFiles(SourceFolder, "*.json").Select(p => Path.GetFileNameWithoutExtension(p).Split('.').Last()).Distinct())
                EnsureLocale(code);
            Locale english = LocalizationEditorSettings.GetLocale(new LocaleIdentifier("en"));
            if (english != null)
                LocalizationSettings.ProjectLocale = english;
            return settings;
        }

        private static Locale EnsureLocale(string code)
        {
            var identifier = new LocaleIdentifier(code);
            Locale locale = LocalizationEditorSettings.GetLocale(identifier);
            if (locale != null)
                return locale;

            Directory.CreateDirectory(LocalesFolder);
            locale = Locale.CreateLocale(identifier);
            locale.name = $"{(string.IsNullOrEmpty(locale.LocaleName) ? code : locale.LocaleName)} ({code})";
            AssetDatabase.CreateAsset(locale, $"{LocalesFolder}/{locale.name}.asset");
            LocalizationEditorSettings.AddLocale(locale);
            return locale;
        }

        private static void SyncTable(string path)
        {
            string[] parts = Path.GetFileNameWithoutExtension(path).Split('.');
            string tableName = parts[0];
            var identifier = new LocaleIdentifier(parts[parts.Length - 1]);

            Directory.CreateDirectory(TablesFolder);
            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(tableName)
                ?? LocalizationEditorSettings.CreateStringTableCollection(tableName, TablesFolder);
            var table = collection.GetTable(identifier) as StringTable ?? collection.AddNewTable(identifier) as StringTable;

            JObject entries = JObject.Parse(File.ReadAllText(path));
            foreach (JProperty property in entries.Properties())
            {
                string text = (string)property.Value;
                StringTableEntry entry = table.GetEntry(property.Name);
                if (entry == null)
                    table.AddEntry(property.Name, text);
                else if (entry.Value != text)
                    entry.Value = text;
            }

            // Keys removed from the source go from the table too.
            foreach (string key in collection.SharedData.Entries.Select(e => e.Key).ToList())
                if (entries[key] == null)
                    collection.RemoveEntry(key);

            EditorUtility.SetDirty(table);
            EditorUtility.SetDirty(collection.SharedData);
            EditorUtility.SetDirty(collection);
        }
    }
}
