using System;
using System.Collections.Generic;
using UnityEngine;

namespace OneMoreFloor.Floors.Trivia
{
    /// <summary>
    /// Reads the question bank (Data/TriviaQuestions.json). Each entry lists the right answer and three wrong
    /// ones; the game shuffles their order when it asks. Difficulty is easy, medium or hard.
    /// </summary>
    public static class TriviaQuestionBank
    {
        [Serializable]
        private sealed class BankFile
        {
            public Entry[] questions;
        }

        [Serializable]
        private sealed class Entry
        {
            public string id;
            public string difficulty;
            public string category;
            public string question;
            public string answer;
            public string[] wrong;
        }

        public static List<TriviaQuestion> Parse(string json)
        {
            BankFile file = JsonUtility.FromJson<BankFile>(json);
            if (file?.questions == null)
                throw new FormatException("The question bank has no \"questions\" list.");

            var questions = new List<TriviaQuestion>(file.questions.Length);
            foreach (Entry entry in file.questions)
            {
                if (entry.wrong == null || entry.wrong.Length != 3)
                    throw new FormatException($"Question {entry.id} needs exactly 3 wrong answers.");
                var answers = new[] { entry.answer, entry.wrong[0], entry.wrong[1], entry.wrong[2] };
                questions.Add(new TriviaQuestion(entry.id, ParseDifficulty(entry.id, entry.difficulty), entry.category, entry.question, answers, 0));
            }
            return questions;
        }

        private static QuestionDifficulty ParseDifficulty(string id, string value)
        {
            switch ((value ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "easy": return QuestionDifficulty.Easy;
                case "medium": return QuestionDifficulty.Medium;
                case "hard": return QuestionDifficulty.Hard;
                default: throw new FormatException($"Question {id}: difficulty must be easy, medium or hard, not \"{value}\".");
            }
        }
    }
}
