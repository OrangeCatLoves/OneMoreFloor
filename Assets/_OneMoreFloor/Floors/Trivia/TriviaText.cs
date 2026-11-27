using OneMoreFloor.FloorSDK;

namespace OneMoreFloor.Floors.Trivia
{
    /// <summary>Keys of Trivia's text in its string table (Localization/Source/Trivia.en.json).</summary>
    public static class TriviaText
    {
        public const string Table = "Trivia";

        public const string Name = "name";
        public const string Goal = "goal";
        public const string QuestionOf = "question_of";
        public const string AskedAgain = "asked_again";
        public const string GetInPosition = "get_in_position";
        public const string InPosition = "in_position";
        public const string WaitingForTeam = "waiting_for_team";
        public const string EveryoneReady = "everyone_ready";
        public const string Ready = "ready";
        public const string PressE = "press_e";
        public const string PodiumTaken = "podium_taken";
        public const string StepOut = "step_out";
        public const string Choose = "choose";
        public const string VotesOne = "votes_one";
        public const string VotesMany = "votes_many";
        public const string OutcomeCorrect = "outcome_correct";
        public const string OutcomeWrong = "outcome_wrong";
        public const string OutcomeNobody = "outcome_nobody";
        public const string OutcomeTieAgain = "outcome_tie_again";
        public const string OutcomeTieCosts = "outcome_tie_costs";
        public const string HeartsLost = "hearts_lost";
        public const string PodiumFree = "podium_free";
        public const string PodiumPlayer = "podium_player";
        public const string YourAnswer = "your_answer";
        public const string AllAnswered = "all_answered";

        public static string Get(string key, params object[] args) => UiText.Get(Table, key, args);

        public static string Votes(int count) => count == 1 ? Get(VotesOne) : Get(VotesMany, count);

        public static string Outcome(VoteOutcome outcome)
        {
            switch (outcome)
            {
                case VoteOutcome.Correct: return Get(OutcomeCorrect);
                case VoteOutcome.Wrong: return Get(OutcomeWrong);
                case VoteOutcome.NobodyAnswered: return Get(OutcomeNobody);
                case VoteOutcome.TieAskAgain: return Get(OutcomeTieAgain);
                default: return Get(OutcomeTieCosts);
            }
        }
    }
}
