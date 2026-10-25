# 05 · GAME_SYSTEM

선택·침묵·플래그·게이지·결과. 코드 정본:

- [`NegotiationTypes.cs`](../../Assets/Scripts/Negotiation/Core/NegotiationTypes.cs)
- 에피소드 구현: `Assets/Scripts/Negotiation/Episodes/`

이 문서와 코드가 어긋나면 **코드를 고치거나 문서를 고친다**. 둘 다 “맞다”고 두지 말 것.

---

## 입력

| `InputKind` | 의미 |
|-------------|------|
| `Choice` | 번호 선택 (UI 1–4 / 콘솔 1–4) |
| `Silence` | 침묵. 타이머 만료 또는 플레이어가 S |
| `Interrupt` | 끼어들기. 창이 열려 있을 때 I / 버튼 |

프롬프트 중 `AllowSilence` + `SilenceFailSeconds`(줄 길이 추정).  
비트 프롬프트에 `AllowInterrupt` + `InterruptWindowSeconds` + `InterruptIsPositive`(POSITIVE 연출). 창이 닫히면 끼어들기 불가.

---

## 페이즈 (`EpisodePhase`)

테스트 에피소드 기준:

`Opening` → `PreChoice` → (`Beat1`…`Beat4`) → `Outcome` → `Done`

에피소드마다 비트 수는 달라질 수 있으나, 이름·순서는 해당 Episode 클래스와 Loc prefix와 맞출 것.

---

## 게이지·상태 (`NegotiationState`)

| 필드 | 범위 / 형 | 의미 |
|------|-----------|------|
| `Cooperation` | 0–3 | 상대 협조 (라이프에 가깝게) |
| `Pride` | −2…+2 | 상대 자존심 / 체면 |
| `Emotion01` | 0–1 | 상대 흥분·열기. UI는 **10칸** (`EmotionBar.Segments`) |
| `MinutesLeft` | ≥ 0 | 남은 분 (압박 계산) |
| `Pressure` | Relaxed / Tight / Critical | `MinutesLeft` ≤8 → Tight, ≤3 → Critical |
| `TriggerFired` | bool | 역린 발동 |
| `KnowsOnlineRoute` | bool | 온라인 발급 루트 인지 |
| `GuidePrepared` | bool | 교섭 전 가이드 준비 (PreChoice B) |
| `ManagerEyeRaised` | bool | 팀장 눈치 ↑ |
| `PrivacyRaised` | bool | 프라이버시 불만 기록 |
| `PayHoldThreatUsed` | bool | 수당 보류 압박 사용 |
| `TeamLeadPingShown` | bool | 팀장 핑 연출 여부 |

UI 칸 수 = `round(Emotion01 * 10)` 클램프 0–10. 역린·무례한 끼어들기로 상승, 순조·POSITIVE 끼어들기로 하락.

`Clamp()`이 매 갱신 시 범위를 고정한다.

---

## 프레임 (`NegotiationFrame`) — 표현이 읽는 계약

| 필드 | 용도 |
|------|------|
| `PortraitId` / `PortraitPose` | 초상. pose: `""`(자동 응시), `left`/`right`/`front`, `chat` |
| `ListenerPortraitId` | 인턴이 말하는 상대 → 파트너 초상 |
| `LeftBackgroundId` / `RightBackgroundId` | 분할 양쪽 배경 |
| `IsChat` / `MessengerOpen` / `ChatSession` / `ChatSeen` | 메신저 모드·세션·읽음 |
| `Choices` / `AllowSilence` / `SilenceFailSeconds` | 선택·침묵 |
| `AllowInterrupt` / `InterruptWindowSeconds` / `InterruptIsPositive` | 끼어들기 창 |
| `AutoAdvance` | 줄 종료 후 자동 다음 |
| `IsOutcome` / `Outcome` / `OutcomeTitle` / `OutcomeBody` | 결말 화면 |

Via(연출): Speech(대면) / Chat(말풍선) / Think(속마음) — 에피소드 `Line` 헬퍼.

---

## 아웃컴 (`EpisodeOutcome`)

| 값 | 의미 (가족관계증명서 기준) |
|----|---------------------------|
| `MinSuccess` | 협조 ≥ 2, 역린 미발동 |
| `Ambiguous` | 압박 후 자존심 < 0 또는 협조 1 등 |
| `Failure` | 역린 후 협조 < 2, 또는 협조 0 |
| `Escalate` | Beat4 침묵·동시 대응 등 → 팀장 직접 개입 |
| `None` | 진행 중 |

판정 식은 에피소드마다 다를 수 있다. 에피소드 md + C# Outcome 분기가 정본.

---

## 재시도

파탄 시 **교섭(메신저 시작) 직전**으로 리스타트.  
`ChatSession++` → 채팅 로그 클리어. 학습 루프.

---

## 콘솔 / 스모크

```bash
dotnet run --project tools/Playtest
dotnet run --project tools/Playtest -- --smoke
```

스모크는 에피소드당 대표 루트 3개 이상을 기대 아웃컴과 대조한다.  
새 에피소드 추가 시 Playtest 루트와 [`08_CONTENT_PIPELINE.md`](08_CONTENT_PIPELINE.md)를 갱신.
