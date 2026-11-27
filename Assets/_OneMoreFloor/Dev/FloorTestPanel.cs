using OneMoreFloor.Core;
using OneMoreFloor.FloorSDK;
using OneMoreFloor.Game;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OneMoreFloor.Dev
{
    /// <summary>
    /// The floor test's controls, on the host's screen: restart the floor, change the difficulty level,
    /// clear the floor, lose a heart, and switch unlimited hearts. Keys F5-F9 do the same without the mouse,
    /// and F1 hides or shows the panel.
    /// </summary>
    public sealed class FloorTestPanel : MonoBehaviour
    {
        private const string Table = "Dev";
        private const int HighestLevel = 20;

        private TextMeshProUGUI _title;
        private TextMeshProUGUI _heartsLabel;
        private RectTransform _panel;
        private InputAction _restart;
        private InputAction _levelDown;
        private InputAction _levelUp;
        private InputAction _clear;
        private InputAction _loseHeart;
        private InputAction _toggle;
        private bool _hidden;
        private FloorDefinition _shownFloor;
        private int _shownLevel = -1;
        private bool? _shownUnlimited;

        public static void Create()
        {
            if (FindAnyObjectByType<FloorTestPanel>() != null)
                return;
            var go = new GameObject("FloorTestPanel");
            DontDestroyOnLoad(go);
            go.AddComponent<FloorTestPanel>();
        }

        private void Awake()
        {
            Canvas canvas = UiKit.CreateCanvas("FloorTestCanvas", 20);
            canvas.transform.SetParent(transform, false);

            _panel = UiKit.Panel(canvas.transform, "Panel", new Vector2(0.78f, 0.56f), new Vector2(0.995f, 0.985f), new Color(0.02f, 0.06f, 0.1f, 0.82f));
            _title = UiKit.Label(_panel, "Title", 26, TextAlignmentOptions.Center, new Color(0.55f, 0.9f, 1f), new Vector2(0f, 0.86f), new Vector2(1f, 1f), FontStyles.Bold);

            AddButton(0, "restart", Restart);
            AddButton(1, "level_down", () => ChangeLevel(-1));
            AddButton(2, "level_up", () => ChangeLevel(+1));
            AddButton(3, "clear", ClearFloor);
            AddButton(4, "lose_heart", LoseHeart);
            (Vector2 heartsMin, Vector2 heartsMax) = Row(5);
            UiKit.Button(_panel, "Hearts", new Color(0.25f, 0.3f, 0.4f), heartsMin, heartsMax, ToggleHearts, out _heartsLabel, 22);
            TextMeshProUGUI hint = UiKit.Label(_panel, "Hint", 16, TextAlignmentOptions.Center, UiKit.Ink, new Vector2(0f, 0f), new Vector2(1f, 0.07f), FontStyles.Italic);
            hint.text = UiText.Get(Table, "mouse_hint");

            _restart = Key("<Keyboard>/f5");
            _levelDown = Key("<Keyboard>/f6");
            _levelUp = Key("<Keyboard>/f7");
            _clear = Key("<Keyboard>/f8");
            _loseHeart = Key("<Keyboard>/f9");
            _toggle = Key("<Keyboard>/f1");
        }

        private void OnDestroy()
        {
            foreach (InputAction action in new[] { _restart, _levelDown, _levelUp, _clear, _loseHeart, _toggle })
                action?.Dispose();
        }

        private void Update()
        {
            RunManager run = RunManager.Instance;
            FloorTestRequest test = FloorTest.Current;
            // Only the host runs the floor, so only the host gets the controls.
            bool host = run != null && run.IsServerInitialized && test != null;
            if (!host)
            {
                _panel.gameObject.SetActive(false);
                return;
            }

            if (_toggle.WasPressedThisFrame())
                _hidden = !_hidden;
            _panel.gameObject.SetActive(!_hidden);

            // Text is looked up only when it changes, not every frame.
            FloorDefinition floor = run.CurrentFloor;
            if (floor != _shownFloor || test.level != _shownLevel)
            {
                _shownFloor = floor;
                _shownLevel = test.level;
                _title.text = UiText.Get(Table, "title") + "\n<size=80%>"
                    + UiText.Get(Table, "floor_level", floor != null ? floor.DisplayName : test.floorId, test.level) + "</size>";
            }
            if (_shownUnlimited != test.unlimitedHearts)
            {
                _shownUnlimited = test.unlimitedHearts;
                _heartsLabel.text = UiText.Get(Table, test.unlimitedHearts ? "hearts_unlimited" : "hearts_normal");
            }

            if (_restart.WasPressedThisFrame())
                Restart();
            if (_levelDown.WasPressedThisFrame())
                ChangeLevel(-1);
            if (_levelUp.WasPressedThisFrame())
                ChangeLevel(+1);
            if (_clear.WasPressedThisFrame())
                ClearFloor();
            if (_loseHeart.WasPressedThisFrame())
                LoseHeart();
        }

        private static void Restart()
        {
            if (RunManager.Instance != null)
                RunManager.Instance.StartRun();
        }

        private static void ChangeLevel(int change)
        {
            FloorTestRequest test = FloorTest.Current;
            if (test == null)
                return;
            test.level = Mathf.Clamp(test.level + change, 1, HighestLevel);
            FloorTestBoot.Save(test);
            Restart();
        }

        private static void ClearFloor()
        {
            RunManager run = RunManager.Instance;
            if (run != null && run.Phase == RunPhase.FloorPlay)
                run.Cleared();
        }

        private static void LoseHeart()
        {
            if (RunManager.Instance != null)
                RunManager.Instance.LoseHearts(1, "dev.floor_test");
        }

        private static void ToggleHearts()
        {
            FloorTestRequest test = FloorTest.Current;
            if (test == null)
                return;
            test.unlimitedHearts = !test.unlimitedHearts;
            FloorTestBoot.Save(test);
        }

        private void AddButton(int row, string key, System.Action onClick)
        {
            (Vector2 min, Vector2 max) = Row(row);
            UiKit.Button(_panel, key, new Color(0.1f, 0.35f, 0.5f), min, max, onClick, out TextMeshProUGUI label, 22);
            label.text = UiText.Get(Table, key);
        }

        private static (Vector2 min, Vector2 max) Row(int row)
        {
            float top = 0.84f - row * 0.128f;
            return (new Vector2(0.06f, top - 0.11f), new Vector2(0.94f, top));
        }

        private static InputAction Key(string binding)
        {
            var action = new InputAction(type: InputActionType.Button, binding: binding);
            action.Enable();
            return action;
        }
    }
}
