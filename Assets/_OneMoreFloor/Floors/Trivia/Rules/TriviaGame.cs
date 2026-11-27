using System;
using System.Collections.Generic;

namespace OneMoreFloor.Floors.Trivia
{
    /// <summary>The heart costs and tie rule from the owner's spec (docs/floors/trivia.md).</summary>
    public sealed class TriviaRules
    {
        public TriviaRules(int heartsForWrongAnswer, int heartsForNoAnswer, int heartsForRepeatedTie, int tieRepeats)
        {
            if (heartsForWrongAnswer < 0 || heartsForNoAnswer < 0 || heartsForRepeatedTie < 0)
                throw new ArgumentOutOfRangeException(nameof(heartsForWrongAnswer), "Heart costs can't be negative.");
            if (tieRepeats < 0)
                throw new ArgumentOutOfRangeException(nameof(tieRepeats), tieRepeats, "Can't be negative.");

            HeartsForWrongAnswer = heartsForWrongAnswer;
            HeartsForNoAnswer = heartsForNoAnswer;
            HeartsForRepeatedTie = heartsForRepeatedTie;
            TieRepeats = tieRepeats;
        }

        public int HeartsForWrongAnswer { get; }
        public int HeartsForNoAnswer { get; }
        public int HeartsForRepeatedTie { get; }

        /// <summary>How many times a tied question is asked again before a tie costs hearts.</summary>
        public int TieRepeats { get; }
    }

    public enum VoteOutcome
    {
        /// <summary>The option with the most votes is right.</summary>
        Correct,

        /// <summary>The option with the most votes is wrong.</summary>
        Wrong,

        /// <summary>Nobody voted before time ran out.</summary>
        NobodyAnswered,

        /// <summary>Two or more options share the most votes: the same question is asked again.</summary>
        TieAskAgain,

        /// <summary>The question tied again after being asked again: it costs hearts and the floor moves on.</summary>
        TieCostsHearts,
    }

    /// <summary>What happened when a question's time ran out.</summary>
    public sealed class VoteResult
    {
        public VoteResult(VoteOutcome outcome, IReadOnlyList<int> counts, int teamAnswer, int correct, int heartsLost)
        {
            Outcome = outcome;
            Counts = counts;
            TeamAnswer = teamAnswer;
            Correct = correct;
            HeartsLost = heartsLost;
        }

        public VoteOutcome Outcome { get; }

        /// <summary>Votes for each answer position (0 to 3).</summary>
        public IReadOnlyList<int> Counts { get; }

        /// <summary>The answer position with the most votes, or -1 for no answer or a tie.</summary>
        public int TeamAnswer { get; }

        public int Correct { get; }
        public int HeartsLost { get; }
        public bool AsksAgain => Outcome == VoteOutcome.TieAskAgain;
    }

    /// <summary>
    /// The questions and votes of one Trivia floor. The host runs it and players only send votes.
    /// Votes stay private until time runs out, and a player may change theirs until then. A player
    /// who doesn't vote (or steps away) simply isn't counted, so their weight goes to the others.
    /// </summary>
    public sealed class TriviaGame
    {
        private readonly IReadOnlyList<AskedQuestion> _questions;
        private readonly TriviaRules _rules;
        private readonly Dictionary<int, int> _votes = new Dictionary<int, int>();
        private int _index;

        public TriviaGame(IReadOnlyList<AskedQuestion> questions, TriviaRules rules)
        {
            if (questions == null || questions.Count == 0)
                throw new ArgumentException("A Trivia floor needs at least one question.", nameof(questions));
            _questions = questions;
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public int QuestionCount => _questions.Count;

        /// <summary>1 for the first question. Stays the same while a tied question is asked again.</summary>
        public int QuestionNumber => Math.Min(_index + 1, _questions.Count);

        public bool IsFinished => _index >= _questions.Count;
        public AskedQuestion Current => IsFinished ? null : _questions[_index];
        public int TiesOnThisQuestion { get; private set; }
        public int VoteCount => _votes.Count;

        /// <summary>Records or changes a player's vote for an answer position (0 to 3).</summary>
        public bool Vote(int playerId, int answer)
        {
            if (IsFinished || answer < 0 || answer >= TriviaQuestion.AnswerCount)
                return false;
            _votes[playerId] = answer;
            return true;
        }

        /// <summary>A player stepped out of position or left: their vote no longer counts.</summary>
        public void Withdraw(int playerId) => _votes.Remove(playerId);

        public bool TryGetVote(int playerId, out int answer) => _votes.TryGetValue(playerId, out answer);

        /// <summary>Time is up: decide the team's answer, then move on (or ask the same question again after a first tie).</summary>
        public VoteResult CloseVoting()
        {
            if (IsFinished)
                throw new InvalidOperationException("Every question has been asked.");

            var counts = new int[TriviaQuestion.AnswerCount];
            foreach (int answer in _votes.Values)
                counts[answer]++;

            int correct = Current.Correct;
            int teamAnswer = Winner(counts, out bool tie);
            VoteResult result;
            if (_votes.Count == 0)
            {
                result = new VoteResult(VoteOutcome.NobodyAnswered, counts, -1, correct, _rules.HeartsForNoAnswer);
            }
            else if (tie && TiesOnThisQuestion < _rules.TieRepeats)
            {
                result = new VoteResult(VoteOutcome.TieAskAgain, counts, -1, correct, 0);
            }
            else if (tie)
            {
                result = new VoteResult(VoteOutcome.TieCostsHearts, counts, -1, correct, _rules.HeartsForRepeatedTie);
            }
            else
            {
                bool right = teamAnswer == correct;
                result = new VoteResult(right ? VoteOutcome.Correct : VoteOutcome.Wrong, counts, teamAnswer, correct, right ? 0 : _rules.HeartsForWrongAnswer);
            }

            _votes.Clear();
            if (result.AsksAgain)
            {
                TiesOnThisQuestion++;
            }
            else
            {
                _index++;
                TiesOnThisQuestion = 0;
            }
            return result;
        }

        /// <summary>The answer with the most votes, or -1 when nobody voted or the most votes are shared.</summary>
        public static int Winner(IReadOnlyList<int> counts, out bool tie)
        {
            int best = -1, bestCount = 0;
            tie = false;
            for (int i = 0; i < counts.Count; i++)
            {
                if (counts[i] > bestCount)
                {
                    best = i;
                    bestCount = counts[i];
                    tie = false;
                }
                else if (counts[i] == bestCount && bestCount > 0)
                {
                    tie = true;
                }
            }
            return tie ? -1 : best;
        }
    }
}
