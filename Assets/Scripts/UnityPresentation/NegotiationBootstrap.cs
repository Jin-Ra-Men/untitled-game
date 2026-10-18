#if UNITY_5_3_OR_NEWER
using UntitledGame.Negotiation;
using UntitledGame.Negotiation.Episodes;
using UnityEngine;
using UnityEngine.UI;

namespace UntitledGame.UnityPresentation
{
    /// <summary>
    /// 빈 씬에서도 가족관계증명서 교섭 UI를 런타임 생성한다.
    /// Unity 에디터에서 이 컴포넌트를 아무 GameObject에 붙이거나, 자동 부트를 사용한다.
    /// </summary>
    public sealed class NegotiationBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBoot()
        {
            if (FindObjectOfType<NegotiationBootstrap>() != null) return;
            var go = new GameObject("NegotiationBootstrap");
            go.AddComponent<NegotiationBootstrap>();
        }

        private FamilyCertificateEpisode _episode = null!;
        private Text _speaker = null!;
        private Text _body = null!;
        private Text _status = null!;
        private Transform _buttonRoot = null!;
        private float _interruptPulse;

        private void Start()
        {
            BuildUi();
            _episode = new FamilyCertificateEpisode();
            _episode.Start();
            Refresh();
        }

        private void Update()
        {
            if (_episode == null || _episode.CurrentFrame.IsOutcome) return;
            _episode.TickRealtime(Time.deltaTime);
            _interruptPulse += Time.deltaTime;
            if (_interruptPulse > 0.25f)
            {
                _interruptPulse = 0f;
                RefreshStatusOnly();
            }
        }

        private void Refresh()
        {
            var frame = _episode.CurrentFrame;
            _speaker.text = frame.Speaker;
            _body.text = frame.Line;
            RefreshStatusOnly();

            foreach (Transform child in _buttonRoot)
                Destroy(child.gameObject);

            for (var i = 0; i < frame.Choices.Length; i++)
            {
                var choice = frame.Choices[i];
                CreateButton(choice, i);
            }

            if (frame.AllowSilence)
                CreateActionButton("침묵", () =>
                {
                    _episode.SubmitSilence();
                    Refresh();
                });

            if (frame.AllowInterrupt)
                CreateActionButton("끼어들기", () =>
                {
                    _episode.SubmitInterrupt();
                    Refresh();
                });
        }

        private void RefreshStatusOnly()
        {
            var frame = _episode.CurrentFrame;
            var extra = "";
            if (frame.AllowInterrupt)
                extra = " · 끼어들기 가능";
            _status.text = frame.StatusLine + extra;
        }

        private void CreateButton(ChoiceOption choice, int index)
        {
            var label = $"[{index + 1}] {choice.Label}";
            if (!string.IsNullOrEmpty(choice.Hint))
                label += $"\n({choice.Hint})";

            CreateActionButton(label, () =>
            {
                if (choice.Id == "quit")
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    return;
                }

                _episode.SubmitChoice(choice.Id);
                Refresh();
            });
        }

        private void CreateActionButton(string label, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Btn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(_buttonRoot, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.18f, 0.95f);
            var le = go.GetComponent<LayoutElement>();
            le.minHeight = 48f;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var t = textGo.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (t.font == null)
                t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.text = label;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleLeft;
            t.fontSize = 16;
            Stretch(textGo.GetComponent<RectTransform>(), 12f, 8f);

            go.GetComponent<Button>().onClick.AddListener(onClick);
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }

            var panel = CreatePanel(canvasGo.transform);
            _speaker = CreateText(panel, "Speaker", 28, FontStyle.Bold, new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.96f));
            _body = CreateText(panel, "Body", 20, FontStyle.Normal, new Vector2(0.05f, 0.42f), new Vector2(0.95f, 0.86f));
            _status = CreateText(panel, "Status", 16, FontStyle.Italic, new Vector2(0.05f, 0.36f), new Vector2(0.95f, 0.42f));
            _status.color = new Color(0.75f, 0.75f, 0.8f);

            var buttonPanel = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            buttonPanel.transform.SetParent(panel, false);
            var rt = buttonPanel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 0.04f);
            rt.anchorMax = new Vector2(0.95f, 0.34f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var v = buttonPanel.GetComponent<VerticalLayoutGroup>();
            v.spacing = 8f;
            v.childForceExpandHeight = false;
            v.childForceExpandWidth = true;
            v.childControlHeight = true;
            v.childControlWidth = true;
            buttonPanel.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            _buttonRoot = buttonPanel.transform;
        }

        private static RectTransform CreatePanel(Transform parent)
        {
            var go = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.08f, 0.09f, 0.12f, 1f);
            Stretch(go.GetComponent<RectTransform>(), 0f, 0f);
            return go.GetComponent<RectTransform>();
        }

        private static Text CreateText(Transform parent, string name, int size, FontStyle style, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (t.font == null)
                t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = size;
            t.fontStyle = style;
            t.color = Color.white;
            t.alignment = TextAnchor.UpperLeft;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private static void Stretch(RectTransform rt, float padX, float padY)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padX, padY);
            rt.offsetMax = new Vector2(-padX, -padY);
        }
    }
}
#endif
