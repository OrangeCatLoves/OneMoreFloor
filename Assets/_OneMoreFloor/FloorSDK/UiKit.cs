using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace OneMoreFloor.FloorSDK
{
    /// <summary>Small helpers to build screen UI in code, in the game's colours. Text always comes from <see cref="UiText"/>.</summary>
    public static class UiKit
    {
        public static readonly Color Ink = new Color(1f, 0.96f, 0.88f);
        public static readonly Color Gold = new Color(0.98f, 0.8f, 0.4f);
        public static readonly Color PanelColour = new Color(0.16f, 0.03f, 0.09f, 0.86f);
        public static readonly Color DimColour = new Color(0f, 0f, 0f, 0.55f);

        /// <summary>Colours and shapes of the four answers, matching the podium buttons.</summary>
        public static readonly Color[] AnswerColours =
        {
            new Color(0.85f, 0.12f, 0.16f),
            new Color(0.12f, 0.36f, 0.9f),
            new Color(0.97f, 0.72f, 0.08f),
            new Color(0.12f, 0.66f, 0.24f),
        };

        public static readonly string[] AnswerShapes = { "●", "■", "×", "▲" };

        public static Canvas CreateCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            EnsureEventSystem();
            return canvas;
        }

        /// <summary>Buttons need an EventSystem that reads the Input System.</summary>
        public static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null)
                return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            UnityEngine.Object.DontDestroyOnLoad(go);
        }

        public static RectTransform Panel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color colour)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.color = colour;
            image.raycastTarget = colour.a > 0f;
            return rect;
        }

        public static RectTransform Area(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        public static TextMeshProUGUI Label(Transform parent, string name, float size, TextAlignmentOptions alignment, Color colour,
            Vector2 anchorMin, Vector2 anchorMax, FontStyles style = FontStyles.Normal)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = new Vector2(12, 6);
            rect.offsetMax = new Vector2(-12, -6);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.fontSize = size;
            text.enableAutoSizing = true;
            text.fontSizeMin = size * 0.5f;
            text.fontSizeMax = size;
            text.alignment = alignment;
            text.color = colour;
            text.fontStyle = style;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false;
            return text;
        }

        public static Button Button(Transform parent, string name, Color colour, Vector2 anchorMin, Vector2 anchorMax, Action onClick, out TextMeshProUGUI label, float textSize = 40f)
        {
            RectTransform rect = Panel(parent, name, anchorMin, anchorMax, colour);
            var button = rect.gameObject.AddComponent<Button>();
            var colours = button.colors;
            colours.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
            colours.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            button.colors = colours;
            button.onClick.AddListener(() => onClick());
            label = Label(rect, "Label", textSize, TextAlignmentOptions.Center, Ink, Vector2.zero, Vector2.one, FontStyles.Bold);
            return button;
        }

        /// <summary>A thin frame drawn round a rect (used to mark a chosen or right answer).</summary>
        public static Outline Frame(Graphic graphic, Color colour, float width)
        {
            // Not GetComponent() ?? AddComponent(): in the Editor a missing component comes back as a fake null that ?? doesn't see.
            if (!graphic.TryGetComponent(out Outline outline))
                outline = graphic.gameObject.AddComponent<Outline>();
            outline.effectColor = colour;
            outline.effectDistance = new Vector2(width, -width);
            return outline;
        }
    }
}
