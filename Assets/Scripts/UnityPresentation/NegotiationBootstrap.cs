#if UNITY_5_3_OR_NEWER
using System.Collections.Generic;
using UntitledGame.Negotiation;
using UntitledGame.Negotiation.Episodes;
using UntitledGame.Negotiation.Localization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UntitledGame.UnityPresentation
{
    /// <summary>
    /// Split-screen negotiation UI inspired by Juusei presentation:
    /// left intern / right partner, typewriter lines (no dialogue box),
    /// numbered choices bottom-center without meta-hints.
    /// The speaker's region widens and the listener's narrows along a jagged yellow seam.
    /// Chat lines appear as bubbles in a company-messenger window over the split.
    /// </summary>
    public sealed class NegotiationBootstrap : MonoBehaviour
    {
        private const float UrgencyStart = 0.55f;
        private const float SplitEqual = 0.50f;
        private const float SplitLeftSpeaks = 0.64f;
        private const float SplitRightSpeaks = 0.36f;
        private const float SplitSlant = 0.22f;
        private const float SplitLerp = 4.5f;
        private const float TextAnchorY = 0.45f;
        private const float CharsPerSecond = 14f;
        private const float SpeakHeight = 0.98f;
        private const float QuietHeight = 0.88f;
        private const float PortraitLerp = 5f;
        // Pacing: let a finished line sit, and let the split slide before the next speaker starts typing.
        private const float HoldBase = 1.0f;
        private const float HoldPerChar = 0.035f;
        private const float HoldMax = 3.5f;
        private const float ChoiceRevealDelay = 0.5f;
        private const float SpeakerSwitchDelay = 0.5f;
        private const float SameSpeakerDelay = 0.2f;
        // Messenger window, in canvas reference units (1920x1080, width-matched).
        private const float MessengerWidth = 700f;
        private const float MessengerHeight = 660f;
        private const float MessengerTop = 36f;
        private const float ChatHeaderHeight = 72f;
        private const float ChatInputHeight = 70f;
        private const float BubbleMaxTextWidth = 400f;
        private const float BubblePadX = 16f;
        private const float BubblePadY = 10f;
        private const float BubbleSpacing = 10f;
        private const float ChatAvatarSize = 42f;
        private const float ChatCharsPerSecond = 20f;
        private const float MinPartnerTyping = 0.9f;
        private const float ChatBlendLerp = 6f;
        private const float ChatPortraitLeft = 0.15f;
        private const float ChatPortraitRight = 0.85f;
        // Desk poses are close-ups; shrink them so heads clear the screen edges and the side text.
        private const float ChatPortraitScale = 0.7f;
        private const int EmotionSegments = EmotionBar.Segments;
        private static readonly Color PlateSpeaking = new Color(0.05f, 0.06f, 0.10f, 0.62f);
        private static readonly Color PlateListening = new Color(0.05f, 0.06f, 0.10f, 0.25f);
        private static readonly Color EmotionFill = new Color(0.85f, 0.18f, 0.16f, 1f);
        private static readonly Color EmotionEmpty = new Color(0.22f, 0.18f, 0.2f, 0.85f);
        private static readonly Color InterruptGreen = new Color(0.25f, 0.85f, 0.35f, 1f);

        private static readonly Color LeftAccent = new Color(0.45f, 0.78f, 1f, 1f);
        private static readonly Color RightAccent = new Color(1f, 0.58f, 0.28f, 1f);
        private static readonly Color SeamYellow = new Color(1f, 0.9f, 0.2f, 1f);
        private static readonly Color SeamGlow = new Color(1f, 0.85f, 0.2f, 0.28f);
        private static readonly Color SeamDark = new Color(0.03f, 0.03f, 0.05f, 0.9f);
        private static readonly Color ListenerTint = new Color(0.62f, 0.62f, 0.68f, 1f);
        private static readonly Color ChoiceText = new Color(0.12f, 0.12f, 0.17f, 1f);
        private static readonly Color ChoiceUrgent = new Color(0.85f, 0.12f, 0.1f, 1f);
        private static readonly Color ChatText = new Color(0.13f, 0.14f, 0.18f, 1f);
        private static readonly Color MineBubble = new Color(0.80f, 0.82f, 0.98f, 1f);
        private static readonly Color TheirBubble = Color.white;
        private static readonly Color UnreadColor = new Color(0.98f, 0.72f, 0.1f, 1f);

        private sealed class ChatRow
        {
            public RectTransform Rt = null!;
            public float Height;
            public Text Unread = null!;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBoot()
        {
            if (Object.FindAnyObjectByType<NegotiationBootstrap>() != null) return;
            var go = new GameObject("NegotiationBootstrap");
            go.AddComponent<NegotiationBootstrap>();
        }

        private FamilyCertificateEpisode _episode = null!;

        private RectTransform _stage = null!;
        private SplitRegionGraphic _leftRegion = null!;
        private SplitRegionGraphic _rightRegion = null!;
        private SplitSeamGraphic[] _seams = null!;
        private RawImage _leftPortrait = null!;
        private RawImage _rightPortrait = null!;
        private RawImage _leftSceneBg = null!;
        private RawImage _rightSceneBg = null!;
        private Image _leftDim = null!;
        private Image _rightDim = null!;
        private RectTransform _leftTextGroup = null!;
        private RectTransform _rightTextGroup = null!;
        private Image _leftPlate = null!;
        private Image _rightPlate = null!;
        private string _partnerBackgroundId = "cafe";
        private Text _leftName = null!;
        private Text _leftLine = null!;
        private Text _rightName = null!;
        private Text _rightLine = null!;
        private Image _leftUnderline = null!;
        private Image _rightUnderline = null!;
        private Image _urgencyVeil = null!;
        private Transform _buttonRoot = null!;
        private RectTransform _buttonRootRt = null!;
        private GameObject _choiceCluster = null!;
        private readonly List<Button> _choiceButtons = new List<Button>();
        private GameObject _emotionRoot = null!;
        private readonly Image[] _emotionCells = new Image[EmotionSegments];
        private GameObject _interruptRoot = null!;
        private Text _interruptLabel = null!;
        private Text _interruptPositive = null!;
        private Button _interruptButton = null!;

        private CanvasGroup _messenger = null!;
        private RectTransform _messengerRt = null!;
        private RectTransform _chatBody = null!;
        private Text _chatInput = null!;
        private RectTransform _typingRow = null!;
        private Text _typingText = null!;
        private readonly List<ChatRow> _chatRows = new List<ChatRow>();
        private int _chatSession = -1;
        private bool _chatLine;
        private bool _partnerTyping;
        private float _chatTarget;
        private float _chatBlend;
        private RectTransform _popRow = null!;
        private float _popT = 1f;

        private string _partnerPortraitId = "minjae";
        private bool _leftIsSpeaking = true;
        private bool _isOutcome;
        private float _split = SplitEqual;
        private float _leftHeight = QuietHeight;
        private float _rightHeight = QuietHeight;
        private float _leftListen;
        private float _rightListen;

        private string _typeFull = "";
        private int _typeIndex;
        private float _typeCarry;
        private bool _typing;
        private bool _choicesReady;
        private float _typeDelay;
        private bool _holding;
        private float _holdTimer;
        private int _inputConsumedFrame = -1;

        private void Start()
        {
            UnityLocLoader.EnsureLoaded();
            BuildUi();
            _episode = new FamilyCertificateEpisode();
            _episode.Start();
            BeginFrame(forceRebuildChoices: true);
            SnapLayout();
        }

        private void Update()
        {
            if (_episode == null) return;

            // Click / Space: finish typing, or end the post-line pause. The click that picked a choice doesn't count.
            if (Time.frameCount != _inputConsumedFrame && WasSkipPressed())
            {
                if (_typing) FinishTypewriter();
                else if (_holding) EndHold();
            }
            else if (_holding)
            {
                _holdTimer -= Time.unscaledDeltaTime;
                if (_holdTimer <= 0f) EndHold();
            }
            else if (_choicesReady)
            {
                if (WasInterruptPressed() && _episode.CurrentFrame.AllowInterrupt)
                    FireInterrupt();
                var picked = PressedChoiceIndex();
                if (picked >= 0 && picked < _choiceButtons.Count)
                    _choiceButtons[picked].onClick.Invoke();
            }

            var beforeLine = _episode.CurrentFrame.Line;
            var beforePhase = _episode.CurrentFrame.Phase;
            var beforeFinished = _episode.IsFinished;

            // Silence timer only after the line has finished typing
            if (_choicesReady && !_episode.CurrentFrame.IsOutcome)
                _episode.TickRealtime(Time.deltaTime);

            var frame = _episode.CurrentFrame;
            if (frame.Line != beforeLine || frame.Phase != beforePhase || frame.IsOutcome != beforeFinished)
                BeginFrame(forceRebuildChoices: true);

            if (_typing)
                TickTypewriter(Time.unscaledDeltaTime);

            AnimateLayout(Time.unscaledDeltaTime);
            AnimateChat(Time.unscaledDeltaTime);
            UpdateTension(frame);
            UpdateEmotionBar();
            UpdateInterruptHud(frame);
        }

        private void BeginFrame(bool forceRebuildChoices)
        {
            var frame = _episode.CurrentFrame;
            var wasLeft = _leftIsSpeaking;
            ApplySpeakerLayout(frame);
            ApplyPortraits(frame);
            ApplyBackgrounds(frame);

            if (frame.ChatSession != _chatSession)
            {
                ClearChat();
                _chatSession = frame.ChatSession;
            }
            if (frame.ChatSeen) MarkChatSeen();
            _chatTarget = frame.MessengerOpen ? 1f : 0f;
            _chatLine = frame.IsChat && frame.MessengerOpen;
            ShowPartnerTyping(false);

            _typeFull = frame.Line ?? "";
            _typeIndex = 0;
            _typeCarry = 0f;
            _typeDelay = wasLeft != _leftIsSpeaking ? SpeakerSwitchDelay : SameSpeakerDelay;
            _holding = false;
            _typing = _typeFull.Length > 0;
            _choicesReady = false;

            if (_leftIsSpeaking)
                _leftLine.text = "";
            else
                _rightLine.text = "";

            if (forceRebuildChoices)
                RebuildChoices(visible: _choicesReady);

            if (!_typing)
                FinishTypewriter();
        }

        private void TickTypewriter(float dt)
        {
            if (_typeDelay > 0f)
            {
                _typeDelay -= dt;
                return;
            }

            if (_chatLine && !_leftIsSpeaking)
            {
                // Partner types off-screen: show "typing…" for a while, then the whole bubble pops in.
                ShowPartnerTyping(true);
                _typeCarry += dt;
                if (_typeCarry >= Mathf.Max(MinPartnerTyping, _typeFull.Length / ChatCharsPerSecond))
                    FinishTypewriter();
                return;
            }

            _typeCarry += dt * (_chatLine ? ChatCharsPerSecond : CharsPerSecond);
            var advance = (int)_typeCarry;
            if (advance <= 0) return;
            _typeCarry -= advance;
            _typeIndex = Mathf.Min(_typeFull.Length, _typeIndex + advance);
            SetSpeakingLine(_typeFull.Substring(0, _typeIndex));
            if (_typeIndex >= _typeFull.Length)
                FinishTypewriter();
        }

        private void FinishTypewriter()
        {
            _typing = false;
            _typeDelay = 0f;
            _typeIndex = _typeFull.Length;
            if (_chatLine)
                SendChatBubble(_typeFull, mine: _leftIsSpeaking);
            else
                SetSpeakingLine(_typeFull);

            // Let the finished line breathe before the next speaker / the choices appear.
            _holding = true;
            _holdTimer = _episode.CurrentFrame.AutoAdvance
                ? Mathf.Min(HoldMax, HoldBase + _typeFull.Length * HoldPerChar)
                : ChoiceRevealDelay;
        }

        private void EndHold()
        {
            _holding = false;
            if (_episode.CurrentFrame.AutoAdvance)
            {
                RebuildChoices(visible: false);
                _episode.Advance();
                BeginFrame(forceRebuildChoices: true);
                return;
            }

            if (!_choicesReady)
            {
                _choicesReady = true;
                RebuildChoices(visible: true);
            }
        }

        private void SetSpeakingLine(string text)
        {
            if (_chatLine)
            {
                if (_leftIsSpeaking) SetChatInput(text);
                return;
            }

            if (_leftIsSpeaking) _leftLine.text = text;
            else _rightLine.text = text;
        }

        private void ApplySpeakerLayout(NegotiationFrame frame)
        {
            var leftSpeaking = IsLeftSpeaking(frame.PortraitId, frame.Speaker);
            _leftIsSpeaking = leftSpeaking;
            _isOutcome = frame.IsOutcome;

            if (leftSpeaking)
            {
                _leftName.text = Loc.T("speaker.intern");
                var listener = frame.ListenerPortraitId;
                if (!string.IsNullOrEmpty(listener) && listener != "intern")
                {
                    _partnerPortraitId = listener;
                    _rightName.text = SpeakerNameFor(listener);
                }
            }
            else
            {
                _rightName.text = string.IsNullOrEmpty(frame.Speaker) ? Loc.T("speaker.minjae") : frame.Speaker;
                if (!string.IsNullOrEmpty(frame.PortraitId) && frame.PortraitId != "intern")
                    _partnerPortraitId = frame.PortraitId;
            }

            _leftName.color = leftSpeaking ? Color.white : new Color(1f, 1f, 1f, 0.5f);
            _leftLine.color = leftSpeaking ? Color.white : new Color(1f, 1f, 1f, 0.45f);
            _leftUnderline.color = leftSpeaking ? LeftAccent : new Color(LeftAccent.r, LeftAccent.g, LeftAccent.b, 0.35f);

            _leftPlate.color = leftSpeaking ? PlateSpeaking : PlateListening;
            _rightPlate.color = leftSpeaking ? PlateListening : PlateSpeaking;

            _rightName.color = leftSpeaking ? new Color(1f, 1f, 1f, 0.5f) : Color.white;
            _rightLine.color = leftSpeaking ? new Color(1f, 1f, 1f, 0.45f) : Color.white;
            _rightUnderline.color = leftSpeaking
                ? new Color(RightAccent.r, RightAccent.g, RightAccent.b, 0.35f)
                : RightAccent;
        }

        private static string SpeakerNameFor(string portraitId) =>
            portraitId switch
            {
                "park" => Loc.T("speaker.park"),
                _ => Loc.T("speaker.minjae")
            };

        private static bool IsLeftSpeaking(string portraitId, string speaker)
        {
            if (portraitId == "intern") return true;
            if (speaker == Loc.T("speaker.you_to_minjae")) return true;
            if (speaker == Loc.T("speaker.outcome")) return true;
            return false;
        }

        private void FireInterrupt()
        {
            _inputConsumedFrame = Time.frameCount;
            _episode.SubmitInterrupt();
            BeginFrame(forceRebuildChoices: true);
        }

        private void UpdateEmotionBar()
        {
            if (_emotionRoot == null) return;
            var filled = EmotionBar.FilledCells(_episode.State.Emotion01);
            for (var i = 0; i < EmotionSegments; i++)
                _emotionCells[i].color = i < filled ? EmotionFill : EmotionEmpty;
        }

        private void UpdateInterruptHud(NegotiationFrame frame)
        {
            if (_interruptRoot == null) return;
            var show = _choicesReady && frame.AllowInterrupt && !frame.IsOutcome;
            _interruptRoot.SetActive(show);
            if (!show) return;
            _interruptPositive.gameObject.SetActive(frame.InterruptIsPositive);
            _interruptLabel.color = frame.InterruptIsPositive ? InterruptGreen : ChoiceText;
        }

        private void RebuildChoices(bool visible)
        {
            foreach (Transform child in _buttonRoot)
                Destroy(child.gameObject);
            _choiceButtons.Clear();

            _choiceCluster.SetActive(visible);
            if (!visible) return;

            var frame = _episode.CurrentFrame;
            for (var i = 0; i < frame.Choices.Length; i++)
                CreateChoice(frame.Choices[i]);

            _buttonRootRt.anchoredPosition = Vector2.zero;
        }

        // ---------- Split layout ----------

        private float SplitTarget()
        {
            // Choices belong to the intern (bottom-left) — give them room while the player decides.
            if (_isOutcome || _choicesReady && _choiceCluster.activeSelf) return SplitEqual;
            return _leftIsSpeaking ? SplitLeftSpeaks : SplitRightSpeaks;
        }

        private void AnimateLayout(float dt)
        {
            var k = 1f - Mathf.Exp(-SplitLerp * dt);
            var p = 1f - Mathf.Exp(-PortraitLerp * dt);

            var target = SplitTarget();
            _split = Mathf.Abs(_split - target) < 0.0005f ? target : Mathf.Lerp(_split, target, k);

            var leftFocus = _isOutcome || _leftIsSpeaking;
            var rightFocus = _isOutcome || !_leftIsSpeaking;
            _leftHeight = Mathf.Lerp(_leftHeight, leftFocus ? SpeakHeight : QuietHeight, p);
            _rightHeight = Mathf.Lerp(_rightHeight, rightFocus ? SpeakHeight : QuietHeight, p);
            _leftListen = Mathf.Lerp(_leftListen, leftFocus ? 0f : 1f, p);
            _rightListen = Mathf.Lerp(_rightListen, rightFocus ? 0f : 1f, p);

            ApplyLayout();
        }

        private void SnapLayout()
        {
            _split = SplitTarget();
            var leftFocus = _isOutcome || _leftIsSpeaking;
            var rightFocus = _isOutcome || !_leftIsSpeaking;
            _leftHeight = leftFocus ? SpeakHeight : QuietHeight;
            _rightHeight = rightFocus ? SpeakHeight : QuietHeight;
            _leftListen = leftFocus ? 0f : 1f;
            _rightListen = rightFocus ? 0f : 1f;
            ApplyLayout();
        }

        private void ApplyLayout()
        {
            var top = _split + SplitSlant * 0.5f;
            var bot = _split - SplitSlant * 0.5f;

            _leftRegion.SetCut(top, bot, keepLeft: true);
            _rightRegion.SetCut(top, bot, keepLeft: false);
            foreach (var seam in _seams)
                seam.SetCut(top, bot);

            var stage = _stage.rect;
            // Center each portrait on its own side at face height so the face stays visible.
            // While the messenger is open, both sit at the outer edges so the window doesn't cover faces.
            var faceEdge = Mathf.Lerp(bot, top, 0.68f);
            var leftX = Mathf.Lerp(faceEdge * 0.5f, ChatPortraitLeft, _chatBlend);
            var rightX = Mathf.Lerp(faceEdge + (1f - faceEdge) * 0.5f, ChatPortraitRight, _chatBlend);
            var chatScale = Mathf.Lerp(1f, ChatPortraitScale, _chatBlend);
            PlacePortrait(_leftPortrait, stage, leftX, _leftHeight * chatScale, _leftListen);
            PlacePortrait(_rightPortrait, stage, rightX, _rightHeight * chatScale, _rightListen);

            _leftDim.color = new Color(0f, 0f, 0f, 0.06f + _leftListen * 0.3f);
            _rightDim.color = new Color(0f, 0f, 0f, 0.06f + _rightListen * 0.3f);

            var textEdge = Mathf.Lerp(bot, top, TextAnchorY);
            var windowHalf = MessengerWidth * 0.5f / 1920f;
            SetGroupX(_leftTextGroup, 0.03f, Mathf.Lerp(textEdge - 0.03f, 0.49f - windowHalf, _chatBlend));
            SetGroupX(_rightTextGroup, Mathf.Lerp(textEdge + 0.03f, 0.51f + windowHalf, _chatBlend), 0.97f);
        }

        private static void PlacePortrait(RawImage portrait, Rect stage, float centerX01, float height01, float listen)
        {
            var tex = portrait.texture;
            var aspect = tex != null && tex.height > 0 ? (float)tex.width / tex.height : 0.75f;
            var h = stage.height * height01;
            var rt = portrait.rectTransform;
            rt.sizeDelta = new Vector2(h * aspect, h);
            rt.anchoredPosition = new Vector2(stage.width * centerX01, 0f);
            portrait.color = Color.Lerp(Color.white, ListenerTint, listen);
        }

        private static void SetGroupX(RectTransform group, float x0, float x1)
        {
            group.anchorMin = new Vector2(x0, 0f);
            group.anchorMax = new Vector2(Mathf.Max(x0 + 0.05f, x1), 1f);
            group.offsetMin = Vector2.zero;
            group.offsetMax = Vector2.zero;
        }

        // ---------- Tension ----------

        private void UpdateTension(NegotiationFrame frame)
        {
            var urgency = 0f;
            if (_choicesReady && frame.AllowSilence && !frame.IsOutcome && frame.SilenceFailSeconds > 0f)
                urgency = _episode.ChoiceUrgency01;

            var veil = urgency < UrgencyStart ? 0f : (urgency - UrgencyStart) / (1f - UrgencyStart);
            var pulse = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * Mathf.Lerp(3f, 10f, veil)));
            _urgencyVeil.color = new Color(0.55f, 0.02f, 0.02f, veil * 0.38f * pulse);

            if (urgency < UrgencyStart || !_choicesReady)
            {
                _buttonRootRt.anchoredPosition = Vector2.zero;
                _stage.anchoredPosition = Vector2.zero;
                TintChoiceTexts(ChoiceText);
                return;
            }

            var t = (urgency - UrgencyStart) / (1f - UrgencyStart);
            var amp = Mathf.Lerp(1.5f, 9f, t);
            var x = Mathf.Sin(Time.unscaledTime * Mathf.Lerp(22f, 48f, t)) * amp;
            var y = Mathf.Cos(Time.unscaledTime * Mathf.Lerp(16f, 40f, t)) * amp * 0.4f;
            _buttonRootRt.anchoredPosition = new Vector2(x, y);
            _stage.anchoredPosition = new Vector2(x * 0.12f, y * 0.08f);
            TintChoiceTexts(Color.Lerp(ChoiceText, ChoiceUrgent, t));
        }

        private void TintChoiceTexts(Color c)
        {
            for (var i = 0; i < _buttonRoot.childCount; i++)
            {
                var label = _buttonRoot.GetChild(i).Find("Label");
                var text = label != null ? label.GetComponent<Text>() : null;
                if (text != null) text.color = c;
            }
        }

        // ---------- Choices ----------

        private void CreateChoice(ChoiceOption choice)
        {
            // Label only — Hint stays in data for tools/debug, never shown to player
            CreateChoiceLabel(choice.Label, () =>
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

                _inputConsumedFrame = Time.frameCount;
                _episode.SubmitChoice(choice.Id);
                BeginFrame(forceRebuildChoices: true);
            });
        }

        private void CreateChoiceLabel(string label, UnityEngine.Events.UnityAction onClick)
        {
            // Numbered white boxes with a dark rim; number keys pick them too.
            var number = _choiceButtons.Count + 1;
            var go = new GameObject("Choice" + number, typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(_buttonRoot, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.96f);
            img.raycastTarget = true;
            var rim = go.GetComponent<Outline>();
            rim.effectColor = new Color(0.1f, 0.1f, 0.14f, 1f);
            rim.effectDistance = new Vector2(3f, -3f);
            var le = go.GetComponent<LayoutElement>();
            le.minHeight = 52f;
            le.preferredHeight = 54f;

            var numGo = new GameObject("Number", typeof(RectTransform), typeof(Image));
            numGo.transform.SetParent(go.transform, false);
            var numRt = numGo.GetComponent<RectTransform>();
            numRt.anchorMin = new Vector2(0f, 0.5f);
            numRt.anchorMax = new Vector2(0f, 0.5f);
            numRt.sizeDelta = new Vector2(38f, 38f);
            numRt.anchoredPosition = new Vector2(30f, 0f);
            var numBg = numGo.GetComponent<Image>();
            numBg.color = new Color(0.12f, 0.12f, 0.17f, 1f);
            numBg.raycastTarget = false;
            var numTextGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            numTextGo.transform.SetParent(numGo.transform, false);
            Stretch(numTextGo.GetComponent<RectTransform>(), 0f, 0f);
            var numText = numTextGo.GetComponent<Text>();
            numText.font = GetUiFont();
            numText.text = number.ToString();
            numText.fontSize = 22;
            numText.fontStyle = FontStyle.Bold;
            numText.color = Color.white;
            numText.alignment = TextAnchor.MiddleCenter;
            numText.raycastTarget = false;

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            Stretch(textRt, 0f, 0f);
            textRt.offsetMin = new Vector2(64f, 4f);
            textRt.offsetMax = new Vector2(-16f, -4f);
            var t = textGo.GetComponent<Text>();
            t.font = GetUiFont();
            t.text = label;
            t.color = ChoiceText;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleLeft;
            t.fontSize = 21;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;

            var btn = go.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.93f, 0.62f, 1f);
            colors.pressedColor = new Color(0.85f, 0.8f, 0.55f, 1f);
            colors.selectedColor = Color.white;
            btn.colors = colors;
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);
            _choiceButtons.Add(btn);
        }

        // ---------- Build ----------

        private void BuildUi()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            _stage = CreatePanel(canvasGo.transform, "Stage", new Color(0.04f, 0.04f, 0.06f, 1f));

            // Each side is a geometry stencil mask: background + portrait live inside and get cut by the seam.
            _leftRegion = CreateSideRegion(_stage, "LeftRegion", isLeft: true, out _leftSceneBg, out _leftPortrait, out _leftDim);
            _rightRegion = CreateSideRegion(_stage, "RightRegion", isLeft: false, out _rightSceneBg, out _rightPortrait, out _rightDim);

            _seams = new[]
            {
                CreateSeam(_stage, "SeamGlow", SeamGlow, width: 26f),
                CreateSeam(_stage, "SeamEdge", SeamDark, width: 11f),
                CreateSeam(_stage, "SeamCore", SeamYellow, width: 5f),
            };

            _leftTextGroup = CreateTextGroup(_stage, "LeftText", LeftAccent, out _leftName, out _leftUnderline, out _leftLine, out _leftPlate);
            _rightTextGroup = CreateTextGroup(_stage, "RightText", RightAccent, out _rightName, out _rightUnderline, out _rightLine, out _rightPlate);
            _leftName.text = Loc.T("speaker.intern");
            _rightName.text = Loc.T("speaker.minjae");

            var veilGo = new GameObject("UrgencyVeil", typeof(RectTransform), typeof(Image));
            veilGo.transform.SetParent(_stage, false);
            Stretch(veilGo.GetComponent<RectTransform>(), 0f, 0f);
            _urgencyVeil = veilGo.GetComponent<Image>();
            _urgencyVeil.color = new Color(0.5f, 0f, 0f, 0f);
            _urgencyVeil.raycastTarget = false;

            BuildMessenger(_stage);

            // Numbered choice boxes, bottom center (under the messenger)
            _choiceCluster = new GameObject("ChoiceCluster", typeof(RectTransform));
            _choiceCluster.transform.SetParent(_stage, false);
            var clusterRt = _choiceCluster.GetComponent<RectTransform>();
            clusterRt.anchorMin = new Vector2(0.22f, 0.025f);
            clusterRt.anchorMax = new Vector2(0.78f, 0.30f);
            clusterRt.offsetMin = Vector2.zero;
            clusterRt.offsetMax = Vector2.zero;

            var buttonPanel = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            buttonPanel.transform.SetParent(_choiceCluster.transform, false);
            _buttonRootRt = buttonPanel.GetComponent<RectTransform>();
            _buttonRootRt.anchorMin = new Vector2(0f, 0f);
            _buttonRootRt.anchorMax = new Vector2(1f, 0f);
            _buttonRootRt.pivot = new Vector2(0.5f, 0f);
            _buttonRootRt.offsetMin = Vector2.zero;
            _buttonRootRt.offsetMax = Vector2.zero;
            var v = buttonPanel.GetComponent<VerticalLayoutGroup>();
            v.spacing = 10f;
            v.childAlignment = TextAnchor.LowerCenter;
            v.childForceExpandHeight = false;
            v.childForceExpandWidth = true;
            v.childControlHeight = true;
            v.childControlWidth = true;
            buttonPanel.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            _buttonRoot = buttonPanel.transform;

            BuildEmotionBar(_stage);
            BuildInterruptButton(_stage);

            SetPortraitTexture(_leftPortrait, "intern", "right");
            SetPortraitTexture(_rightPortrait, "minjae", "left");
            SetBackgroundTexture(_leftSceneBg, "office_hr");
            SetBackgroundTexture(_rightSceneBg, "cafe");
        }

        private void BuildEmotionBar(Transform parent)
        {
            // Juusei-style: label + 10 discrete cells, bottom-right.
            _emotionRoot = new GameObject("EmotionBar", typeof(RectTransform));
            _emotionRoot.transform.SetParent(parent, false);
            var root = _emotionRoot.GetComponent<RectTransform>();
            root.anchorMin = new Vector2(0.72f, 0.02f);
            root.anchorMax = new Vector2(0.98f, 0.11f);
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(Outline));
            labelGo.transform.SetParent(root, false);
            SetAnchors(labelGo.GetComponent<RectTransform>(), 0f, 0.55f, 1f, 1f);
            var label = labelGo.GetComponent<Text>();
            label.font = GetUiFont();
            label.fontSize = 22;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = Color.white;
            label.text = Loc.T("ui.emotion");
            label.raycastTarget = false;
            AddTextEdge(labelGo, soft: false);

            var row = new GameObject("Cells", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(root, false);
            SetAnchors(row.GetComponent<RectTransform>(), 0f, 0.05f, 1f, 0.55f);
            var h = row.GetComponent<HorizontalLayoutGroup>();
            h.spacing = 4f;
            h.childForceExpandWidth = true;
            h.childForceExpandHeight = true;
            h.childControlWidth = true;
            h.childControlHeight = true;

            for (var i = 0; i < EmotionSegments; i++)
            {
                var cell = new GameObject("Cell" + i, typeof(RectTransform), typeof(Image), typeof(Outline));
                cell.transform.SetParent(row.transform, false);
                var img = cell.GetComponent<Image>();
                img.color = EmotionEmpty;
                img.raycastTarget = false;
                var outline = cell.GetComponent<Outline>();
                outline.effectColor = new Color(0.05f, 0.02f, 0.02f, 0.9f);
                outline.effectDistance = new Vector2(1.5f, -1.5f);
                _emotionCells[i] = img;
            }
        }

        private void BuildInterruptButton(Transform parent)
        {
            _interruptRoot = new GameObject("Interrupt", typeof(RectTransform));
            _interruptRoot.transform.SetParent(parent, false);
            var root = _interruptRoot.GetComponent<RectTransform>();
            root.anchorMin = new Vector2(0.78f, 0.32f);
            root.anchorMax = new Vector2(0.97f, 0.42f);
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            var posGo = new GameObject("Positive", typeof(RectTransform), typeof(Text), typeof(Outline));
            posGo.transform.SetParent(root, false);
            SetAnchors(posGo.GetComponent<RectTransform>(), 0f, 0.7f, 1f, 1f);
            _interruptPositive = posGo.GetComponent<Text>();
            _interruptPositive.font = GetUiFont();
            _interruptPositive.fontSize = 16;
            _interruptPositive.fontStyle = FontStyle.Bold;
            _interruptPositive.alignment = TextAnchor.LowerRight;
            _interruptPositive.color = InterruptGreen;
            _interruptPositive.text = Loc.T("ui.interrupt_positive");
            _interruptPositive.raycastTarget = false;
            AddTextEdge(posGo, soft: false);

            var btnGo = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Button));
            btnGo.transform.SetParent(root, false);
            SetAnchors(btnGo.GetComponent<RectTransform>(), 0f, 0f, 1f, 0.72f);
            var img = btnGo.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.95f);
            var rim = btnGo.GetComponent<Outline>();
            rim.effectColor = new Color(0.1f, 0.1f, 0.14f, 1f);
            rim.effectDistance = new Vector2(2f, -2f);
            _interruptButton = btnGo.GetComponent<Button>();
            _interruptButton.targetGraphic = img;
            _interruptButton.onClick.AddListener(() =>
            {
                if (!_choicesReady || !_episode.CurrentFrame.AllowInterrupt) return;
                FireInterrupt();
            });

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(btnGo.transform, false);
            Stretch(textGo.GetComponent<RectTransform>(), 8f, 4f);
            _interruptLabel = textGo.GetComponent<Text>();
            _interruptLabel.font = GetUiFont();
            _interruptLabel.fontSize = 22;
            _interruptLabel.fontStyle = FontStyle.Bold;
            _interruptLabel.alignment = TextAnchor.MiddleCenter;
            _interruptLabel.color = ChoiceText;
            _interruptLabel.text = "◎ " + Loc.T("ui.interrupt");
            _interruptLabel.raycastTarget = false;

            _interruptRoot.SetActive(false);
        }

        private static SplitRegionGraphic CreateSideRegion(
            Transform parent,
            string name,
            bool isLeft,
            out RawImage sceneBg,
            out RawImage portrait,
            out Image dim)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(SplitRegionGraphic), typeof(Mask));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            Stretch(rt, 0f, 0f);
            var region = go.GetComponent<SplitRegionGraphic>();
            region.color = Color.white;
            region.raycastTarget = false;
            go.GetComponent<Mask>().showMaskGraphic = false;

            var fallbackGo = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            fallbackGo.transform.SetParent(rt, false);
            Stretch(fallbackGo.GetComponent<RectTransform>(), 0f, 0f);
            var fallback = fallbackGo.GetComponent<Image>();
            fallback.color = isLeft ? new Color(0.07f, 0.08f, 0.11f, 1f) : new Color(0.11f, 0.07f, 0.06f, 1f);
            fallback.raycastTarget = false;

            var sceneGo = new GameObject("SceneBg", typeof(RectTransform), typeof(RawImage));
            sceneGo.transform.SetParent(rt, false);
            Stretch(sceneGo.GetComponent<RectTransform>(), 0f, 0f);
            sceneBg = sceneGo.GetComponent<RawImage>();
            sceneBg.enabled = false;
            sceneBg.raycastTarget = false;

            var portraitGo = new GameObject("Portrait", typeof(RectTransform), typeof(RawImage));
            portraitGo.transform.SetParent(rt, false);
            var prt = portraitGo.GetComponent<RectTransform>();
            prt.anchorMin = Vector2.zero;
            prt.anchorMax = Vector2.zero;
            prt.pivot = new Vector2(0.5f, 0f);
            portrait = portraitGo.GetComponent<RawImage>();
            portrait.raycastTarget = false;
            portrait.enabled = false;

            var dimGo = new GameObject("Dim", typeof(RectTransform), typeof(Image));
            dimGo.transform.SetParent(rt, false);
            Stretch(dimGo.GetComponent<RectTransform>(), 0f, 0f);
            dim = dimGo.GetComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.1f);
            dim.raycastTarget = false;

            return region;
        }

        private static SplitSeamGraphic CreateSeam(Transform parent, string name, Color color, float width)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(SplitSeamGraphic));
            go.transform.SetParent(parent, false);
            Stretch(go.GetComponent<RectTransform>(), 0f, 0f);
            var seam = go.GetComponent<SplitSeamGraphic>();
            seam.color = color;
            seam.raycastTarget = false;
            seam.Configure(width, zigzag: 9f);
            return seam;
        }

        private static RectTransform CreateTextGroup(
            Transform parent,
            string name,
            Color accent,
            out Text nameText,
            out Image underline,
            out Text lineText,
            out Image plate)
        {
            // No dialogue box — Juusei-style floating name + line with hard outline, left-aligned next to the seam.
            var group = new GameObject(name, typeof(RectTransform));
            group.transform.SetParent(parent, false);
            var groupRt = group.GetComponent<RectTransform>();
            Stretch(groupRt, 0f, 0f);

            // Busy office backgrounds and big chibi heads sit behind the text; a dark plate keeps it readable.
            var plateGo = new GameObject("Plate", typeof(RectTransform), typeof(Image));
            plateGo.transform.SetParent(groupRt, false);
            var plateRt = plateGo.GetComponent<RectTransform>();
            SetAnchors(plateRt, 0f, 0.24f, 1f, 0.60f);
            plateRt.offsetMin = new Vector2(-18f, 0f);
            plateRt.offsetMax = new Vector2(18f, 0f);
            plate = plateGo.GetComponent<Image>();
            plate.color = PlateSpeaking;
            plate.raycastTarget = false;

            var nameGo = new GameObject("Name", typeof(RectTransform), typeof(Text), typeof(Shadow), typeof(Outline));
            nameGo.transform.SetParent(groupRt, false);
            SetAnchors(nameGo.GetComponent<RectTransform>(), 0f, 0.50f, 1f, 0.58f);
            nameText = nameGo.GetComponent<Text>();
            nameText.font = GetUiFont();
            nameText.fontSize = 34;
            nameText.fontStyle = FontStyle.Bold;
            nameText.color = Color.white;
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.horizontalOverflow = HorizontalWrapMode.Overflow;
            nameText.raycastTarget = false;
            AddTextEdge(nameGo, soft: false);

            var underGo = new GameObject("Underline", typeof(RectTransform), typeof(Image));
            underGo.transform.SetParent(groupRt, false);
            SetAnchors(underGo.GetComponent<RectTransform>(), 0f, 0.488f, 0.6f, 0.496f);
            underline = underGo.GetComponent<Image>();
            underline.color = accent;
            underline.raycastTarget = false;

            var lineGo = new GameObject("Line", typeof(RectTransform), typeof(Text), typeof(Shadow), typeof(Outline));
            lineGo.transform.SetParent(groupRt, false);
            SetAnchors(lineGo.GetComponent<RectTransform>(), 0f, 0.26f, 1f, 0.48f);
            lineText = lineGo.GetComponent<Text>();
            lineText.font = GetUiFont();
            lineText.fontSize = 26;
            lineText.color = Color.white;
            lineText.alignment = TextAnchor.UpperLeft;
            lineText.horizontalOverflow = HorizontalWrapMode.Wrap;
            lineText.verticalOverflow = VerticalWrapMode.Overflow;
            lineText.raycastTarget = false;
            lineText.text = "";
            AddTextEdge(lineGo, soft: false);

            return groupRt;
        }

        // ---------- Messenger ----------

        private void BuildMessenger(Transform parent)
        {
            var root = new GameObject("Messenger", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(Outline));
            root.transform.SetParent(parent, false);
            _messengerRt = root.GetComponent<RectTransform>();
            _messengerRt.anchorMin = new Vector2(0.5f, 1f);
            _messengerRt.anchorMax = new Vector2(0.5f, 1f);
            _messengerRt.pivot = new Vector2(0.5f, 1f);
            _messengerRt.sizeDelta = new Vector2(MessengerWidth, MessengerHeight);
            _messengerRt.anchoredPosition = new Vector2(0f, -MessengerTop);
            var frameImg = root.GetComponent<Image>();
            frameImg.color = new Color(0.17f, 0.19f, 0.26f, 1f);
            frameImg.raycastTarget = false;
            var rim = root.GetComponent<Outline>();
            rim.effectColor = new Color(0f, 0f, 0f, 0.55f);
            rim.effectDistance = new Vector2(4f, -4f);
            _messenger = root.GetComponent<CanvasGroup>();
            _messenger.alpha = 0f;
            _messenger.blocksRaycasts = false;
            _messenger.interactable = false;

            // Header: partner avatar, name, online dot
            var header = new GameObject("Header", typeof(RectTransform), typeof(Image));
            header.transform.SetParent(root.transform, false);
            var headerRt = header.GetComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0f, 1f);
            headerRt.anchorMax = new Vector2(1f, 1f);
            headerRt.pivot = new Vector2(0.5f, 1f);
            headerRt.sizeDelta = new Vector2(0f, ChatHeaderHeight);
            headerRt.anchoredPosition = Vector2.zero;
            header.GetComponent<Image>().color = new Color(0.22f, 0.25f, 0.34f, 1f);

            var avatarRt = CreateAvatar(headerRt, 52f);
            avatarRt.anchorMin = new Vector2(0f, 0.5f);
            avatarRt.anchorMax = new Vector2(0f, 0.5f);
            avatarRt.anchoredPosition = new Vector2(44f, 0f);

            var nameText = CreateChatText(headerRt, "Name", 24, Color.white, TextAnchor.MiddleLeft);
            nameText.fontStyle = FontStyle.Bold;
            nameText.text = Loc.T("speaker.minjae");
            SetOffsets(nameText.rectTransform, 84f, 34f, -20f, -6f);

            var dotGo = new GameObject("OnlineDot", typeof(RectTransform), typeof(Image));
            dotGo.transform.SetParent(headerRt, false);
            var dotRt = dotGo.GetComponent<RectTransform>();
            dotRt.anchorMin = new Vector2(0f, 0f);
            dotRt.anchorMax = new Vector2(0f, 0f);
            dotRt.sizeDelta = new Vector2(10f, 10f);
            dotRt.anchoredPosition = new Vector2(90f, 20f);
            dotGo.GetComponent<Image>().color = new Color(0.3f, 0.85f, 0.4f, 1f);

            var status = CreateChatText(headerRt, "Status", 16, new Color(0.75f, 0.78f, 0.85f, 1f), TextAnchor.MiddleLeft);
            status.text = Loc.T("ui.chat.online");
            SetOffsets(status.rectTransform, 102f, 6f, -20f, -40f);

            // Body: bubbles stack up from the bottom; old ones scroll out under the mask.
            var body = new GameObject("Body", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            body.transform.SetParent(root.transform, false);
            _chatBody = body.GetComponent<RectTransform>();
            Stretch(_chatBody, 0f, 0f);
            _chatBody.offsetMin = new Vector2(6f, ChatInputHeight);
            _chatBody.offsetMax = new Vector2(-6f, -ChatHeaderHeight);
            body.GetComponent<Image>().color = new Color(0.92f, 0.93f, 0.96f, 1f);

            _typingRow = CreatePartnerRow(out _typingText, out _);
            _typingRow.gameObject.SetActive(false);

            // Input bar
            var input = new GameObject("Input", typeof(RectTransform), typeof(Image));
            input.transform.SetParent(root.transform, false);
            var inputRt = input.GetComponent<RectTransform>();
            inputRt.anchorMin = new Vector2(0f, 0f);
            inputRt.anchorMax = new Vector2(1f, 0f);
            inputRt.pivot = new Vector2(0.5f, 0f);
            inputRt.sizeDelta = new Vector2(-12f, ChatInputHeight - 12f);
            inputRt.anchoredPosition = new Vector2(0f, 6f);
            input.GetComponent<Image>().color = Color.white;

            _chatInput = CreateChatText(inputRt, "Text", 20, ChatText, TextAnchor.MiddleLeft);
            SetOffsets(_chatInput.rectTransform, 18f, 4f, -18f, -4f);
            _chatInput.horizontalOverflow = HorizontalWrapMode.Wrap;
            _chatInput.verticalOverflow = VerticalWrapMode.Truncate;
            SetChatInput("");
        }

        private void AnimateChat(float dt)
        {
            var k = 1f - Mathf.Exp(-ChatBlendLerp * dt);
            _chatBlend = Mathf.Abs(_chatBlend - _chatTarget) < 0.001f ? _chatTarget : Mathf.Lerp(_chatBlend, _chatTarget, k);
            _messenger.alpha = _chatBlend;
            _messengerRt.anchoredPosition = new Vector2(0f, -MessengerTop + (1f - _chatBlend) * 40f);

            if (_partnerTyping)
            {
                var dots = 1 + (int)(Time.unscaledTime * 3f) % 3;
                _typingText.text = Loc.T("ui.chat.typing") + " " + new string('•', dots);
            }

            if (_popRow != null && _popT < 1f)
            {
                _popT = Mathf.Min(1f, _popT + dt * 7f);
                var e = 1f - (1f - _popT) * (1f - _popT);
                _popRow.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, e);
            }
        }

        private void SetChatInput(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                _chatInput.text = Loc.T("ui.chat.placeholder");
                _chatInput.color = new Color(0.6f, 0.62f, 0.68f, 1f);
                return;
            }

            _chatInput.text = text;
            _chatInput.color = ChatText;
        }

        private void ShowPartnerTyping(bool on)
        {
            if (_partnerTyping == on) return;
            _partnerTyping = on;
            if (on)
            {
                _typingText.text = Loc.T("ui.chat.typing");
                SizeBubble(_typingRow, _typingText, Loc.T("ui.chat.typing") + " •••", mine: false);
            }
            _typingRow.gameObject.SetActive(on);
            LayoutChat();
        }

        private void SendChatBubble(string text, bool mine)
        {
            ShowPartnerTyping(false);
            if (mine)
            {
                SetChatInput("");
            }
            else
            {
                MarkChatSeen();
            }

            Text bubbleText;
            Text unread;
            var rowRt = mine ? CreateMineRow(out bubbleText, out unread) : CreatePartnerRow(out bubbleText, out unread);
            var height = SizeBubble(rowRt, bubbleText, text, mine);
            _chatRows.Add(new ChatRow { Rt = rowRt, Height = height, Unread = unread });

            if (_popRow != null) _popRow.localScale = Vector3.one;
            _popRow = rowRt;
            _popT = 0f;
            rowRt.localScale = Vector3.one * 0.85f;
            LayoutChat();
        }

        private void MarkChatSeen()
        {
            foreach (var row in _chatRows)
                if (row.Unread != null) row.Unread.text = "";
        }

        private void ClearChat()
        {
            foreach (var row in _chatRows)
                if (row.Rt != null) Destroy(row.Rt.gameObject);
            _chatRows.Clear();
            _popRow = null!;
            ShowPartnerTyping(false);
            SetChatInput("");
        }

        private void LayoutChat()
        {
            var y = 14f;
            if (_typingRow.gameObject.activeSelf)
            {
                _typingRow.anchoredPosition = new Vector2(0f, y);
                y += _typingRow.sizeDelta.y + BubbleSpacing;
            }

            var bodyHeight = MessengerHeight - ChatHeaderHeight - ChatInputHeight;
            for (var i = _chatRows.Count - 1; i >= 0; i--)
            {
                var row = _chatRows[i];
                row.Rt.anchoredPosition = new Vector2(0f, y);
                y += row.Height + BubbleSpacing;
                if (y > bodyHeight * 2f && i > 0)
                {
                    for (var j = 0; j < i; j++)
                        Destroy(_chatRows[j].Rt.gameObject);
                    _chatRows.RemoveRange(0, i);
                    break;
                }
            }
        }

        private RectTransform CreateRow(string name)
        {
            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(_chatBody, false);
            var rt = row.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            return rt;
        }

        private RectTransform CreatePartnerRow(out Text text, out Text unread)
        {
            var row = CreateRow("Their");
            var art = CreateAvatar(row, ChatAvatarSize);
            art.anchorMin = new Vector2(0f, 1f);
            art.anchorMax = new Vector2(0f, 1f);
            art.pivot = new Vector2(0f, 1f);
            art.anchoredPosition = new Vector2(12f, 0f);

            var bubble = CreateBubble(row, TheirBubble, out text);
            bubble.anchorMin = new Vector2(0f, 0f);
            bubble.anchorMax = new Vector2(0f, 0f);
            bubble.pivot = new Vector2(0f, 0f);
            bubble.anchoredPosition = new Vector2(12f + ChatAvatarSize + 10f, 0f);
            unread = null!;
            return row;
        }

        private RectTransform CreateMineRow(out Text text, out Text unread)
        {
            var row = CreateRow("Mine");
            var bubble = CreateBubble(row, MineBubble, out text);
            bubble.anchorMin = new Vector2(1f, 0f);
            bubble.anchorMax = new Vector2(1f, 0f);
            bubble.pivot = new Vector2(1f, 0f);
            bubble.anchoredPosition = new Vector2(-14f, 0f);

            unread = CreateChatText(row, "Unread", 16, UnreadColor, TextAnchor.LowerRight);
            unread.fontStyle = FontStyle.Bold;
            unread.text = Loc.T("ui.chat.unread");
            var urt = unread.rectTransform;
            urt.anchorMin = new Vector2(1f, 0f);
            urt.anchorMax = new Vector2(1f, 0f);
            urt.pivot = new Vector2(1f, 0f);
            urt.sizeDelta = new Vector2(30f, 22f);
            return row;
        }

        private RectTransform CreateBubble(RectTransform row, Color color, out Text text)
        {
            var go = new GameObject("Bubble", typeof(RectTransform), typeof(Image), typeof(Shadow));
            go.transform.SetParent(row, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            var shadow = go.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.12f);
            shadow.effectDistance = new Vector2(0f, -2f);

            text = CreateChatText(go.GetComponent<RectTransform>(), "Text", 22, ChatText, TextAnchor.UpperLeft);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            SetOffsets(text.rectTransform, BubblePadX, BubblePadY, -BubblePadX, -BubblePadY);
            return go.GetComponent<RectTransform>();
        }

        /// <summary>Sizes the bubble to its wrapped text and returns the row height.</summary>
        private static float SizeBubble(RectTransform row, Text text, string content, bool mine)
        {
            text.text = content;
            var gen = text.cachedTextGeneratorForLayout;
            var ppu = Mathf.Max(0.01f, text.pixelsPerUnit);
            var wide = text.GetGenerationSettings(Vector2.zero);
            wide.horizontalOverflow = HorizontalWrapMode.Overflow;
            var w = Mathf.Min(BubbleMaxTextWidth, gen.GetPreferredWidth(content, wide) / ppu + 2f);
            var h = gen.GetPreferredHeight(content, text.GetGenerationSettings(new Vector2(w, 0f))) / ppu;

            var bubble = (RectTransform)text.transform.parent;
            bubble.sizeDelta = new Vector2(w + BubblePadX * 2f, h + BubblePadY * 2f);
            var rowHeight = Mathf.Max(bubble.sizeDelta.y, mine ? 0f : ChatAvatarSize);
            row.sizeDelta = new Vector2(0f, rowHeight);

            var unread = row.Find("Unread");
            if (unread != null)
                ((RectTransform)unread).anchoredPosition = new Vector2(-14f - bubble.sizeDelta.x - 6f, 0f);
            return rowHeight;
        }

        private static RectTransform CreateAvatar(RectTransform parent, float size)
        {
            var go = new GameObject("Avatar", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(size, size);
            var ring = go.GetComponent<Image>();
            ring.color = new Color(1f, 0.75f, 0.85f, 1f);
            ring.raycastTarget = false;

            var faceGo = new GameObject("Face", typeof(RectTransform), typeof(RawImage));
            faceGo.transform.SetParent(rt, false);
            Stretch(faceGo.GetComponent<RectTransform>(), 2f, 2f);
            var face = faceGo.GetComponent<RawImage>();
            face.texture = Resources.Load<Texture2D>("Portraits/minjae");
            // Head crop of the 3:4 portrait
            face.uvRect = new Rect(0.25f, 0.47f, 0.50f, 0.375f);
            face.raycastTarget = false;
            return rt;
        }

        private static Text CreateChatText(RectTransform parent, string name, int size, Color color, TextAnchor align)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Stretch(go.GetComponent<RectTransform>(), 0f, 0f);
            var t = go.GetComponent<Text>();
            t.font = GetUiFont();
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private static void SetOffsets(RectTransform rt, float left, float bottom, float right, float top)
        {
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(right, top);
        }

        private static void AddTextEdge(GameObject go, bool soft)
        {
            var shadow = go.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, soft ? 0.85f : 0.98f);
            shadow.effectDistance = soft ? new Vector2(1.5f, -1.5f) : new Vector2(2.5f, -2.5f);
            var outline = go.GetComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, soft ? 0.8f : 0.95f);
            outline.effectDistance = soft ? new Vector2(1.2f, -1.2f) : new Vector2(2f, -2f);
        }

        // ---------- Art ----------

        private void ApplyPortraits(NegotiationFrame frame)
        {
            var portraitId = frame.PortraitId;
            if (!string.IsNullOrEmpty(portraitId) && portraitId != "intern")
                _partnerPortraitId = portraitId;

            var partnerId = string.IsNullOrEmpty(_partnerPortraitId) ? "minjae" : _partnerPortraitId;
            var pose = frame.PortraitPose ?? "";

            // Left faces right (toward partner); right faces left (toward intern). "chat" = typing / texting.
            var leftPose = pose == "front" ? "" : string.IsNullOrEmpty(pose) ? "right" : pose;
            var rightPose = pose == "front" ? "" : string.IsNullOrEmpty(pose) ? "left" : pose;

            SetPortraitTexture(_leftPortrait, "intern", leftPose);
            SetPortraitTexture(_rightPortrait, partnerId, rightPose);
        }

        private void ApplyBackgrounds(NegotiationFrame frame)
        {
            var leftId = string.IsNullOrEmpty(frame.LeftBackgroundId) ? "office_hr" : frame.LeftBackgroundId;
            var rightId = string.IsNullOrEmpty(frame.RightBackgroundId) ? _partnerBackgroundId : frame.RightBackgroundId;
            if (!string.IsNullOrEmpty(frame.RightBackgroundId))
                _partnerBackgroundId = frame.RightBackgroundId;

            SetBackgroundTexture(_leftSceneBg, leftId);
            SetBackgroundTexture(_rightSceneBg, rightId);
        }

        private static void SetBackgroundTexture(RawImage target, string backgroundId)
        {
            if (target == null) return;
            var tex = Resources.Load<Texture2D>("Backgrounds/" + backgroundId);
            if (tex == null)
            {
                target.enabled = false;
                return;
            }

            target.texture = tex;
            target.enabled = true;
            // Soft wash behind moe portraits — keep atmosphere, not a second focal plane
            target.color = new Color(0.88f, 0.88f, 0.9f, 1f);
        }

        private static void SetPortraitTexture(RawImage target, string portraitId, string pose)
        {
            if (target == null) return;
            Texture2D tex = null!;
            if (!string.IsNullOrEmpty(pose))
                tex = Resources.Load<Texture2D>("Portraits/" + portraitId + "_" + pose);
            if (tex == null)
                tex = Resources.Load<Texture2D>("Portraits/" + portraitId);
            if (tex == null)
                return; // keep last texture — characters shrink, never vanish

            target.texture = tex;
            target.enabled = true;
        }

        // ---------- Helpers ----------

        private static RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            Stretch(go.GetComponent<RectTransform>(), 0f, 0f);
            return go.GetComponent<RectTransform>();
        }

        private static Font GetUiFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static void Stretch(RectTransform rt, float padX, float padY)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padX, padY);
            rt.offsetMax = new Vector2(-padX, -padY);
        }

        private static void SetAnchors(RectTransform rt, float x0, float y0, float x1, float y1)
        {
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static int PressedChoiceIndex()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return -1;
            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame) return 0;
            if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame) return 1;
            if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame) return 2;
            if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame) return 3;
            return -1;
        }

        private static bool WasInterruptPressed()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.iKey.wasPressedThisFrame;
        }

        private static bool WasSkipPressed()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
                return true;
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                return true;
            return false;
        }
    }
}
#endif
