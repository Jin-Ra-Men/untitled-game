using System;
using UntitledGame.Negotiation.Localization;

namespace UntitledGame.Negotiation.Episodes
{
    /// <summary>
    /// Test episode: family certificate. Han Minji joined two weeks ago and still hasn't submitted it;
    /// the manager orders it in person, the negotiation itself happens over the company messenger.
    /// Logic only — player-facing text via Loc keys (fc.*, speaker.*, ui.*).
    /// </summary>
    public sealed class FamilyCertificateEpisode : INegotiationEpisode
    {
        private enum Step
        {
            PreChoice,
            Beat1,
            Beat2,
            Beat3,
            Beat4,
            Bridge,
            Outcome
        }

        /// <summary>How a line reaches the other person.</summary>
        private enum Via
        {
            Speech,
            Chat,
            Think
        }

        private readonly struct Line
        {
            public Line(string who, string key, Via via, bool seen = false)
            {
                Who = who;
                Key = key;
                Via = via;
                Seen = seen;
            }

            public string Who { get; }
            public string Key { get; }
            public Via Via { get; }
            public bool Seen { get; }
        }

        private const string Park = "park";
        private const string Intern = "intern";
        private const string Minji = "minjae";

        private Step _step;
        private NegotiationFrame _frame = new NegotiationFrame();
        private float _silenceHeld;
        private float _interruptHeld;
        private bool _waitingInput;
        private Action? _afterBridge;
        private string _partnerBackgroundId = "office_manager";
        private string _talkingTo = Park;
        private bool _messengerOpen;
        private int _chatSession;

        public string Title => Loc.T("fc.title");
        public NegotiationState State { get; private set; } = new NegotiationState();
        public NegotiationFrame CurrentFrame => _frame;
        public bool IsFinished => _step == Step.Outcome;

        /// <summary>0..1 how close we are to auto-silence. Used for choice shake UI.</summary>
        public float ChoiceUrgency01
        {
            get
            {
                if (!_waitingInput || !_frame.AllowSilence || _frame.SilenceFailSeconds <= 0f)
                    return 0f;
                return Math.Clamp(_silenceHeld / _frame.SilenceFailSeconds, 0f, 1f);
            }
        }

        public void Start()
        {
            State = new NegotiationState { MinutesLeft = 14f, Emotion01 = 0.4f };
            _messengerOpen = false;
            _talkingTo = Park;
            Talk(ShowPreChoice,
                S(Park, "fc.open.1"),
                S(Intern, "fc.open.2"),
                S(Park, "fc.open.3"),
                S(Intern, "fc.open.4"),
                S(Park, "fc.open.5"),
                S(Intern, "fc.open.6"));
        }

        /// <summary>Advance auto-bridge lines after the UI finishes presenting them.</summary>
        public void Advance()
        {
            if (_step != Step.Bridge || !_frame.AutoAdvance) return;
            var next = _afterBridge;
            _afterBridge = null;
            next?.Invoke();
        }

        public void RestartFromNegotiation()
        {
            var prepared = State.GuidePrepared;
            var eye = State.ManagerEyeRaised;
            State = new NegotiationState
            {
                MinutesLeft = prepared ? 12f : 14f,
                GuidePrepared = prepared,
                ManagerEyeRaised = eye,
                Cooperation = prepared ? 1 : 0,
                Emotion01 = 0.45f
            };
            State.Clamp();
            _messengerOpen = false;
            _chatSession++;
            Talk(StartChat, T("fc.restart_pending"));
        }

        public void SubmitChoice(string choiceId)
        {
            if (!_waitingInput) return;
            _silenceHeld = 0f;
            _interruptHeld = 0f;

            switch (_step)
            {
                case Step.Outcome:
                    if (choiceId == "retry") RestartFromNegotiation();
                    return;
                case Step.Bridge:
                    Advance();
                    return;
                case Step.PreChoice:
                    ApplyPreChoice(choiceId);
                    break;
                case Step.Beat1:
                    ApplyBeat1(choiceId, InputKind.Choice);
                    break;
                case Step.Beat2:
                    ApplyBeat2(choiceId, InputKind.Choice);
                    break;
                case Step.Beat3:
                    ApplyBeat3(choiceId, InputKind.Choice);
                    break;
                case Step.Beat4:
                    ApplyBeat4(choiceId, InputKind.Choice);
                    break;
            }
        }

