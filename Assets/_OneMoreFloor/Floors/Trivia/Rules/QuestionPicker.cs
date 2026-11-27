using System;
using System.Collections.Generic;
using OneMoreFloor.Core;

namespace OneMoreFloor.Floors.Trivia
{
    /// <summary>
    /// Picks each Trivia floor's questions from the bank for one run. A question is never asked twice in
    /// a run while unused ones remain. The level's difficulty comes first; if those run out, the nearest
    /// other difficulty is used, and only when the whole bank has been asked do questions repeat.
    /// Answers are shuffled, so the right one isn't always in the same place.
    /// </summary>
    public sealed class QuestionPicker
    {
        private readonly IReadOnlyList<TriviaQuestion> _bank;
        private readonly HashSet<string> _asked = new HashSet<string>();

        public QuestionPicker(IReadOnlyList<TriviaQuestion> bank)
        {
            if (bank == null || bank.Count == 0)
                throw new ArgumentException("The question bank is empty.", nameof(bank));
            _bank = bank;
        }

        public int AskedCount => _asked.Count;

        public IReadOnlyList<AskedQuestion> Pick(int count, QuestionDifficulty difficulty, SeededRandom random)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Must be at least 1.");
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            var chosen = new List<TriviaQuestion>(count);
            foreach (QuestionDifficulty preferred in PreferenceOrder(difficulty))
            {
                var unasked = _bank.FindAll(q => q.Difficulty == preferred && !_asked.Contains(q.Id));
                Shuffle(unasked, random);
                foreach (TriviaQuestion question in unasked)
                {
                    if (chosen.Count == count)
                        break;
                    chosen.Add(question);
                }
                if (chosen.Count == count)
                    break;
            }

            if (chosen.Count < count)
            {
                // The whole bank has been asked this run: repeats are unavoidable. Never repeat within one floor.
                var repeats = _bank.FindAll(q => !chosen.Contains(q));
                Shuffle(repeats, random);
                foreach (TriviaQuestion question in repeats)
                {
                    if (chosen.Count == count)
                        break;
                    chosen.Add(question);
                }
            }

            var asked = new List<AskedQuestion>(chosen.Count);
            foreach (TriviaQuestion question in chosen)
            {
                _asked.Add(question.Id);
                var order = new List<int> { 0, 1, 2, 3 };
                Shuffle(order, random);
                asked.Add(new AskedQuestion(question, order));
            }
            return asked;
        }

        private static IEnumerable<QuestionDifficulty> PreferenceOrder(QuestionDifficulty difficulty)
        {
            switch (difficulty)
            {
                case QuestionDifficulty.Easy:
                    return new[] { QuestionDifficulty.Easy, QuestionDifficulty.Medium, QuestionDifficulty.Hard };
                case QuestionDifficulty.Medium:
                    return new[] { QuestionDifficulty.Medium, QuestionDifficulty.Easy, QuestionDifficulty.Hard };
                default:
                    return new[] { QuestionDifficulty.Hard, QuestionDifficulty.Medium, QuestionDifficulty.Easy };
            }
        }

        // Fisher-Yates, driven only by the run's random generator.
        private static void Shuffle<T>(IList<T> items, SeededRandom random)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int j = random.NextInt(i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
        }
    }

    internal static class ListExtensions
    {
        public static List<T> FindAll<T>(this IReadOnlyList<T> items, Predicate<T> match)
        {
            var found = new List<T>();
            foreach (T item in items)
                if (match(item))
                    found.Add(item);
            return found;
        }
    }
}
