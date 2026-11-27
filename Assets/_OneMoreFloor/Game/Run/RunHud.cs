using OneMoreFloor.Core;
using OneMoreFloor.FloorSDK;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneMoreFloor.Game
{
    /// <summary>
    /// The run's screen overlay on every player's machine: hearts and floor number, the title card,
    /// the "floor cleared" and "game over" banners, and the results with Play again.
    /// </summary>
    public sealed class RunHud : MonoBehaviour
    {
        private const string Table = "Game";

        private Canvas _canvas;
        private TextMeshProUGUI _hearts;
        private TextMeshProUGUI _floor;
        private RectTransform _titleCard;
        private TextMeshProUGUI _titleName;
        private TextMeshProUGUI _titleGoal;
        private RectTransform _banner;
        private TextMeshProUGUI _bannerText;
        private RectTransform _results;
        private TextMeshProUGUI _resultsTitle;
        private TextMeshProUGUI _resultsScore;
        private TextMeshProUGUI _playAgainLabel;
        private TextMeshProUGUI _heartPop;

        private RunManager _subscribed;
        private float _heartPopUntil;
        private int _shownHearts = -1, _shownStarting = -1, _shownFloor = -1, _shownCleared = -1;
        private RunPhase _shownPhase = (RunPhase)(-1);
        private bool _heldByHud;

        private void Awake()
        {
            _canvas = UiKit.CreateCanvas("RunHudCanvas", 10);
            _canvas.transform.SetParent(transform, false);

            RectTransform corner = UiKit.Panel(_canvas.transform, "Status", new Vector2(0.015f, 0.9f), new Vector2(0.22f, 0.98f), UiKit.PanelColour);
            _hearts = UiKit.Label(corner, "Hearts", 44, TextAlignmentOptions.Left, UiKit.Ink, new Vector2(0f, 0f), new Vector2(0.6f, 1f), FontStyles.Bold);
            _floor = UiKit.Label(corner, "Floor", 34, TextAlignmentOptions.Right, UiKit.Gold, new Vector2(0.55f, 0f), new Vector2(1f, 1f));
            // Beside the hearts, clear of floors' own panels below.
            _heartPop = UiKit.Label(_canvas.transform, "HeartPop", 56, TextAlignmentOptions.Left, new Color(1f, 0.35f, 0.35f), new Vector2(0.23f, 0.9f), new Vector2(0.45f, 0.98f), FontStyles.Bold);

            _titleCard = UiKit.Panel(_canvas.transform, "TitleCard", new Vector2(0.2f, 0.38f), new Vector2(0.8f, 0.66f), UiKit.PanelColour);
            _titleName = UiKit.Label(_titleCard, "Name", 110, TextAlignmentOptions.Center, UiKit.Gold, new Vector2(0f, 0.42f), new Vector2(1f, 1f), FontStyles.Bold);
            _titleGoal = UiKit.Label(_titleCard, "Goal", 46, TextAlignmentOptions.Center, UiKit.Ink, new Vector2(0f, 0f), new Vector2(1f, 0.42f));

            _banner = UiKit.Panel(_canvas.transform, "Banner", new Vector2(0.25f, 0.44f), new Vector2(0.75f, 0.58f), UiKit.PanelColour);
            _bannerText = UiKit.Label(_banner, "Text", 90, TextAlignmentOptions.Center, UiKit.Gold, Vector2.zero, Vector2.one, FontStyles.Bold);

            _results = UiKit.Panel(_canvas.transform, "Results", new Vector2(0.3f, 0.3f), new Vector2(0.7f, 0.7f), UiKit.PanelColour);
            _resultsTitle = UiKit.Label(_results, "Title", 90, TextAlignmentOptions.Center, UiKit.Gold, new Vector2(0f, 0.62f), new Vector2(1f, 1f), FontStyles.Bold);
            _resultsScore = UiKit.Label(_results, "Score", 56, TextAlignmentOptions.Center, UiKit.Ink, new Vector2(0f, 0.36f), new Vector2(1f, 0.62f));
            UiKit.Button(_results, "PlayAgain", new Color(0.55f, 0.1f, 0.18f), new Vector2(0.28f, 0.08f), new Vector2(0.72f, 0.3f), PlayAgain, out _playAgainLabel, 46);

            _titleCard.gameObject.SetActive(false);
            _banner.gameObject.SetActive(false);
            _results.gameObject.SetActive(false);
            _heartPop.text = string.Empty;
        }

        private void Update()
        {
            RunManager run = RunManager.Instance;
            _canvas.enabled = run != null;
            if (run == null)
                return;

            if (_subscribed != run)
            {
                if (_subscribed != null)
                    _subscribed.HeartsLost -= OnHeartsLost;
                run.HeartsLost += OnHeartsLost;
                _subscribed = run;
            }

            if (run.Hearts != _shownHearts || run.StartingHearts != _shownStarting)
            {
                _shownHearts = run.Hearts;
                _shownStarting = run.StartingHearts;
                _hearts.text = UiText.Get(Table, "hearts", run.Hearts, run.StartingHearts);
            }
            if (run.FloorNumber != _shownFloor)
            {
                _shownFloor = run.FloorNumber;
                _floor.text = UiText.Get(Table, "floor", run.FloorNumber);
            }
            if (run.Phase != _shownPhase || run.FloorsCleared != _shownCleared)
                ShowPhase(run);

            if (Time.time > _heartPopUntil)
                _heartPop.text = string.Empty;
        }

        private void ShowPhase(RunManager run)
        {
            _shownPhase = run.Phase;
            _shownCleared = run.FloorsCleared;

            FloorDefinition floor = run.CurrentFloor;
            _titleCard.gameObject.SetActive(run.Phase == RunPhase.FloorIntro && floor != null);
            if (floor != null)
            {
                _titleName.text = floor.DisplayName;
                _titleGoal.text = floor.Goal;
            }

            bool banner = run.Phase == RunPhase.FloorCleared || run.Phase == RunPhase.GameOver;
            _banner.gameObject.SetActive(banner);
            if (banner)
                _bannerText.text = UiText.Get(Table, run.Phase == RunPhase.FloorCleared ? "floor_cleared" : "game_over");

            bool results = run.Phase == RunPhase.Results;
            _results.gameObject.SetActive(results);
            if (results)
            {
                _resultsTitle.text = UiText.Get(Table, "results_title");
                _resultsScore.text = UiText.Get(Table, "results_floors", run.FloorsCleared);
                _playAgainLabel.text = UiText.Get(Table, "play_again");
            }
            HoldForResults(results);
        }

        // The results need a free mouse cursor for Play again; a player already held (e.g. at a podium) stays as is.
        private void HoldForResults(bool results)
        {
            IFloorPlayer local = FloorPlayers.Local;
            if (local == null)
                return;
            if (results && !local.IsHeld)
            {
                local.HoldAt(local.Body.position, local.Body.eulerAngles.y);
                _heldByHud = true;
            }
            else if (!results && _heldByHud)
            {
                local.Release();
                _heldByHud = false;
            }
        }

        private void PlayAgain()
        {
            if (RunManager.Instance != null)
                RunManager.Instance.ServerPlayAgain();
        }

        private void OnHeartsLost(int amount, string reason)
        {
            _heartPop.text = UiText.Get(Table, "hearts_lost", amount);
            _heartPopUntil = Time.time + 1.6f;
        }

        private void OnDestroy()
        {
            if (_subscribed != null)
                _subscribed.HeartsLost -= OnHeartsLost;
        }
    }
}