        public void SubmitSilence()
        {
            if (!_waitingInput || !_frame.AllowSilence || _step == Step.Outcome || _step == Step.Bridge)
                return;
            _silenceHeld = 0f;
            _interruptHeld = 0f;

            switch (_step)
            {
                case Step.Beat1:
                    ApplyBeat1("silence", InputKind.Silence);
                    break;
                case Step.Beat2:
                    ApplyBeat2("silence", InputKind.Silence);
                    break;
                case Step.Beat3:
                    ApplyBeat3("silence", InputKind.Silence);
                    break;
                case Step.Beat4:
                    ApplyBeat4("silence", InputKind.Silence);
                    break;
            }
        }

        public void SubmitInterrupt()
        {
            if (!_waitingInput || !_frame.AllowInterrupt || _step == Step.Outcome || _step == Step.Bridge)
                return;
            if (_step is not (Step.Beat1 or Step.Beat2 or Step.Beat3 or Step.Beat4))
                return;

            var positive = _frame.InterruptIsPositive;
            var resume = _step;
            _silenceHeld = 0f;
            _interruptHeld = 0f;
            _frame.AllowInterrupt = false;
            Spend(0.4f);

            Action back = () =>
            {
                switch (resume)
                {
                    case Step.Beat1: EnterBeat1(); break;
                    case Step.Beat2: EnterBeat2(); break;
                    case Step.Beat3: EnterBeat3(); break;
                    case Step.Beat4: EnterBeat4(); break;
                }
            };

            if (positive)
            {
                TouchEmotion(-0.12f);
                State.Cooperation += 1;
                State.Clamp();
                Talk(back,
                    C(Intern, "fc.interrupt.pos.say"),
                    C(Minji, "fc.interrupt.pos.reply"));
            }
            else
            {
                TouchEmotion(0.22f);
                State.Cooperation -= 1;
                State.Pride -= 1;
                State.Clamp();
                Talk(back,
                    C(Intern, "fc.interrupt.neg.say"),
                    C(Minji, "fc.interrupt.neg.reply"));
            }
        }

        public void TickRealtime(float deltaSeconds)
        {
            if (!_waitingInput || _step == Step.Outcome || _step == Step.Bridge) return;

            if (_frame.AllowInterrupt && _frame.InterruptWindowSeconds > 0f)
            {
                _interruptHeld += deltaSeconds;
                if (_interruptHeld >= _frame.InterruptWindowSeconds)
                    _frame.AllowInterrupt = false;
            }

            if (_frame.AllowSilence)
            {
                _silenceHeld += deltaSeconds;
                if (_silenceHeld >= _frame.SilenceFailSeconds)
                    SubmitSilence();
            }
        }

        // ---------- Before messaging ----------

        private void ShowPreChoice()
        {
            _step = Step.PreChoice;
            _waitingInput = true;
            _frame = new NegotiationFrame
            {
                Phase = EpisodePhase.PreChoice,
                Speaker = Loc.T("speaker.intern"),
                Line = Loc.T("fc.pre.prompt"),
                StatusLine = Loc.F("fc.pre.status", State.DebugSummary()),
                PortraitId = Intern,
                PortraitPose = "front",
                LeftBackgroundId = "office_hr",
                RightBackgroundId = _partnerBackgroundId,
                ChatSession = _chatSession,
                Choices = new[]
                {
                    Loc.Choice("A", "fc.pre.a"),
                    Loc.Choice("B", "fc.pre.b", "fc.pre.b_hint"),
                    Loc.Choice("C", "fc.pre.c", "fc.pre.c_hint")
                }
            };
        }

