using System;
using OneMoreFloor.FloorSDK;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OneMoreFloor.Floors.Trivia
{
    /// <summary>
    /// Trivia on each player's machine: E steps into or out of a podium, 1-4 or a click votes, and Ready
    /// starts the questions. It shows the state on the player's screen (only while in position) and on the
    /// room's big screen, the players' TV and the four podium screens, and moves the camera to the podium view.
    /// </summary>
    public sealed class TriviaPresenter : MonoBehaviour
    {
        // The room's camera exposure brightens anything drawn in it, so boards in the room use dimmer colours.
        private const float RoomBoardBrightness = 0.6f;

        // Canvas resolution across each kind of screen, in canvas pixels.
        private const float ScreenPixelsWide = 1776f;
        private const float PodiumPixelsWide = 1000f;

        // Fixed podium view: behind and above the player's right shoulder, looking over the podium towards the TV.
        private const float CameraBack = 1.7f;
        private const float CameraUp = 2.35f;
        private const float CameraRight = 0.55f;
        private const float LookTowardsTv = 0.6f;
        private const float CameraFieldOfView = 55f;
        private const float CameraBlendSeconds = 0.6f;

        private TriviaFloor _floor;
        private TriviaRoom _room;

        private GameObject _worldRoot;
        private TriviaBoard _bigScreen;
        private TriviaBoard _tv;
        private readonly TriviaBoard[] _podiumBoards = new TriviaBoard[TriviaFloor.PodiumCount];
        private readonly BoardView _screenView = new BoardView();
        private readonly BoardView[] _podiumViews = new BoardView[TriviaFloor.PodiumCount];

        private Canvas _hud;
        private RectTransform _hudBoardRoot;
        private TriviaBoard _hudBoard;
        private readonly BoardView _hudView = new BoardView();
        private Button _ready;
        private TextMeshProUGUI _readyLabel;
        private TextMeshProUGUI _prompt;
        private RectTransform _promptBack;
        private TextMeshProUGUI _hint;

        private InputAction _interact;
        private readonly InputAction[] _choices = new InputAction[TriviaQuestion.AnswerCount];

        private CinemachineCamera _podiumCamera;
        private int _heldAt = -1;
        private int _myVote = TriviaFloor.NoAnswer;
        private int _voteRound = -1;
        private string _shownPromptKey = string.Empty;
        private string _shownHintKey = string.Empty;

        public void Show(TriviaFloor floor)
        {
            _floor = floor;
            for (int i = 0; i < _podiumViews.Length; i++)
                _podiumViews[i] = new BoardView();

            try
            {
                _room = TriviaRoom.Find();
                BuildRoomBoards();
                BuildHud();
            }
            catch (Exception exception)
            {
                // Without its room or boards Trivia can't be shown: say why once, rather than failing every frame.
                Debug.LogException(exception);
                enabled = false;
            }
        }

        private void Update()
        {
            if (_floor == null)
                return;

            IFloorPlayer local = FloorPlayers.Local;
            if (local != null && _interact == null)
                BindInput(local);

            int myPodium = local != null ? _floor.PodiumOf(local.ClientId) : -1;
            if (_floor.Round != _voteRound || myPodium < 0)
            {
                // A new question (or asked again), or out of position: no vote yet.
                _voteRound = _floor.Round;
                _myVote = TriviaFloor.NoAnswer;
            }

            HoldAtPodium(local, myPodium);
            ReadInput(local, myPodium);
            ShowBoards(myPodium);
            ShowHud(local, myPodium);
        }

        private void OnDestroy()
        {
            if (_heldAt >= 0)
                FloorPlayers.Local?.Release();
            if (_podiumCamera != null)
                Destroy(_podiumCamera.gameObject);
            if (_hud != null)
                Destroy(_hud.gameObject);
            if (_worldRoot != null)
                Destroy(_worldRoot);
        }

        // ----- Podium: position and camera -----

        private void HoldAtPodium(IFloorPlayer local, int podium)
        {
            if (local == null || podium == _heldAt)
                return;

            if (podium >= 0)
            {
                local.HoldAt(_room.Stands[podium] + Vector3.up * 0.02f, _room.StandYaw[podium]);
                ShowPodiumCamera(podium);
            }
            else
            {
                local.Release();
                if (_podiumCamera != null)
                    _podiumCamera.gameObject.SetActive(false);
            }
            _heldAt = podium;
        }

        private void ShowPodiumCamera(int podium)
        {
            if (_podiumCamera == null)
            {
                CinemachineCamera playerCamera = FindAnyObjectByType<CinemachineCamera>();
                var go = new GameObject("TriviaPodiumCamera");
                go.SetActive(false);
                _podiumCamera = go.AddComponent<CinemachineCamera>();
                if (playerCamera != null)
                    _podiumCamera.Lens = playerCamera.Lens;
                LensSettings lens = _podiumCamera.Lens;
                lens.FieldOfView = CameraFieldOfView;
                _podiumCamera.Lens = lens;

                Camera main = Camera.main;
                if (main != null && main.TryGetComponent(out CinemachineBrain brain))
                    brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, CameraBlendSeconds);
            }

            Quaternion facing = Quaternion.Euler(0f, _room.StandYaw[podium], 0f);
            Vector3 position = _room.Stands[podium] - facing * Vector3.forward * CameraBack + Vector3.up * CameraUp + facing * Vector3.right * CameraRight;
            Vector3 target = Vector3.Lerp(_room.PodiumScreens[podium].Centre, _room.PlayerScreen.Centre, LookTowardsTv);
            _podiumCamera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(target - position, Vector3.up));
            _podiumCamera.gameObject.SetActive(true);
        }

        // ----- Input -----

        private void BindInput(IFloorPlayer local)
        {
            InputActionAsset controls = local.Controls;
            if (controls == null)
                return;
            _interact = controls.FindAction("Player/Interact", throwIfNotFound: true);
            for (int i = 0; i < _choices.Length; i++)
                _choices[i] = controls.FindAction("Player/Choice" + (i + 1), throwIfNotFound: true);
            _interact.Enable();
            foreach (InputAction choice in _choices)
                choice.Enable();
        }

        private void ReadInput(IFloorPlayer local, int myPodium)
        {
            if (local == null || _interact == null)
                return;

            if (_interact.WasPressedThisFrame())
            {
                if (myPodium >= 0)
                {
                    _floor.RequestLeavePodium();
                }
                else if (_floor.IsActive)
                {
                    int near = _room.PodiumWithin(local.Body.position, _floor.Settings.PodiumReach);
                    if (near >= 0 && _floor.Podiums[near] == TriviaFloor.Nobody)
                        _floor.RequestEnterPodium(near);
                }
            }

            for (int i = 0; i < _choices.Length; i++)
                if (_choices[i].WasPressedThisFrame())
                    Vote(i);
        }

        private void Vote(int answer)
        {
            IFloorPlayer local = FloorPlayers.Local;
            if (local == null || _floor.Stage != TriviaStage.Question || _floor.PodiumOf(local.ClientId) < 0)
                return;
            _myVote = answer;
            _floor.RequestVote(answer);
        }

        // ----- Boards -----

        private void BuildRoomBoards()
        {
            _worldRoot = new GameObject("TriviaScreens");
            _bigScreen = new TriviaBoard(WorldCanvas("BigScreen", _room.QuestionScreen, ScreenPixelsWide, 0.02f), BoardStyle.Screen, RoomBoardBrightness);
            _tv = new TriviaBoard(WorldCanvas("PlayersTV", _room.PlayerScreen, ScreenPixelsWide, 0.01f), BoardStyle.Screen, RoomBoardBrightness);
            for (int i = 0; i < _podiumBoards.Length; i++)
                _podiumBoards[i] = new TriviaBoard(WorldCanvas("PodiumScreen" + (i + 1), _room.PodiumScreens[i], PodiumPixelsWide, 0.01f), BoardStyle.Podium, RoomBoardBrightness);
        }

        private RectTransform WorldCanvas(string name, ScreenSurface surface, float pixelsWide, float inset)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas));
            go.transform.SetParent(_worldRoot.transform, false);
            go.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;

            float width = surface.Width * (1f - 2f * inset);
            float height = surface.Height - surface.Width * 2f * inset;
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(pixelsWide, pixelsWide * height / width);
            rect.SetPositionAndRotation(surface.Centre + surface.Facing * 0.012f, surface.CanvasRotation);
            rect.localScale = Vector3.one * (width / pixelsWide);
            return rect;
        }

        private void ShowBoards(int myPodium)
        {
            Fill(_screenView);
            _bigScreen.Show(_screenView);
            _tv.Show(_screenView);

            for (int i = 0; i < _podiumBoards.Length; i++)
            {
                BoardView view = _podiumViews[i];
                Fill(view);
                int occupant = _floor.Podiums.Count > i ? _floor.Podiums[i] : TriviaFloor.Nobody;
                view.PodiumPlayer = occupant != TriviaFloor.Nobody ? occupant + 1 : 0;
                // Votes stay hidden until time runs out; then each podium shows its player's answer.
                view.Chosen = _floor.Stage == TriviaStage.Reveal && _floor.PodiumVotes.Count > i ? _floor.PodiumVotes[i] : TriviaFloor.NoAnswer;
                _podiumBoards[i].Show(view);
            }

            if (_hudBoard != null && myPodium >= 0)
            {
                Fill(_hudView);
                _hudView.Chosen = _myVote;
                _hudBoard.Show(_hudView);
            }
        }

        private void Fill(BoardView view)
        {
            view.Active = _floor.IsActive;
            view.Stage = _floor.Stage;
            view.Round = _floor.Round;
            view.Number = _floor.QuestionNumber;
            view.Count = _floor.QuestionCount;
            view.AskedAgain = _floor.AskedAgain;
            view.Question = _floor.Question;
            view.Answers = _floor.Answers;
            view.SecondsLeft = _floor.SecondsLeft;
            view.SecondsTotal = _floor.Settings.SecondsPerQuestion;
            view.Correct = _floor.Correct;
            view.Counts = _floor.Counts;
            view.Outcome = _floor.Outcome;
            view.HeartsLost = _floor.HeartsLost;
            view.Chosen = TriviaFloor.NoAnswer;
            view.InPosition = _floor.InPosition;
            view.Players = _floor.Players;
            view.EveryoneInPosition = _floor.EveryoneInPosition;
        }

        // ----- The player's own screen -----

        private void BuildHud()
        {
            _hud = UiKit.CreateCanvas("TriviaHud", 5);
            _hudBoardRoot = UiKit.Area(_hud.transform, "Board", Vector2.zero, Vector2.one);
            _hudBoard = new TriviaBoard(_hudBoardRoot, BoardStyle.Hud, 1f, Vote);
            _ready = UiKit.Button(_hud.transform, "Ready", new Color(0.12f, 0.6f, 0.25f), new Vector2(0.4f, 0.3f), new Vector2(0.6f, 0.42f), () => _floor.RequestStart(), out _readyLabel, 60);
            _promptBack = UiKit.Panel(_hud.transform, "Prompt", new Vector2(0.3f, 0.18f), new Vector2(0.7f, 0.26f), new Color(0.1f, 0.03f, 0.12f, 0.75f));
            _promptBack.GetComponent<Image>().raycastTarget = false;
            _prompt = UiKit.Label(_promptBack, "Text", 44, TextAlignmentOptions.Center, UiKit.Ink, Vector2.zero, Vector2.one, FontStyles.Bold);
            _hint = UiKit.Label(_hud.transform, "Hint", 30, TextAlignmentOptions.Center, UiKit.Ink, new Vector2(0.1f, 0.012f), new Vector2(0.9f, 0.06f));
            _promptBack.gameObject.SetActive(false);
            _hudBoardRoot.gameObject.SetActive(false);
            _ready.gameObject.SetActive(false);
        }

        private void ShowHud(IFloorPlayer local, int myPodium)
        {
            if (_hud == null)
                return;

            bool inPosition = myPodium >= 0 && _floor.IsActive;
            _hudBoardRoot.gameObject.SetActive(inPosition);

            bool ready = inPosition && _floor.Stage == TriviaStage.Waiting && _floor.EveryoneInPosition;
            if (ready != _ready.gameObject.activeSelf)
            {
                _ready.gameObject.SetActive(ready);
                _readyLabel.text = TriviaText.Get(TriviaText.Ready);
            }

            // Text is looked up only when the prompt or hint changes, not every frame.
            string promptKey = string.Empty;
            if (!inPosition && local != null && _floor.IsActive && _floor.Stage != TriviaStage.Finished)
            {
                int near = _room.PodiumWithin(local.Body.position, _floor.Settings.PodiumReach);
                if (near >= 0)
                    promptKey = _floor.Podiums[near] == TriviaFloor.Nobody ? TriviaText.PressE : TriviaText.PodiumTaken;
            }
            if (promptKey != _shownPromptKey)
            {
                _shownPromptKey = promptKey;
                _prompt.text = promptKey.Length > 0 ? TriviaText.Get(promptKey) : string.Empty;
                _promptBack.gameObject.SetActive(promptKey.Length > 0);
            }

            string hintKey = !inPosition ? string.Empty : _floor.Stage == TriviaStage.Question ? TriviaText.Choose : TriviaText.StepOut;
            if (hintKey != _shownHintKey)
            {
                _shownHintKey = hintKey;
                _hint.text = hintKey == TriviaText.Choose ? TriviaText.Get(TriviaText.Choose) + "     " + TriviaText.Get(TriviaText.StepOut)
                    : hintKey.Length > 0 ? TriviaText.Get(hintKey) : string.Empty;
            }
        }
    }
}
