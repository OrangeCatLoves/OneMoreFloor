using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OneMoreFloor.Core;
using OneMoreFloor.Floors.Trivia;

namespace OneMoreFloor.Tests.EditMode
{
    public class TriviaRulesTests
    {
        private static readonly TriviaRules Rules = new TriviaRules(heartsForWrongAnswer: 1, heartsForNoAnswer: 1, heartsForRepeatedTie: 1, tieRepeats: 1);

        private static TriviaQuestion Question(string id, QuestionDifficulty difficulty = QuestionDifficulty.Easy, int correct = 0) =>
            new TriviaQuestion(id, difficulty, "test", "Question " + id + "?", new[] { "A" + id, "B" + id, "C" + id, "D" + id }, correct);

        // Questions asked in their original order, so answer position 0 is right unless stated.
        private static TriviaGame Game(int count) =>
            new TriviaGame(Enumerable.Range(1, count).Select(i => new AskedQuestion(Question("q" + i), new[] { 0, 1, 2, 3 })).ToList(), Rules);

        [Test]
        public void TheAnswerWithMostVotesIsTheTeamsAnswer_Even211()
        {
            var game = Game(2);
            game.Vote(1, 0);
            game.Vote(2, 0);
            game.Vote(3, 1);
            game.Vote(4, 2);

            VoteResult result = game.CloseVoting();

            Assert.That(result.Outcome, Is.EqualTo(VoteOutcome.Correct));
            Assert.That(result.TeamAnswer, Is.EqualTo(0));
            Assert.That(result.Counts, Is.EqualTo(new[] { 2, 1, 1, 0 }));
            Assert.That(result.HeartsLost, Is.EqualTo(0));
            Assert.That(game.QuestionNumber, Is.EqualTo(2));
        }

        [Test]
        public void AWrongTeamAnswer_CostsOneHeart_AndMovesOn()
        {
            var game = Game(2);
            game.Vote(1, 3);

            VoteResult result = game.CloseVoting();

            Assert.That(result.Outcome, Is.EqualTo(VoteOutcome.Wrong));
            Assert.That(result.HeartsLost, Is.EqualTo(1));
            Assert.That(game.QuestionNumber, Is.EqualTo(2));
        }

        [Test]
        public void NobodyAnswering_CostsOneHeart_AndMovesOn()
        {
            var game = Game(2);

            VoteResult result = game.CloseVoting();

            Assert.That(result.Outcome, Is.EqualTo(VoteOutcome.NobodyAnswered));
            Assert.That(result.HeartsLost, Is.EqualTo(1));
            Assert.That(game.QuestionNumber, Is.EqualTo(2));
        }

        [Test]
        public void AFirstTie_AsksTheSameQuestionAgain_ForFree()
        {
            var game = Game(2);
            game.Vote(1, 0);
            game.Vote(2, 1);

            VoteResult result = game.CloseVoting();

            Assert.That(result.Outcome, Is.EqualTo(VoteOutcome.TieAskAgain));
            Assert.That(result.HeartsLost, Is.EqualTo(0));
            Assert.That(game.QuestionNumber, Is.EqualTo(1));
            Assert.That(game.TiesOnThisQuestion, Is.EqualTo(1));
            Assert.That(game.VoteCount, Is.EqualTo(0), "Votes start fresh when the question is asked again.");
        }

        [Test]
        public void ASecondTieOnTheSameQuestion_CostsOneHeart_AndMovesOn()
        {
            var game = Game(2);
            game.Vote(1, 0);
            game.Vote(2, 1);
            game.CloseVoting();
            game.Vote(1, 2);
            game.Vote(2, 3);

            VoteResult result = game.CloseVoting();

            Assert.That(result.Outcome, Is.EqualTo(VoteOutcome.TieCostsHearts));
            Assert.That(result.HeartsLost, Is.EqualTo(1));
            Assert.That(game.QuestionNumber, Is.EqualTo(2));
            Assert.That(game.TiesOnThisQuestion, Is.EqualTo(0));
        }

        [Test]
        public void ATieAnsweredRightOnTheRepeat_IsFree_AndResetsTheTieCount()
        {
            var game = Game(2);
            game.Vote(1, 0);
            game.Vote(2, 1);
            game.CloseVoting();
            game.Vote(1, 0);
            game.Vote(2, 0);

            VoteResult result = game.CloseVoting();

            Assert.That(result.Outcome, Is.EqualTo(VoteOutcome.Correct));
            Assert.That(game.TiesOnThisQuestion, Is.EqualTo(0));
            Assert.That(game.QuestionNumber, Is.EqualTo(2));
        }