        private void ApplyPreChoice(string id)
        {
            switch (id)
            {
                case "B":
                    State.GuidePrepared = true;
                    State.MinutesLeft -= 2f;
                    State.Cooperation += 1;
                    State.Clamp();
                    Talk(StartChat,
                        T("fc.pre.b.say"),
                        T("fc.pre.b.after"));
                    return;
                case "C":
                    State.ManagerEyeRaised = true;
                    State.MinutesLeft -= 1f;
                    State.Clamp();
                    Talk(StartChat,
                        S(Intern, "fc.pre.c.say"),
                        S(Park, "fc.pre.c.park"),
                        S(Intern, "fc.pre.c.after"));
                    return;
                default:
                    Talk(StartChat, T("fc.pre.a.say"));
                    return;
            }
        }

        // ---------- Messenger ----------

        private void StartChat()
        {
            _messengerOpen = true;
            _talkingTo = Minji;
            Talk(EnterBeat1,
                C(Intern, "fc.chat.1"),
                C(Intern, "fc.chat.2"),
                Seen("fc.chat.seen"),
                C(Minji, "fc.chat.3"));
        }

        private void EnterBeat1()
        {
            _step = Step.Beat1;
            ArmTimedPrompt(EpisodePhase.Beat1, C(Minji, "fc.b1.line"), Loc.T("fc.b1.status"),
                Loc.Choice("rule", "fc.b1.choice.rule"),
                Loc.Choice("family", "fc.b1.choice.family", "fc.b1.choice.family_hint"),
                Loc.Choice("money", "fc.b1.choice.money", "fc.b1.choice.money_hint"));
        }

        private void ApplyBeat1(string id, InputKind kind)
        {
            Spend(0.8f);

            if (kind == InputKind.Silence)
            {
                State.Cooperation -= 1;
                State.PrivacyRaised = true;
                TouchEmotion(0.1f);
                State.Clamp();
                Talk(EnterBeat2,
                    C(Minji, "fc.b1.silence.1"),
                    C(Minji, "fc.b1.silence.2"),
                    C(Intern, "fc.b1.silence.you"),
                    C(Minji, "fc.b1.silence.3"),
                    C(Minji, "fc.b2.ask"));
                return;
            }

            switch (id)
            {
                case "family":
                    State.TriggerFired = true;
                    State.Pride -= 2;
                    State.Cooperation -= 1;
                    TouchEmotion(0.28f);
                    State.Clamp();
                    Talk(EnterBeat2,
                        C(Intern, "fc.b1.choice.family.say"),
                        C(Minji, "fc.b1.family.1"),
                        C(Minji, "fc.b1.family.2"),
                        C(Minji, "fc.b1.family.3"),
                        T("fc.b1.family.think"),
                        C(Intern, "fc.b1.family.sorry"),
                        C(Minji, "fc.b2.ask"));
                    return;
                case "money":
                    State.Cooperation += 1;
                    State.Pride += 1;
                    TouchEmotion(-0.08f);
                    State.Clamp();
                    Talk(EnterBeat2,
                        C(Intern, "fc.b1.choice.money.say"),
                        C(Minji, "fc.b1.money.1"),
                        C(Minji, "fc.b1.money.2"),
                        T("fc.b1.money.think"),
                        C(Minji, "fc.b2.ask"));
                    return;
                default:
                    State.Cooperation += 1;
                    State.PrivacyRaised = true;
                    TouchEmotion(-0.05f);
                    State.Clamp();
                    Talk(EnterBeat2,
                        C(Intern, "fc.b1.choice.rule.say"),
                        C(Minji, "fc.b1.rule.1"),
                        C(Intern, "fc.b1.rule.2"),
                        C(Minji, "fc.b1.rule.3"),
                        C(Minji, "fc.b2.ask"));
                    return;
            }
        }

        private void EnterBeat2()
        {
            _step = Step.Beat2;
            ArmTimedPrompt(EpisodePhase.Beat2, C(Minji, "fc.b2.line"), Loc.T("fc.b2.status"),
                Loc.Choice("online", "fc.b2.choice.online"),
                Loc.Choice("push", "fc.b2.choice.push"),
                Loc.Choice("snark", "fc.b2.choice.snark", "fc.b2.choice.snark_hint"));
        }

