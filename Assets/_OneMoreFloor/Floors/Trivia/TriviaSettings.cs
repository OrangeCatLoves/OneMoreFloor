using System;
using OneMoreFloor.FloorSDK;
using UnityEngine;

namespace OneMoreFloor.Floors.Trivia
{
    /// <summary>Trivia's numbers from the owner's spec (docs/floors/trivia.md). Edit them in the TriviaSettings asset.</summary>
    [CreateAssetMenu(menuName = "OneMoreFloor/Trivia Settings", fileName = "TriviaSettings")]
    public sealed class TriviaSettings : ScriptableObject, IDescribesLevels
    {
        [Serializable]
        public sealed class Level
        {
            [Min(1), Tooltip("Questions to answer to clear the floor at this level.")]
            public int questions = 5;

            [Tooltip("Which questions are asked at this level.")]
            public QuestionDifficulty difficulty = QuestionDifficulty.Easy;
        }

        [SerializeField, Tooltip("One entry per difficulty level, from level 1. Past the last level the floor stays at its hardest.")]
        private Level[] levels =
        {
            new Level { questions = 5, difficulty = QuestionDifficulty.Easy },
            new Level { questions = 8, difficulty = QuestionDifficulty.Medium },
            new Level { questions = 12, difficulty = QuestionDifficulty.Hard },
        };

        [SerializeField, Min(1f), Tooltip("Seconds to vote on each question.")]
        private float secondsPerQuestion = 30f;

        [SerializeField, Min(0f), Tooltip("Seconds the votes and the right answer show before the next question.")]
        private float revealSeconds = 3f;

        [SerializeField, Min(0), Tooltip("Hearts the team loses when its answer is wrong.")]
        private int heartsForWrongAnswer = 1;

        [SerializeField, Min(0), Tooltip("Hearts the team loses when nobody answers before time runs out.")]
        private int heartsForNoAnswer = 1;

        [SerializeField, Min(0), Tooltip("Hearts the team loses when a question ties again after being asked again.")]
        private int heartsForRepeatedTie = 1;

        [SerializeField, Min(0), Tooltip("How many times a tied question is asked again before a tie costs hearts.")]
        private int tieRepeats = 1;

        [SerializeField, Min(0f), Tooltip("Once every player has voted, the question ends this many seconds later. Votes can still change until then.")]
        private float secondsAfterEveryoneVoted = 1f;

        [SerializeField, Min(0.5f), Tooltip("How close to a podium a player must stand to get into position with E, in metres.")]
        private float podiumReach = 2f;

        public float SecondsPerQuestion => secondsPerQuestion;
        public float RevealSeconds => revealSeconds;
        public float SecondsAfterEveryoneVoted => secondsAfterEveryoneVoted;
        public float PodiumReach => podiumReach;

        public Level ForLevel(int level) => levels[Mathf.Clamp(level - 1, 0, levels.Length - 1)];

        public TriviaRules Rules => new TriviaRules(heartsForWrongAnswer, heartsForNoAnswer, heartsForRepeatedTie, tieRepeats);

        public int LevelCount => levels.Length;

        // For the Floor Tester (developers only), so it isn't in the string tables.
        public string DescribeLevel(int level)
        {
            Level settings = ForLevel(level);
            return $"{settings.questions} questions, {settings.difficulty.ToString().ToLowerInvariant()}, {secondsPerQuestion:0} s each";
        }

        /// <summary>Tests only: a copy with shorter timers, so a test doesn't wait 30 s per question.</summary>
        public TriviaSettings WithTimes(float seconds, float reveal)
        {
            TriviaSettings copy = Instantiate(this);
            copy.secondsPerQuestion = seconds;
            copy.revealSeconds = reveal;
            return copy;
        }
    }
}
