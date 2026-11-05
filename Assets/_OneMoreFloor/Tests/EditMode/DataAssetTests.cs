using NUnit.Framework;
using OneMoreFloor.Core;
using OneMoreFloor.Game;
using UnityEditor;
using UnityEngine;

namespace OneMoreFloor.Tests.EditMode
{
    public class DataAssetTests
    {
        private const string DataFolder = "Assets/_OneMoreFloor/Data/";

        // The owner tunes these assets, so this checks that their values are usable, not what they are.
        [Test]
        public void GameRulesAsset_ExistsWithUsableValues()
        {
            var rules = AssetDatabase.LoadAssetAtPath<GameRules>(DataFolder + "GameRules.asset");

            Assert.That(rules, Is.Not.Null);
            Assert.That(rules.HeartsPerPlayer, Is.GreaterThanOrEqualTo(1));
            Assert.That(rules.ElevatorRideSeconds, Is.GreaterThanOrEqualTo(0f));
            Assert.That(rules.IntroSeconds, Is.GreaterThanOrEqualTo(0f));
            Assert.That(rules.CountdownSeconds, Is.GreaterThanOrEqualTo(0f));
            Assert.That(rules.RespawnDelaySeconds, Is.GreaterThanOrEqualTo(0f));
        }

        [Test]
        public void DirectorSettingsAsset_ExistsWithUsableValues()
        {
            var settings = AssetDatabase.LoadAssetAtPath<DirectorSettings>(DataFolder + "DirectorSettings.asset");

            Assert.That(settings, Is.Not.Null);
            Assert.DoesNotThrow(() => settings.ToRules());
        }

        [Test]
        public void NewAssets_StartWithTheDefaultsFromTheDesign()
        {
            var gameRules = ScriptableObject.CreateInstance<GameRules>();
            var director = ScriptableObject.CreateInstance<DirectorSettings>();

            try
            {
                DirectorRules rules = director.ToRules();

                Assert.That(gameRules.HeartsPerPlayer, Is.EqualTo(10));
                Assert.That(rules.FloorsPerDifficultyLevel, Is.EqualTo(5));
                Assert.That(rules.TierStartFloors, Is.EqualTo(new[] { 1, 11, 21 }));
            }
            finally
            {
                Object.DestroyImmediate(gameRules);
                Object.DestroyImmediate(director);
            }
        }
    }
}
