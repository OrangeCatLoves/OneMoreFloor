using System;
using System.Collections.Generic;

namespace OneMoreFloor.Floors.Trivia
{
    public enum QuestionDifficulty
    {
        Easy,
        Medium,
        Hard,
    }

    /// <summary>One question from the bank: its text, four answers and which of them is right.</summary>
    public sealed class TriviaQuestion
    {
        public const int AnswerCount = 4;

        public TriviaQuestion(string id, QuestionDifficulty difficulty, string category, string text, IReadOnlyList<string> answers, int correct)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("A question needs an id.", nameof(id));
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException($"Question {id} has no text.", nameof(text));
            if (answers == null || answers.Count != AnswerCount)
                throw new ArgumentException($"Question {id} needs exactly {AnswerCount} answers.", nameof(answers));
            foreach (string answer in answers)
                if (string.IsNullOrWhiteSpace(answer))
                    throw new ArgumentException($"Question {id} has an empty answer.", nameof(answers));
            if (correct < 0 || correct >= AnswerCount)
                throw new ArgumentOutOfRangeException(nameof(correct), correct, $"Question {id}: the right answer must be 0 to {AnswerCount - 1}.");

            Id = id;
            Difficulty = difficulty;
            Category = category ?? string.Empty;
            Text = text;
            Answers = answers;
            Correct = correct;
        }

        public string Id { get; }
        public QuestionDifficulty Difficulty { get; }
        public string Category { get; }
        public string Text { get; }
        public IReadOnlyList<string> Answers { get; }

        /// <summary>Index into <see cref="Answers"/> of the right answer.</summary>
        public int Correct { get; }
    }

    /// <summary>A question as it is asked: the same question, with its answers in the order shown on screen.</summary>
    public sealed class AskedQuestion
    {
        private readonly string[] _answers;

        public AskedQuestion(TriviaQuestion source, IReadOnlyList<int> order)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            if (order == null || order.Count != TriviaQuestion.AnswerCount)
                throw new ArgumentException("The order must list every answer once.", nameof(order));

            _answers = new string[order.Count];
            var seen = new bool[order.Count];
            for (int shown = 0; shown < order.Count; shown++)
            {
                int original = order[shown];
                if (original < 0 || original >= order.Count || seen[original])
                    throw new ArgumentException("The order must list every answer once.", nameof(order));
                seen[original] = true;
                _answers[shown] = source.Answers[original];
                if (original == source.Correct)
                    Correct = shown;
            }
        }

        public TriviaQuestion Source { get; }
        public string Text => Source.Text;
        public IReadOnlyList<string> Answers => _answers;

        /// <summary>Position on screen (0 to 3) of the right answer.</summary>
        public int Correct { get; }
    }
}
