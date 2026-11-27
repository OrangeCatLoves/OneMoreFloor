using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using OneMoreFloor.Floors.Trivia;

namespace OneMoreFloor.Tests.EditMode
{
    /// <summary>On-screen text and Trivia's question bank: everything the game shows must exist and be ready.</summary>
    public class TextAndContentTests
    {
        private const string SourceFolder = "Assets/_OneMoreFloor/Localization/Source";
        private const string TablesFolder = "Assets/_OneMoreFloor/Localization/Tables";
        private const string QuestionsPath = "Assets/_OneMoreFloor/Floors/Trivia/Data/TriviaQuestions.json";

        private static readonly Dictionary<string, string[]> KeysUsedInCode = new Dictionary<string, string[]>
        {
            ["Game"] = new[] { "hearts", "floor", "hearts_lost", "floor_cleared", "game_over", "results_title", "results_floors", "play_again" },
            ["Dev"] = new[] { "title", "floor_level", "restart", "level_down", "level_up", "clear", "lose_heart", "hearts_unlimited", "hearts_normal", "mouse_hint" },
            ["Trivia"] = typeof(TriviaText).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.IsLiteral && field.Name != nameof(TriviaText.Table))
                .Select(field => (string)field.GetRawConstantValue())
                .ToArray(),
        };

        private static string[] SourceKeys(string table) =>
            Regex.Matches(File.ReadAllText($"{SourceFolder}/{table}.en.json"), "^\\s*\"([a-z0-9_]+)\"\\s*:", RegexOptions.Multiline)
                .Cast<Match>().Select(match => match.Groups[1].Value).ToArray();

        [Test]
        public void EveryKeyTheCodeUses_IsInTheStringTableSources()
        {
            foreach (KeyValuePair<string, string[]> table in KeysUsedInCode)
                Assert.That(SourceKeys(table.Key), Is.SupersetOf(table.Value), $"Missing text in {table.Key}.en.json");
        }

        // The tables are made from the sources by OneMoreFloor > Setup > Text and Localization: run it after editing them.
        [Test]
        public void TheStringTables_AreUpToDateWithTheSources()
        {
            foreach (string table in KeysUsedInCode.Keys)
            {
                string shared = $"{TablesFolder}/{table} Shared Data.asset";
                Assert.That(File.Exists(shared), Is.True, $"{table} has no string table yet: run OneMoreFloor > Setup > Text and Localization.");
                string tableKeys = File.ReadAllText(shared);
                foreach (string key in SourceKeys(table))
                    Assert.That(tableKeys, Does.Contain("m_Key: " + key + "\n").Or.Contain("m_Key: " + key + "\r"),
                        $"{table}/{key} isn't in the string table: run OneMoreFloor > Setup > Text and Localization.");
            }
        }

        [Test]
        public void TheQuestionBank_HasEnoughQuestionsForEveryLevel()
        {
            List<TriviaQuestion> bank = TriviaQuestionBank.Parse(File.ReadAllText(QuestionsPath));

            Assert.That(bank.Select(q => q.Id).Distinct().Count(), Is.EqualTo(bank.Count), "Question ids must be unique.");
            Assert.That(bank.Select(q => q.Text).Distinct().Count(), Is.EqualTo(bank.Count), "The same question is in the bank twice.");
            foreach (TriviaQuestion question in bank)
                Assert.That(question.Answers.Distinct().Count(), Is.EqualTo(TriviaQuestion.AnswerCount), $"{question.Id} repeats an answer.");

            // The longest floor (level 3 and up) asks 12 questions.
            foreach (QuestionDifficulty difficulty in new[] { QuestionDifficulty.Easy, QuestionDifficulty.Medium, QuestionDifficulty.Hard })
                Assert.That(bank.Count(q => q.Difficulty == difficulty), Is.GreaterThanOrEqualTo(12), $"Too few {difficulty} questions.");
        }
    }
}
