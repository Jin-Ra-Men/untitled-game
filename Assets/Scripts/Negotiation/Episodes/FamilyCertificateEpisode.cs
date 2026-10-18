using System;

namespace UntitledGame.Negotiation.Episodes
{
    /// <summary>
    /// 테스트 에피소드: 가족관계증명서. 근거 docs/test-episode-family-certificate.md
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

        private Step _step;
        private NegotiationFrame _frame = new NegotiationFrame();
        private float _lineElapsed;
        private float _silenceHeld;
        private bool _waitingInput;
        private bool _interruptWindowOpen;
        private string _pendingSystemLine = "";
        private Action? _afterBridge;

        public string Title => "가족관계증명서";
        public NegotiationState State { get; private set; } = new NegotiationState();
        public NegotiationFrame CurrentFrame => _frame;
        public bool IsFinished => _step == Step.Outcome;

        public void Start()
        {
            State = new NegotiationState { MinutesLeft = 14f };
            _pendingSystemLine =
                "박팀장: 한민재. 가족관계증명서. 내일까지. 아니, 오늘까지.\n" +
                "인턴이 연락해. 나 미팅 전에 체크리스트 닫을 거니까.\n" +
                "…뭐? \"집 어딘가\"라고? 그거 네 문제야. 가서 받아와.";
            ShowPreChoice();
        }

        public void RestartFromNegotiation()
        {
            var slack = State.SlackPrepared;
            var eye = State.ManagerEyeRaised;
            State = new NegotiationState
            {
                MinutesLeft = slack ? 12f : 14f,
                SlackPrepared = slack,
                ManagerEyeRaised = eye,
                Cooperation = slack ? 1 : 0
            };
            State.Clamp();
            _pendingSystemLine = "교섭 직전으로 돌아왔다. (오프닝·프리초이스 스킵)";
            EnterBeat1();
        }