        [Test]
        public void AVoteCanBeChangedUntilTimeRunsOut()
        {
            var game = Game(1);
            game.Vote(1, 2);
            game.Vote(1, 0);

            Assert.That(game.CloseVoting().Outcome, Is.EqualTo(VoteOutcome.Correct));
        }

        [Test]
        public void APlayerWhoStepsAway_NoLongerCounts_SoTheOthersDecide()
        {
            var game = Game(1);
            game.Vote(1, 0);
            game.Vote(2, 3);
            game.Withdraw(2);

            VoteResult result = game.CloseVoting();

            Assert.That(result.Outcome, Is.EqualTo(VoteOutcome.Correct));
            Assert.That(result.Counts, Is.EqualTo(new[] { 1, 0, 0, 0 }));
        }

        [Test]
        public void VotesOutsideTheFourAnswers_AreIgnored()
        {
            var game = Game(1);

            Assert.That(game.Vote(1, 4), Is.False);
            Assert.That(game.Vote(1, -1), Is.False);
            Assert.That(game.VoteCount, Is.EqualTo(0));
        }

        [Test]
        public void TheFloorFinishes_AfterItsLastQuestion()
        {
            var game = Game(3);
            for (int i = 0; i < 3; i++)
            {
                Assert.That(game.IsFinished, Is.False);
                game.Vote(1, 0);
                game.CloseVoting();
            }

            Assert.That(game.IsFinished, Is.True);
            Assert.That(game.Vote(1, 0), Is.False);
        }

        [Test]
        public void ShuffledAnswers_StillPointAtTheRightOne()
        {
            var asked = new AskedQuestion(Question("x", correct: 2), new[] { 3, 2, 1, 0 });

            Assert.That(asked.Answers, Is.EqualTo(new[] { "Dx", "Cx", "Bx", "Ax" }));
            Assert.That(asked.Correct, Is.EqualTo(1));
            Assert.That(asked.Answers[asked.Correct], Is.EqualTo("Cx"));
        }

        [Test]
        public void Picker_UsesTheLevelsDifficulty_AndNeverRepeatsWithinARun()
        {
            var bank = Enumerable.Range(1, 10).Select(i => Question("e" + i, QuestionDifficulty.Easy))
                .Concat(Enumerable.Range(1, 10).Select(i => Question("h" + i, QuestionDifficulty.Hard))).ToList();
            var picker = new QuestionPicker(bank);
            var random = new SeededRandom(42);

            var first = picker.Pick(5, QuestionDifficulty.Easy, random);
            var second = picker.Pick(5, QuestionDifficulty.Easy, random);

            Assert.That(first.Concat(second).All(q => q.Source.Difficulty == QuestionDifficulty.Easy), Is.True);
            Assert.That(first.Concat(second).Select(q => q.Source.Id).Distinct().Count(), Is.EqualTo(10));
        }

        [Test]
        public void Picker_BorrowsTheNearestDifficulty_ThenRepeatsOnlyWhenTheBankIsUsedUp()
        {
            var bank = Enumerable.Range(1, 3).Select(i => Question("e" + i, QuestionDifficulty.Easy))
                .Concat(Enumerable.Range(1, 3).Select(i => Question("m" + i, QuestionDifficulty.Medium)))
                .Concat(Enumerable.Range(1, 3).Select(i => Question("h" + i, QuestionDifficulty.Hard))).ToList();
            var picker = new QuestionPicker(bank);
            var random = new SeededRandom(7);

            var easyFloor = picker.Pick(5, QuestionDifficulty.Easy, random);
            Assert.That(easyFloor.Count(q => q.Source.Difficulty == QuestionDifficulty.Easy), Is.EqualTo(3));
            Assert.That(easyFloor.Count(q => q.Source.Difficulty == QuestionDifficulty.Medium), Is.EqualTo(2), "Medium is nearest to easy.");

            var later = picker.Pick(6, QuestionDifficulty.Hard, random);
            Assert.That(later.Select(q => q.Source.Id).Distinct().Count(), Is.EqualTo(6), "No repeats within one floor.");
            Assert.That(picker.AskedCount, Is.EqualTo(9));
        }

        [Test]
        public void Picker_IsTheSameForTheSameSeed()
        {
            var bank = Enumerable.Range(1, 20).Select(i => Question("q" + i)).ToList();

            string Run(ulong seed) => string.Join(",", new QuestionPicker(bank).Pick(8, QuestionDifficulty.Easy, new SeededRandom(seed))
                .Select(q => q.Source.Id + ":" + string.Join("", q.Answers)));

            Assert.That(Run(99), Is.EqualTo(Run(99)));
            Assert.That(Run(99), Is.Not.EqualTo(Run(100)));
        }
    }
}