        private void ApplyBeat2(string id, InputKind kind)
        {
            if (kind == InputKind.Silence)
            {
                State.MinutesLeft -= 2f;
                State.Clamp();
                Talk(EnterBeat3,
                    C(Minji, "fc.b2.silence.1"),
                    C(Minji, "fc.b2.silence.2"),
                    T("fc.b2.silence.think"));
                return;
            }

            switch (id)
            {
                case "snark":
                    State.TriggerFired = true;
                    TouchEmotion(0.28f);
                    State.Pride -= 2;
                    State.Cooperation -= 1;
                    State.Clamp();
                    Talk(EnterBeat3,
                        C(Intern, "fc.b2.choice.snark.say"),
                        C(Minji, "fc.b2.snark.1"),
                        C(Minji, "fc.b2.snark.2"));
                    return;
                case "push":
                    State.Cooperation += 1;
                    State.Pride -= 1;
                    State.Clamp();
                    Talk(EnterBeat3,
                        C(Intern, "fc.b2.choice.push.say"),
                        C(Minji, "fc.b2.push.1"),
                        C(Intern, "fc.b2.push.2"),
                        C(Minji, "fc.b2.push.3"));
                    return;
                default:
                    State.Cooperation += 1;
                    State.KnowsOnlineRoute = true;
                    State.Clamp();
                    if (State.GuidePrepared)
                    {
                        Talk(EnterBeat3,
                            C(Intern, "fc.b2.choice.online.say"),
                            C(Intern, "fc.b2.online.guide"),
                            C(Minji, "fc.b2.online.1"),
                            C(Intern, "fc.b2.online.2"),
                            C(Minji, "fc.b2.online.3"));
                        return;
                    }

                    Talk(EnterBeat3,
                        C(Intern, "fc.b2.choice.online.say"),
                        C(Minji, "fc.b2.online.1"),
                        C(Intern, "fc.b2.online.2"),
                        C(Minji, "fc.b2.online.3"));
                    return;
            }
        }

        private void EnterBeat3()
        {
            _step = Step.Beat3;
            ArmTimedPrompt(EpisodePhase.Beat3, C(Minji, "fc.b3.line"), Loc.T("fc.b3.status"),
                Loc.Choice("now", "fc.b3.choice.now"),
                Loc.Choice("mask", "fc.b3.choice.mask"),
                Loc.Choice("cover", "fc.b3.choice.cover"),
                Loc.Choice("payhold", "fc.b3.choice.payhold", "fc.b3.choice.payhold_hint"));
        }

        private void ApplyBeat3(string id, InputKind kind)
        {
            Spend(1.0f);

            if (kind == InputKind.Silence)
            {
                State.Cooperation -= 1;
                State.Clamp();
                Talk(EnterBeat4,
                    C(Minji, "fc.b3.silence.1"),
                    C(Intern, "fc.b3.silence.you"),
                    Seen("fc.b3.silence.think"));
                return;
            }

            string youKey;
            string reactionKey;
            switch (id)
            {
                case "mask":
                    youKey = "fc.b3.choice.mask.say";
                    if (State.PrivacyRaised && !State.TriggerFired)
                    {
                        State.Cooperation += 2;
                        reactionKey = "fc.b3.react.mask_hit";
                    }
                    else
                    {
                        State.Cooperation += 1;
                        reactionKey = "fc.b3.react.mask";
                    }
                    break;
                case "cover":
                    State.Pride += 1;
                    State.Cooperation += 1;
                    youKey = "fc.b3.choice.cover.say";
                    reactionKey = "fc.b3.react.cover";
                    break;
                case "payhold":
                    State.PayHoldThreatUsed = true;
                    State.Cooperation += 2;
                    State.Pride -= 1;
                    youKey = "fc.b3.choice.payhold.say";
                    reactionKey = "fc.b3.react.payhold";
                    break;
                default:
                    if (State.Cooperation < 2) State.Cooperation += 1;
                    if (State.GuidePrepared || State.KnowsOnlineRoute) State.Cooperation += 1;
                    youKey = "fc.b3.choice.now.say";
                    reactionKey = "fc.b3.react.now";
                    break;
            }

            State.Clamp();
            Talk(EnterBeat4, C(Intern, youKey), C(Minji, reactionKey));
        }

