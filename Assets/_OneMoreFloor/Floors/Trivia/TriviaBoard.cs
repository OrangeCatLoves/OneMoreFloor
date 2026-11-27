using System;
using System.Collections.Generic;
using OneMoreFloor.FloorSDK;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneMoreFloor.Floors.Trivia
{
    public enum BoardStyle
    {
        /// <summary>The player's own screen while in position: clickable answers.</summary>
        Hud,

        /// <summary>The big screen behind the podiums and the players' TV (same content).</summary>
        Screen,

        /// <summary>A podium's small screen.</summary>
        Podium,
    }

    /// <summary>Everything a board shows. The presenter fills one in each frame.</summary>
    public sealed class BoardView
    {
        public bool Active;
        public TriviaStage Stage;
        public int Round;
        public int Number;
        public int Count;
        public bool AskedAgain;
        public string Question = string.Empty;
        public IReadOnlyList<string> Answers;
        public float SecondsLeft;
        public float SecondsTotal = 1f;
        public int Correct = -1;
        public IReadOnlyList<int> Counts;
        public VoteOutcome Outcome;
        public int HeartsLost;

        /// <summary>The answer to frame: the viewer's own vote (HUD), or this podium player's vote once revealed.</summary>
        public int Chosen = -1;

        public int InPosition;
        public int Players;
        public bool EveryoneInPosition;

        /// <summary>Podium boards: the number shown for the player at this podium, or 0 when it's free.</summary>
        public int PodiumPlayer;
    }

    /// <summary>
    /// One display of the question, its four answers, the timer and the result, built in code for the HUD,
    /// the room's big screen and TV, and the podium screens. All its text comes from the string tables.
    /// </summary>
    public sealed class TriviaBoard
    {
        private static readonly Color Background = new Color(0.07f, 0.03f, 0.16f, 1f);
        private static readonly Color HudBackground = new Color(0.1f, 0.03f, 0.12f, 0.9f);
        private static readonly Color Good = new Color(0.35f, 0.95f, 0.45f);
        private static readonly Color Bad = new Color(1f, 0.38f, 0.38f);

        private readonly BoardStyle _style;
        private readonly float _brightness;
        private readonly RectTransform _questionArea;
        private readonly RectTransform _answersArea;
        private readonly TextMeshProUGUI _header;
        private readonly TextMeshProUGUI _timer;
        private readonly Image _timerFill;
        private readonly TextMeshProUGUI _question;
        private readonly TextMeshProUGUI _message;
        private readonly RectTransform _messageBack;
        private readonly RectTransform _banner;
        private readonly TextMeshProUGUI _bannerText;
        private readonly Cell[] _cells = new Cell[TriviaQuestion.AnswerCount];

        // What is on show, to redraw text only when something changes.
        private readonly BoardView _shown = new BoardView();
        private readonly string[] _shownAnswers = new string[TriviaQuestion.AnswerCount];
        private readonly int[] _shownCounts = new int[TriviaQuestion.AnswerCount];
        private bool _hasShown;
        private int _shownSeconds = -1;

        private sealed class Cell
        {
            public Image Panel;
            public Outline Frame;
            public TextMeshProUGUI Text;
            public TextMeshProUGUI Votes;
            public TextMeshProUGUI Tag;
        }

        /// <param name="root">Where to build the board; it fills this rect.</param>
        /// <param name="brightness">Colour scale for boards drawn in the lit room, where the camera's exposure brightens them.</param>
        /// <param name="onAnswer">HUD only: called with the answer (0 to 3) a player clicks.</param>
        public TriviaBoard(RectTransform root, BoardStyle style, float brightness = 1f, Action<int> onAnswer = null)
        {
            Root = root;
            _style = style;
            _brightness = brightness;
            bool hud = style == BoardStyle.Hud;
            bool podium = style == BoardStyle.Podium;

            if (!hud)
                UiKit.Panel(root, "Background", Vector2.zero, Vector2.one, Scale(Background)).GetComponent<Image>().raycastTarget = false;

            // The question: a header (question n of N, or the result), the timer, the question and a bar that shrinks with the time left.
            // On the HUD it sits below the run's hearts and floor panel.
            _questionArea = hud
                ? UiKit.Panel(root, "Question", new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.875f), HudBackground)
                : UiKit.Area(root, "Question", new Vector2(0.025f, 0.5f), new Vector2(0.975f, 0.97f));
            float headerSize = hud ? 34 : podium ? 46 : 56;
            _header = UiKit.Label(_questionArea, "Header", headerSize, TextAlignmentOptions.Left, Scale(UiKit.Gold), new Vector2(0f, 0.79f), new Vector2(0.8f, 1f), FontStyles.Bold);
            _timer = UiKit.Label(_questionArea, "Timer", headerSize * 1.25f, TextAlignmentOptions.Right, Scale(UiKit.Gold), new Vector2(0.8f, 0.76f), new Vector2(1f, 1f), FontStyles.Bold);
            RectTransform bar = UiKit.Panel(_questionArea, "TimerBar", new Vector2(0.01f, 0.73f), new Vector2(0.99f, 0.77f), Scale(new Color(1f, 1f, 1f, 0.15f)));
            _timerFill = UiKit.Panel(bar, "Fill", Vector2.zero, Vector2.one, Scale(UiKit.Gold)).GetComponent<Image>();
            _question = UiKit.Label(_questionArea, "Text", hud ? 56 : podium ? 66 : 96, TextAlignmentOptions.Center, Scale(UiKit.Ink), new Vector2(0f, 0f), new Vector2(1f, 0.73f), FontStyles.Bold);

            // Four answers in the podium buttons' layout: circle and square on top, cross and triangle below.
            _answersArea = hud
                ? UiKit.Area(root, "Answers", new Vector2(0.1f, 0.075f), new Vector2(0.9f, 0.34f))
                : UiKit.Area(root, "Answers", new Vector2(0.025f, 0.04f), new Vector2(0.975f, 0.5f));
            for (int i = 0; i < _cells.Length; i++)
                _cells[i] = BuildCell(i, hud, podium, onAnswer);

            // Title, instructions or "in position" across the middle, when no question is up. On the HUD it has a
            // backdrop, above the Ready button, so it reads over the room.
            if (hud)
            {
                _messageBack = UiKit.Panel(root, "MessageBack", new Vector2(0.17f, 0.46f), new Vector2(0.83f, 0.58f), HudBackground);
                _messageBack.GetComponent<Image>().raycastTarget = false;
                _message = UiKit.Label(_messageBack, "Message", 46, TextAlignmentOptions.Center, UiKit.Ink, Vector2.zero, Vector2.one, FontStyles.Bold);
            }
            else
            {
                _message = UiKit.Label(root, "Message", podium ? 72 : 110, TextAlignmentOptions.Center, Scale(UiKit.Ink), new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.85f), FontStyles.Bold);
            }

            // HUD: the result in a banner between the question and the answers.
            if (hud)
            {
                _banner = UiKit.Panel(root, "Result", new Vector2(0.3f, 0.41f), new Vector2(0.7f, 0.55f), HudBackground);
                _banner.GetComponent<Image>().raycastTarget = false;
                _bannerText = UiKit.Label(_banner, "Text", 64, TextAlignmentOptions.Center, UiKit.Ink, Vector2.zero, Vector2.one, FontStyles.Bold);
            }
        }

        public RectTransform Root { get; }

        public void Show(BoardView view)
        {
            bool asking = view.Active && (view.Stage == TriviaStage.Question || view.Stage == TriviaStage.Reveal) && view.Answers != null;
            bool revealing = asking && view.Stage == TriviaStage.Reveal;

            // The timer bar moves every frame; everything else changes only when the view does.
            bool counting = view.Stage == TriviaStage.Question;
            _timerFill.rectTransform.anchorMax = new Vector2(counting ? Mathf.Clamp01(view.SecondsLeft / view.SecondsTotal) : 0f, 1f);
            int seconds = counting ? Mathf.CeilToInt(view.SecondsLeft) : -1;
            if (seconds != _shownSeconds)
            {
                _shownSeconds = seconds;
                _timer.text = counting ? seconds.ToString() : string.Empty;
            }

            if (_hasShown && SameAsShown(view))
                return;
            Remember(view);

            _questionArea.gameObject.SetActive(asking);
            _answersArea.gameObject.SetActive(asking);
            if (asking)
            {
                string number = TriviaText.Get(TriviaText.QuestionOf, view.Number, view.Count);
                if (view.AskedAgain)
                    number += "  •  " + TriviaText.Get(TriviaText.AskedAgain);
                _header.text = revealing && _style != BoardStyle.Hud ? Result(view) : number;
                _header.color = Scale(revealing && _style != BoardStyle.Hud ? ResultColour(view) : UiKit.Gold);
                _question.text = view.Question;
                for (int i = 0; i < _cells.Length; i++)
                    ShowCell(i, view, revealing);
            }

            if (_banner != null)
            {
                _banner.gameObject.SetActive(revealing);
                if (revealing)
                {
                    _bannerText.text = Result(view);
                    _bannerText.color = ResultColour(view);
                }
            }

            (_messageBack != null ? _messageBack.gameObject : _message.gameObject).SetActive(!asking);
            if (!asking)
                _message.text = Message(view);
        }

        private static string Result(BoardView view)
        {
            string text = TriviaText.Outcome(view.Outcome);
            if (view.HeartsLost > 0)
                text += "  " + TriviaText.Get(TriviaText.HeartsLost, view.HeartsLost);
            return text;
        }

        private static Color ResultColour(BoardView view) =>
            view.Outcome == VoteOutcome.Correct ? Good : view.Outcome == VoteOutcome.TieAskAgain ? UiKit.Gold : Bad;

        private string Message(BoardView view)
        {
            if (view.Stage == TriviaStage.Finished && view.Number > 0)
                return TriviaText.Get(TriviaText.AllAnswered);
            if (!view.Active)
                return TriviaText.Get(TriviaText.Name) + "\n<size=55%>" + TriviaText.Get(TriviaText.Goal) + "</size>";

            switch (_style)
            {
                case BoardStyle.Podium:
                    return view.PodiumPlayer > 0
                        ? TriviaText.Get(TriviaText.PodiumPlayer, view.PodiumPlayer)
                        : TriviaText.Get(TriviaText.PodiumFree);
                case BoardStyle.Hud:
                    return view.EveryoneInPosition
                        ? TriviaText.Get(TriviaText.EveryoneReady)
                        : TriviaText.Get(TriviaText.WaitingForTeam, view.InPosition, view.Players);
                default:
                    return (view.EveryoneInPosition ? TriviaText.Get(TriviaText.EveryoneReady) : TriviaText.Get(TriviaText.GetInPosition))
                        + "\n<size=60%>" + TriviaText.Get(TriviaText.InPosition, view.InPosition, view.Players) + "</size>";
            }
        }

        private Cell BuildCell(int index, bool hud, bool podium, Action<int> onAnswer)
        {
            int column = index % 2, row = index / 2;
            var min = new Vector2(column == 0 ? 0f : 0.51f, row == 0 ? 0.52f : 0f);
            var max = new Vector2(column == 0 ? 0.49f : 1f, row == 0 ? 1f : 0.48f);
            Color colour = UiKit.AnswerColours[index];

            RectTransform rect;
            if (onAnswer != null)
            {
                int answer = index;
                Button button = UiKit.Button(_answersArea, "Answer" + (index + 1), colour, min, max, () => onAnswer(answer), out TextMeshProUGUI unused);
                UnityEngine.Object.Destroy(unused.gameObject);
                rect = (RectTransform)button.transform;
            }
            else
            {
                rect = UiKit.Panel(_answersArea, "Answer" + (index + 1), min, max, Scale(colour));
                rect.GetComponent<Image>().raycastTarget = false;
            }

            var cell = new Cell { Panel = rect.GetComponent<Image>() };
            cell.Frame = UiKit.Frame(cell.Panel, Scale(UiKit.Gold), hud ? 6f : podium ? 10f : 14f);
            cell.Frame.enabled = false;

            // The podium button's shape (and, on the HUD, its key) on a darker square at the left.
            float size = hud ? 44 : podium ? 54 : 78;
            RectTransform badge = UiKit.Panel(rect, "Shape", Vector2.zero, new Vector2(0.15f, 1f), Scale(new Color(0f, 0f, 0f, 0.28f)));
            badge.GetComponent<Image>().raycastTarget = false;
            TextMeshProUGUI shape = UiKit.Label(badge, "Symbol", size * 1.1f, TextAlignmentOptions.Center, Scale(UiKit.Ink), Vector2.zero, Vector2.one, FontStyles.Bold);
            shape.text = hud ? "<size=55%>" + (index + 1) + "</size>\n" + UiKit.AnswerShapes[index] : UiKit.AnswerShapes[index];

            cell.Text = UiKit.Label(rect, "Text", size, TextAlignmentOptions.Left, Scale(UiKit.Ink), new Vector2(0.17f, 0f), new Vector2(0.8f, 1f), FontStyles.Bold);
            cell.Votes = UiKit.Label(rect, "Votes", size * 0.6f, TextAlignmentOptions.Right, Scale(UiKit.Ink), new Vector2(0.72f, 0f), new Vector2(1f, 0.5f), FontStyles.Bold);
            cell.Tag = UiKit.Label(rect, "Tag", size * 0.5f, TextAlignmentOptions.Right, Scale(UiKit.Ink), new Vector2(0.6f, 0.5f), new Vector2(1f, 1f), FontStyles.Italic);
            return cell;
        }

        private void ShowCell(int index, BoardView view, bool revealing)
        {
            Cell cell = _cells[index];
            cell.Text.text = index < view.Answers.Count ? view.Answers[index] : string.Empty;

            bool right = revealing && index == view.Correct;
            bool chosen = index == view.Chosen;
            Color colour = UiKit.AnswerColours[index];
            if (revealing && !right)
                colour = Color.Lerp(colour, new Color(0.12f, 0.1f, 0.14f), 0.7f);
            cell.Panel.color = Scale(colour);

            cell.Frame.enabled = right || chosen;
            cell.Frame.effectColor = Scale(right ? UiKit.Gold : UiKit.Ink);

            cell.Votes.text = revealing && view.Counts != null && index < view.Counts.Count ? TriviaText.Votes(view.Counts[index]) : string.Empty;
            cell.Tag.text = chosen && _style == BoardStyle.Hud ? TriviaText.Get(TriviaText.YourAnswer) : string.Empty;
        }

        private bool SameAsShown(BoardView view)
        {
            BoardView s = _shown;
            if (s.Active != view.Active || s.Stage != view.Stage || s.Round != view.Round || s.Number != view.Number || s.Count != view.Count
                || s.AskedAgain != view.AskedAgain || s.Correct != view.Correct || s.Outcome != view.Outcome || s.HeartsLost != view.HeartsLost
                || s.Chosen != view.Chosen || s.InPosition != view.InPosition || s.Players != view.Players
                || s.EveryoneInPosition != view.EveryoneInPosition || s.PodiumPlayer != view.PodiumPlayer || s.Question != view.Question)
                return false;
            for (int i = 0; i < TriviaQuestion.AnswerCount; i++)
            {
                string answer = view.Answers != null && i < view.Answers.Count ? view.Answers[i] : null;
                int count = view.Counts != null && i < view.Counts.Count ? view.Counts[i] : 0;
                if (answer != _shownAnswers[i] || count != _shownCounts[i])
                    return false;
            }
            return true;
        }

        private void Remember(BoardView view)
        {
            BoardView s = _shown;
            s.Active = view.Active;
            s.Stage = view.Stage;
            s.Round = view.Round;
            s.Number = view.Number;
            s.Count = view.Count;
            s.AskedAgain = view.AskedAgain;
            s.Correct = view.Correct;
            s.Outcome = view.Outcome;
            s.HeartsLost = view.HeartsLost;
            s.Chosen = view.Chosen;
            s.InPosition = view.InPosition;
            s.Players = view.Players;
            s.EveryoneInPosition = view.EveryoneInPosition;
            s.PodiumPlayer = view.PodiumPlayer;
            s.Question = view.Question;
            for (int i = 0; i < TriviaQuestion.AnswerCount; i++)
            {
                _shownAnswers[i] = view.Answers != null && i < view.Answers.Count ? view.Answers[i] : null;
                _shownCounts[i] = view.Counts != null && i < view.Counts.Count ? view.Counts[i] : 0;
            }
            _hasShown = true;
        }

        private Color Scale(Color colour) =>
            _brightness >= 0.999f ? colour : new Color(colour.r * _brightness, colour.g * _brightness, colour.b * _brightness, colour.a);
    }
}