        public void SubmitChoice(string choiceId)
        {
            if (!_waitingInput) return;
            _silenceHeld = 0f;

            if (_step == Step.Outcome)
            {
                if (choiceId == "retry") RestartFromNegotiation();
                return;
            }

            if (_step == Step.Bridge)
            {
                if (choiceId == "continue")
                {
                    var next = _afterBridge;
                    _afterBridge = null;
                    next?.Invoke();
                }

                return;
            }

            switch (_step)
            {
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
            if (!_waitingInput || !_frame.AllowInterrupt || !_interruptWindowOpen) return;
            if (_step == Step.Outcome || _step == Step.Bridge) return;
            _silenceHeld = 0f;

            switch (_step)
            {
                case Step.Beat1:
                    ApplyBeat1("interrupt", InputKind.Interrupt);
                    break;
                case Step.Beat2:
                    ApplyBeat2("interrupt", InputKind.Interrupt);
                    break;
                case Step.Beat3:
                    ApplyBeat3("interrupt", InputKind.Interrupt);
                    break;
            }
        }

        public void TickRealtime(float deltaSeconds)
        {
            if (!_waitingInput || _step == Step.Outcome || _step == Step.Bridge) return;

            _lineElapsed += deltaSeconds;
            if (_frame.AllowInterrupt)
                _interruptWindowOpen = _lineElapsed <= _frame.InterruptWindowSeconds;

            if (_frame.AllowSilence)
            {
                _silenceHeld += deltaSeconds;
                if (_silenceHeld >= _frame.SilenceFailSeconds)
                    SubmitSilence();
            }
        }

        private void ShowPreChoice()
        {
            _step = Step.PreChoice;
            _waitingInput = true;
            _interruptWindowOpen = false;
            _frame = new NegotiationFrame
            {
                Phase = EpisodePhase.PreChoice,
                Speaker = "오프닝",
                Line = _pendingSystemLine + "\n\n교섭 전. 어떻게 들어갈까?",
                StatusLine = "D-23 · 14:10 → 미팅 15:30 · " + State.DebugSummary(),
                Choices = new[]
                {
                    new ChoiceOption("A", "프로파일만 훑고 바로 연락한다."),
                    new ChoiceOption("B", "슬랙으로 서류 리스트 + 제출 링크를 먼저 보낸다.", "시간 −2 · 준비도 ↑"),
                    new ChoiceOption("C", "팀장에게 기한을 내일 아침으로 물어본다.", "거절 + 눈치 ↑")
                }
            };
            _pendingSystemLine = "";
        }

        private void ApplyPreChoice(string id)
        {
            switch (id)
            {
                case "B":
                    State.SlackPrepared = true;
                    State.MinutesLeft -= 2f;
                    State.Cooperation += 1;
                    _pendingSystemLine = "슬랙 전송 완료. 민재가 첨부 링크는 열어 둔 상태.";
                    break;
                case "C":
                    State.ManagerEyeRaised = true;
                    State.MinutesLeft -= 1f;
                    _pendingSystemLine = "박팀장: \"내일? 내가 미쳤냐. 오늘.\" 눈치 ↑";
                    break;
                default:
                    _pendingSystemLine = "프로파일: 트리거=세대론 / 약점=명찰·슬랙 / 체면=부모 호출.";
                    break;
            }

            State.Clamp();
            EnterBeat1();
        }

        private void EnterBeat1()
        {
            _step = Step.Beat1;
            ArmTimedPrompt(
                EpisodePhase.Beat1,
                "한민재",
                PrefixPending("어, 안녕하세요? 저 지금 카페인데요. 입사 관련이죠? 카페에서—"),
                "Beat 1 · 연결",
                allowSilence: true,
                allowInterrupt: true,
                interruptWindow: 2.2f,
                silenceFail: 2.8f,
                new ChoiceOption("A", "가족관계증명서만 남았어요. 오늘 중으로 주세요."),
                new ChoiceOption("B", "민재님 입사 준비 잘 되고 계세요?", "스몰톡 · 시간 −1"));
        }

        private void ApplyBeat1(string id, InputKind kind)
        {
            Spend(0.8f);

            if (kind == InputKind.Silence)
            {
                State.Cooperation -= 1;
                State.Clamp();
                Bridge(
                    "한민재",
                    "…여보세요? 끊긴 거예요?\n아 그거… 집에 있을 건데, 지금 밖에 있어서요. 내일 들고 가면 안 돼요?",
                    EnterBeat2);
                return;
            }

            if (kind == InputKind.Interrupt)
            {
                State.Cooperation += 1;
                State.Pride -= 1;
                State.Pressure = DeadlinePressure.Tight;
                State.Clamp();
                Bridge(
                    "당신 → 한민재",
                    "서류부터요.\n\n한민재: …네. 그거… 집에 있을 건데, 지금 밖에 있어서요. 내일 들고 가면 안 돼요?",
                    EnterBeat2);
                return;
            }

            if (id == "B")
            {
                State.Pride += 1;
                State.MinutesLeft -= 1f;
                State.Clamp();
                Bridge(
                    "한민재",
                    "아 네네 잘 되고 있어요~ 아 그거… 집에 있을 건데, 지금 밖에 있어서요. 내일 들고 가면 안 돼요?",
                    EnterBeat2);
                return;
            }

            State.Pressure = DeadlinePressure.Tight;
            State.Clamp();
            Bridge(
                "한민재",
                "아 그거… 집에 있을 건데, 지금 밖에 있어서요. 내일 들고 가면 안 돼요?",
                EnterBeat2);
        }

        private void EnterBeat2()
        {
            _step = Step.Beat2;
            ArmTimedPrompt(
                EpisodePhase.Beat2,
                "한민재",
                "이거 인터넷으로도 되죠? 근데 공동인증서가… 아 맞다 폰 바꿨는데—\n그리고 어제 메일은 봤는데, 첨부 리스트가 길어서 그냥… 스크롤하다가—",
                "Beat 2 · 변명 · 끼어들기로 루프 차단",
                allowSilence: true,
                allowInterrupt: true,
                interruptWindow: 3.5f,
                silenceFail: 4.5f,
                new ChoiceOption("push", "내일은 팀장님이 안 받아요. 오늘이요."),
                new ChoiceOption("mz", "MZ분들은 서류를 잘 안 보시더라고요.", "역린 · 함정"));
        }

        private void ApplyBeat2(string id, InputKind kind)
        {
            if (kind == InputKind.Silence)
            {
                State.GotPhoneInfo = true;
                State.MinutesLeft -= 2f;
                State.Clamp();
                Bridge("시스템", "민재가 끝까지 말했다. 정보: 폰을 바꿈. 시간 −2분.", EnterBeat3);
                return;
            }

            if (kind == InputKind.Interrupt)
            {
                State.Cooperation += 1;
                State.Clamp();
                Bridge(
                    "당신 → 한민재",
                    "정부24로 바로 발급 돼요. 카페에서도요.\n\n한민재: …아, 그거요? 지금요?",
                    EnterBeat3);
                return;
            }

            if (id == "mz")
            {
                State.TriggerFired = true;
                State.Pride -= 2;
                State.Cooperation -= 1;
                State.Clamp();
                Bridge(
                    "한민재",
                    "와 첫날부터 세대론이세요? ㅋㅋ 서류는 찾을게요. 태도 문제는 아닌 것 같은데요.",
                    EnterBeat3);
                return;
            }

            State.Cooperation += 1;
            State.Pride -= 1;
            State.Clamp();
            Bridge(
                "한민재",
                "…오늘이요? 하, 알겠어요. 그러면… 저녁에 집 가서 찾아서 내일 아침에 찍어서 보내면요?",
                EnterBeat3);
        }

        private void EnterBeat3()
        {
            _step = Step.Beat3;
            ArmTimedPrompt(
                EpisodePhase.Beat3,
                "한민재",
                "그러면… 저녁에 집 가서 찾아서 내일 아침에 찍어서 보내면요?",
                "Beat 3 · 거래",
                allowSilence: true,
                allowInterrupt: true,
                interruptWindow: 2.0f,
                silenceFail: 3.0f,
                new ChoiceOption("practical", "지금 정부24 → PDF. 링크 다시 보낼게요. 5분이면 됩니다."),
                new ChoiceOption("face", "잊으신 거 누가 알겠어요. 저만 체크리스트 닫으면 끝이에요."),
                new ChoiceOption("badge", "안 오면 입사식 명찰·슬랙 보류예요.", "약점 압박 · 애매 위험"),
                new ChoiceOption("parent", "어머니께 부탁하시면 제일 빨라요.", "체면 위험"));
        }

        private void ApplyBeat3(string id, InputKind kind)
        {
            Spend(1.0f);

            if (kind == InputKind.Silence)
            {
                State.Cooperation -= 1;
                State.Clamp();
                Bridge("한민재", "……그럼 저녁에 할게요.", EnterBeat4);
                return;
            }

            if (kind == InputKind.Interrupt)
            {
                State.Cooperation += 1;
                State.Clamp();
                Bridge(
                    "당신 → 한민재",
                    "아침이면 팀장 미팅 지나가요.\n\n한민재: …아. 맞다. 오늘이긴 해야 하는데.",
                    EnterBeat4);
                return;
            }

            string reaction;
            switch (id)
            {
                case "face":
                    State.Pride += 1;
                    State.Cooperation += 1;
                    reaction = "ㅋㅋ 그 말 들으니까 좀 덜 창피하네요. 지금 끊어보고 보낼게요.";
                    break;
                case "badge":
                    State.BadgeThreatUsed = true;
                    State.Cooperation += 2;
                    State.Pride -= 1;
                    reaction = "명찰요? …알겠어요. 지금 할게요. 근데 좀 과한 것 같긴 한데.";
                    break;
                case "parent":
                    State.ParentRouteUsed = true;
                    if (State.Pride <= 0 || State.TriggerFired)
                    {
                        State.Cooperation -= 1;
                        State.Pride -= 1;
                        State.Clamp();
                        Bridge(
                            "한민재",
                            "잠깐, 부모님까지요? 제가 애예요? …그냥 저녁에 할게요.",
                            EnterBeat4);
                        return;
                    }

                    State.Cooperation += 2;
                    reaction = "엄마한테… 아 됐어요. 제가 정부24로 할게요.";
                    break;
                default:
                    if (State.Cooperation < 2) State.Cooperation += 1;
                    if (State.SlackPrepared || State.GotPhoneInfo) State.Cooperation += 1;
                    reaction = "…알겠어요. 지금 해볼게요. 링크만요.";
                    break;
            }

            State.Clamp();
            Bridge("한민재", reaction, EnterBeat4);
        }

        private void EnterBeat4()
        {
            State.TeamLeadPingShown = true;
            State.MinutesLeft = Math.Min(State.MinutesLeft, 2.5f);
            State.Clamp();

            _step = Step.Beat4;
            ArmTimedPrompt(
                EpisodePhase.Beat4,
                "박팀장 (슬랙)",
                "한민재 서류. 미팅 전. 답만.",
                "Beat 4 · 마감임박 · 침묵하면 escalate",
                allowSilence: true,
                allowInterrupt: false,
                interruptWindow: 0f,
                silenceFail: 2.2f,
                new ChoiceOption("honest", "민재에게 사실대로: 팀장이 지금 재촉 중."),
                new ChoiceOption("stall", "팀장에게 \"받는 중\"만 회신하고 민재 교섭 계속."),
                new ChoiceOption("panic", "팀장·민재 동시 대응 (당황)"));
        }

        private void ApplyBeat4(string id, InputKind kind)
        {
            if (kind == InputKind.Silence || id == "panic")
            {
                Finish(EpisodeOutcome.Escalate);
                return;
            }

            if (id == "honest")
            {
                if (State.Cooperation >= 2 && !State.TriggerFired)
                    State.Cooperation += 1;
                else
                    State.Pride -= 1;
            }

            State.Clamp();
            Finish(ResolveOutcome());
        }

        private EpisodeOutcome ResolveOutcome()
        {
            if (State.TriggerFired && State.Cooperation < 2)
                return EpisodeOutcome.Failure;

            if (State.BadgeThreatUsed && State.Pride < 0)
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
            _interruptWindowOpen = false;

            string title;
            string body;
            switch (outcome)
            {
                case EpisodeOutcome.MinSuccess:
                    title = "★ 최소 성공 — PDF 입고";
                    body =
                        "민재가 카페에서 발급해 제출했다.\n박팀장: \"됐어.\" (칭찬 없음. 체크리스트는 닫힘.)\n전환 심사: 온보딩 클로징 작은 플러스.\n\n서류는 왔다. 자존심 상한 신입은 아직 안 왔다. 그건 내일 문제다.";
                    break;
                case EpisodeOutcome.Ambiguous:
                    title = "◆ 애매 — 오긴 왔는데";
                    body =
                        "서류는 오늘 23:40에 도착. 팀장 미팅에는 못 썼다.\n박팀장: \"미팅 전에 하라고 했지. 인턴 일정 감각이…\"\n전환 심사: 플러스·마이너스 상쇄. 팀장 눈치 잔여.";
                    break;
                case EpisodeOutcome.Escalate:
                    title = "✗✗ 팀장 escalate";
                    body =
                        "박팀장이 민재에게 직접 전화했다. 민재 불쾌.\n박팀장: \"내가 왜 신입한테 전화하냐. 너 전환 때 이 얘기 나온다.\"\n\n서류는 팀장이 받아냈다. 네 체면도 같이 가져갔다.";
                    break;
                default:
                    title = "✗ 실패 — 내일 들고 갈게요";
                    body =
                        "한민재: \"내일 입사할 때 지참할게요. 끊을게요.\"\n체크리스트 미닫힘. 전환 심사에 구멍.";
                    break;
            }

            _frame = new NegotiationFrame
            {
                Phase = EpisodePhase.Outcome,
                IsOutcome = true,
                Outcome = outcome,
                OutcomeTitle = title,
                OutcomeBody = body,
                Speaker = "결과",
                Line = title + "\n\n" + body,
                StatusLine = State.DebugSummary(),
                Choices = new[]
                {
                    new ChoiceOption("retry", "교섭 직전부터 다시 (학습 루프)"),
                    new ChoiceOption("quit", "종료")
                }
            };
        }

        private void Bridge(string speaker, string line, Action next)
        {
            _step = Step.Bridge;
            _waitingInput = true;
            _interruptWindowOpen = false;
            _afterBridge = next;
            _frame = new NegotiationFrame
            {
                Phase = EpisodePhase.Beat1,
                Speaker = speaker,
                Line = line,
                StatusLine = "계속 · " + State.DebugSummary(),
                Choices = new[] { new ChoiceOption("continue", "계속") }
            };
        }

        private void ArmTimedPrompt(
            EpisodePhase phase,
            string speaker,
            string line,
            string statusPrefix,
            bool allowSilence,
            bool allowInterrupt,
            float interruptWindow,
            float silenceFail,
            params ChoiceOption[] choices)
        {
            _waitingInput = true;
            _lineElapsed = 0f;
            _silenceHeld = 0f;
            _interruptWindowOpen = allowInterrupt;
            _frame = new NegotiationFrame
            {
                Phase = phase,
                Speaker = speaker,
                Line = line,
                StatusLine = statusPrefix + " · " + State.DebugSummary(),
                AllowSilence = allowSilence,
                AllowInterrupt = allowInterrupt,
                InterruptWindowSeconds = interruptWindow,
                SilenceFailSeconds = silenceFail,
                Choices = choices
            };
        }

        private string PrefixPending(string line)
        {
            if (string.IsNullOrEmpty(_pendingSystemLine)) return line;
            var p = _pendingSystemLine;
            _pendingSystemLine = "";
            return p + "\n\n" + line;
        }

        private void Spend(float minutes)
        {
            State.MinutesLeft -= minutes;
            State.Clamp();
        }
    }
}