        private void EnterBeat4()
        {
            State.TeamLeadPingShown = true;
            State.MinutesLeft = Math.Min(State.MinutesLeft, 2.5f);
            State.Clamp();

            _step = Step.Beat4;
            ArmTimedPrompt(EpisodePhase.Beat4, S(Park, "fc.b4.line"), Loc.T("fc.b4.status"),
                Loc.Choice("honest", "fc.b4.choice.honest"),
                Loc.Choice("stall", "fc.b4.choice.stall"),
                Loc.Choice("panic", "fc.b4.choice.panic"));
        }

        private void ApplyBeat4(string id, InputKind kind)
        {
            if (kind == InputKind.Silence)
            {
                Talk(() => Finish(EpisodeOutcome.Escalate), S(Park, "fc.b4.silence"));
                return;
            }

            switch (id)
            {
                case "panic":
                    Talk(() => Finish(EpisodeOutcome.Escalate),
                        C(Intern, "fc.b4.choice.panic.say"),
                        C(Minji, "fc.b4.panic.1"),
                        S(Park, "fc.b4.panic.2"));
                    return;
                case "honest":
                    if (State.Cooperation >= 2 && !State.TriggerFired)
                        State.Cooperation += 1;
                    else
                        State.Pride -= 1;
                    State.Clamp();
                    Talk(() => Finish(ResolveOutcome()), C(Intern, "fc.b4.choice.honest.say"));
                    return;
                default:
                    State.Clamp();
                    Talk(() => Finish(ResolveOutcome()), S(Intern, "fc.b4.choice.stall.say"));
                    return;
            }
        }

        // ---------- Outcome ----------

        private EpisodeOutcome ResolveOutcome()
        {
            if (State.TriggerFired && State.Cooperation < 2)
                return EpisodeOutcome.Failure;

            if (State.PayHoldThreatUsed && State.Pride < 0)
                return EpisodeOutcome.Ambiguous;

            if (State.Cooperation >= 2 && !State.TriggerFired)
                return EpisodeOutcome.MinSuccess;

            if (State.Cooperation >= 1)
                return EpisodeOutcome.Ambiguous;

            return EpisodeOutcome.Failure;
        }

        private void Finish(EpisodeOutcome outcome)
        {
            _step = Step.Outcome;
            _waitingInput = true;
            _messengerOpen = false;

            string titleKey;
            string bodyKey;
            switch (outcome)
            {
                case EpisodeOutcome.MinSuccess:
                    titleKey = "fc.out.success.title";
                    bodyKey = "fc.out.success.body";
                    break;
                case EpisodeOutcome.Ambiguous:
                    titleKey = "fc.out.ambiguous.title";
                    bodyKey = "fc.out.ambiguous.body";
                    break;
                case EpisodeOutcome.Escalate:
                    titleKey = "fc.out.escalate.title";
                    bodyKey = "fc.out.escalate.body";
                    break;
                default:
                    titleKey = "fc.out.fail.title";
                    bodyKey = "fc.out.fail.body";
                    break;
            }

            var title = Loc.T(titleKey);
            var body = Loc.T(bodyKey);
            _frame = new NegotiationFrame
            {
                Phase = EpisodePhase.Outcome,
                IsOutcome = true,
                Outcome = outcome,
                OutcomeTitle = title,
                OutcomeBody = body,
                Speaker = Loc.T("speaker.outcome"),
                Line = title + "\n\n" + body,
                StatusLine = State.DebugSummary(),
                PortraitId = Intern,
                PortraitPose = "front",
                LeftBackgroundId = "office_hr",
                RightBackgroundId = _partnerBackgroundId,
                ChatSession = _chatSession,
                Choices = new[]
                {
                    Loc.Choice("retry", "ui.retry"),
                    Loc.Choice("quit", "ui.quit")
                }
            };
        }

