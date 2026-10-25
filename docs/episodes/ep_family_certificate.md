# 가족관계증명서 (`ep_family_certificate`)

| | |
|--|--|
| 상태 | 프로토타입 / 플레이 가능 |
| 유형 | 테스트 |
| D-day | D-23 |
| Loc | `Assets/Resources/Loc/{ko,en}/ep_family_certificate.json` |
| C# | `FamilyCertificateEpisode` |
| 바이블 | [`../bible/`](../bible/) |

대사 원문 정본은 Loc. 이 문서는 **의도·플래그·비트·키**만.

---

## 1. 목표 / 비용

| | |
|--|--|
| 목표 | 입사 2주 차 **한민지**에게서 미제출 **가족관계증명서**를 받아낸다 |
| 매체 | 오프닝·Beat4 일부 = 대면 / 본편 = **사내 메신저 DM** |
| 입력 | Choice / Silence / Interrupt (비트 프롬프트, Beat3는 POSITIVE) |
| 시한 | 당일 **18:00 급여 마감** |
| 성공 비용/보상 | 마감 통과, 전환 미세 플러스 (칭찬 없음) |
| 실패 비용 | 전환 마이너스, 체면, escalate 시 무능 프레임 |

**스테이크:** 사안 = 서류 한 장 / 비용 = 전환·팀장·체면 (도시락형).

---

## 2. 인물·장소

| PortraitId | 역할 | BackgroundId |
|------------|------|--------------|
| `intern` | 플레이어 | `office_hr` (왼쪽) |
| `park` | HR 팀장 | `office_manager` |
| `minjae` | 신입 (주 교섭) | `office_dev` |

프로파일·역린 → [`../bible/02_CHARACTER_BIBLE.md`](../bible/02_CHARACTER_BIBLE.md)

---

## 3. 플래그·게이지 (이 에피소드)

공통 스키마 → [`../bible/05_GAME_SYSTEM.md`](../bible/05_GAME_SYSTEM.md)

사용: `Cooperation`, `Pride`, `Emotion01`(UI 10칸), `MinutesLeft`, `TriggerFired`, `KnowsOnlineRoute`, `GuidePrepared`, `ManagerEyeRaised`, `PrivacyRaised`, `PayHoldThreatUsed`, `TeamLeadPingShown`

---

## 4. 비트

### Opening (대면)

팀장: 증명서 아직? → 인턴 해명 → 팀장: 마감 전에 받아와.

### PreChoice

| id | 효과 |
|----|------|
| A | 바로 DM |
| B | 가이드 준비: 시간 −2, Coop +1, `GuidePrepared` |
| C | 팀장에게 부탁 → 거절, `ManagerEyeRaised`, 시간 −1 |

### StartChat

인턴 DM 2줄 → Seen → 민지 “아 그거요?”

### Beat 1 — 프라이버시 (`fc.b1.*`)

| 입력 | 효과 |
|------|------|
| rule | Coop +1, `PrivacyRaised` |
| family (역린) | Trigger, Pride −2, Coop −1 |
| money | Coop +1, Pride +1 |
| silence | Coop −1, `PrivacyRaised` |

→ 민지 Beat2 ask

### Beat 2 — 핑계 (`fc.b2.*`)

| 입력 | 효과 |
|------|------|
| online | Coop +1, `KnowsOnlineRoute` (+ 가이드면 링크 줄) |
| push | Coop +1, Pride −1 |
| snark (역린) | Trigger, Pride −2, Coop −1 |
| silence | 시간 −2 |

### Beat 3 — 거래 (`fc.b3.*`)

| 입력 | 효과 |
|------|------|
| now | Coop +1 (+1 if Guide/Online) |
| mask | Privacy+!Trigger → +2 else +1 |
| cover | Pride +1, Coop +1 |
| payhold | `PayHoldThreatUsed`, Coop +2, Pride −1 |
| silence | Coop −1 |

### Beat 4 — 팀장 (`fc.b4.*`)

| 입력 | 결과 |
|------|------|
| honest (DM) | 관계 괜찮으면 Coop +1 else Pride −1 → 판정 |
| stall (대면) | 안전 → 판정 |
| panic | 팀장 멘트를 민지 DM에 → Escalate |
| silence | Escalate |

### Outcome

| Outcome | 조건 (요약) |
|---------|-------------|
| MinSuccess | Coop ≥ 2, !Trigger |
| Ambiguous | 압박 후 Pride < 0 또는 Coop 1 등 |
| Failure | Trigger 후 Coop < 2, 또는 Coop 0 |
| Escalate | Beat4 panic/silence |

재시도: 메신저 직전, `ChatSession++`.

---

## 5. Loc 키 prefix

`fc.*` — 파일 `ep_family_certificate.json`

주요 그룹: `fc.open.*` · `fc.pre.*` · `fc.chat.*` · `fc.b1.*`…`fc.b4.*` · `fc.out.*` · `fc.restart_pending`

스피커·UI: `common.json` (`speaker.*`, `ui.chat.*`)

---

## 6. 스모크 루트 (`tools/Playtest --smoke`)

| 루트 | 기대 |
|------|------|
| B > rule > online > now > stall | MinSuccess |
| A > family > snark > payhold > honest | Ambiguous |
| C > money > push > mask > panic | Escalate |

---

## 7. 메모

- **서사 연결:** 시작 ep [`ep_opening.md`](ep_opening.md) (D-30)에서 온보딩 **서류 목록 확인** 예고 → 본 ep(D-23)는 가족관계증명서 **본** 수취. 오프닝 비트를 여기로 옮기지 말 것.
- 역린 선택지는 **함정** (보상처럼 보이게 쓰지 말 것)
- 성공해도 칭찬 없음
- 연출 → [`../bible/07_PRESENTATION.md`](../bible/07_PRESENTATION.md)
- 긴장 → [`../reference-tension-from-lunchbox.md`](../reference-tension-from-lunchbox.md)