        // ---------- Dialogue plumbing ----------

        private static Line S(string who, string key) => new Line(who, key, Via.Speech);
        private static Line C(string who, string key) => new Line(who, key, Via.Chat);
        private static Line T(string key) => new Line(Intern, key, Via.Think);
        /// <summary>Intern's inner line at the moment the partner reads (unread "1" disappears).</summary>
        private static Line Seen(string key) => new Line(Intern, key, Via.Think, seen: true);

        /// <summary>Plays a short back-and-forth exchange, one line per frame, then continues.</summary>
        private void Talk(Action next, params Line[] lines) => TalkFrom(0, lines, next);

        private void TalkFrom(int index, Line[] lines, Action next)
        {
            if (index >= lines.Length)
            {
                next();
                return;
            }

            var line = lines[index];
            _step = Step.Bridge;
            _afterBridge = () => TalkFrom(index + 1, lines, next);
            _frame = BuildFrame(EpisodePhase.Beat1, line);
            _frame.AutoAdvance = true;
            _waitingInput = true;
        }

        private void ArmTimedPrompt(EpisodePhase phase, Line line, string statusPrefix, params ChoiceOption[] choices)
        {
            _waitingInput = true;
            _silenceHeld = 0f;
            _interruptHeld = 0f;
            _frame = BuildFrame(phase, line);
            _frame.StatusLine = statusPrefix + " · " + State.DebugSummary();
            _frame.AllowSilence = true;
            _frame.SilenceFailSeconds = EstimateReadSeconds(_frame.Line);
            _frame.AllowInterrupt = phase is EpisodePhase.Beat1 or EpisodePhase.Beat2 or EpisodePhase.Beat3 or EpisodePhase.Beat4;
            _frame.InterruptIsPositive = phase == EpisodePhase.Beat3;
            _frame.InterruptWindowSeconds = Math.Clamp(_frame.SilenceFailSeconds * 0.4f, 5f, 10f);
            _frame.Choices = choices;
        }

        private void TouchEmotion(float delta)
        {
            State.Emotion01 += delta;
            State.Clamp();
        }

        private NegotiationFrame BuildFrame(EpisodePhase phase, Line line)
        {
            if (line.Who != Intern) _talkingTo = line.Who;
            else if (line.Via == Via.Chat) _talkingTo = Minji;
            RememberPartnerPlace(_talkingTo);

            return new NegotiationFrame
            {
                Phase = phase,
                Speaker = SpeakerName(line.Who),
                Line = Loc.T(line.Key),
                PortraitId = line.Who,
                PortraitPose = PoseFor(line),
                ListenerPortraitId = line.Who == Intern ? _talkingTo : "",
                LeftBackgroundId = "office_hr",
                RightBackgroundId = _partnerBackgroundId,
                IsChat = line.Via == Via.Chat,
                MessengerOpen = _messengerOpen,
                ChatSession = _chatSession,
                ChatSeen = line.Seen,
                Choices = Array.Empty<ChoiceOption>()
            };
        }

        private static string SpeakerName(string who) =>
            who switch
            {
                Park => Loc.T("speaker.park"),
                Minji => Loc.T("speaker.minjae"),
                _ => Loc.T("speaker.intern")
            };

        /// <summary>At the keyboard while messaging; face each other when speaking in person.</summary>
        private string PoseFor(Line line) =>
            _messengerOpen && line.Via != Via.Speech && _talkingTo == Minji ? "chat" : "";

        private void RememberPartnerPlace(string portraitId)
        {
            if (string.IsNullOrEmpty(portraitId) || portraitId == Intern)
                return;
            _partnerBackgroundId = portraitId == Park ? "office_manager" : "office_dev";
        }

        private static float EstimateReadSeconds(string line)
        {
            var n = string.IsNullOrEmpty(line) ? 0 : line.Length;
            return Math.Clamp(12f + n * 0.09f, 14f, 45f);
        }

        private void Spend(float minutes)
        {
            State.MinutesLeft -= minutes;
            State.Clamp();
        }
    }
}
