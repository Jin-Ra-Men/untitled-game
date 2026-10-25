# D-30 오프닝 (`ep_opening`)

| | |
|--|--|
| 상태 | 초안 |
| 유형 | 본편 (시작) |
| D-day | **D-30** |
| Loc | `Assets/Resources/Loc/{ko,en}/ep_opening.json` (미작성 · 키 자리만) |
| C# | *(미구현)* |
| 바이블 | [`../bible/`](../bible/) |

대사 원문 정본은 Loc. 이 문서는 **의도·플래그·비트·키·장소**만.  
사람 확정: 튜토리얼 상대 **T1=`park`** · 사안 **S3=온보딩 서류 목록 확인 회신(민지 예고, `ep_family_certificate`와 사안 분리)**.

---

## 1. 목표 / 비용

| | |
|--|--|
| 목표 (오프닝) | 팀장 브리핑으로 **D-30·인턴·전환 압박**을 플레이어가 자연히 파악 |
| 목표 (튜토리얼) | 팀장(`park`)의 짧은 압박 교섭을 넘기며 **Choice / Silence**에 익숙해진다. 사안 축 = **온보딩 서류 목록**을 민지에게 확인해 회신 받으라는 지시(본편 증명서 수취와 별개) |
| 매체 | Opening = **대면** / Return = **독백** / Tutorial = **사내 메신저** (플레이어 자리 기준) |
| 입력 | Choice / Silence / Interrupt (구현 시 시스템 정본 따름) |
| 시한 | 튜토리얼: 당일 **짧은 창** (구체 시각은 구현 시 오픈 · 예: “오전에 보고”) |
| 성공 비용/보상 | 지시 수락·진행 인정. 칭찬 없음 (“됐어.”) |
| 실패 비용 | 팀장 눈치↑, 전환 미세 마이너스 감각, escalate 시 무능 프레임 |

**스테이크:** 사안 = 온보딩 목록 확인 회신(하찮음) / 비용 = 전환·팀장·체면 (도시락형).  
**주의:** 가족관계증명서 본수취는 [`ep_family_certificate.md`](ep_family_certificate.md) (D-23). 여기서는 목록 확인·예고만.

---

## 2. 인물·장소

| PortraitId | 역할 | BackgroundId (이 ep에서) |
|------------|------|--------------------------|
| `intern` | 플레이어 | `office_hr` (독백·메신저 시 자리) |
| `park` | HR 팀장 · 오프닝 대면 · **튜토리얼 주 교섭** | `office_manager` (대면) / 메신저 시 플레이어 측 `office_hr` |
| `minjae` | 개발 신입 · **사안 대상(회신)** · 튜토리얼에서 직접 교섭하지 않음 | *(등장 시)* `office_dev` — 비트에 넣지 않음(예고·플래그만). 확정 투입 시 비트에 BackgroundId 명시 |

프로파일·역린 → [`../bible/02_CHARACTER_BIBLE.md`](../bible/02_CHARACTER_BIBLE.md)  
배경 ID → [`../bible/03_WORLD_BIBLE.md`](../bible/03_WORLD_BIBLE.md)

---

## 3. 플래그·게이지 (이 에피소드 · 초안)

공통 스키마 → [`../bible/05_GAME_SYSTEM.md`](../bible/05_GAME_SYSTEM.md)  
캠페인 메타 신설 금지. 아래는 **에피소드 로컬 후보**.

| 후보 | 의미 |
|------|------|
| `Cooperation` / `Pride` / `MinutesLeft` / `TriggerFired` | 튜토리얼 구간에서 기존 스키마 재사용 가능 |
| `ManagerEyeRaised` | 팀장 눈치↑ (기존 키 재사용 후보) |
| `OnboardingListAssigned` | 온보딩 목록 확인 지시 수령 |
| `OnboardingListChased` | 민지에게 확인을 시도·예고함 (본편 fc와 별 플래그) |

수치·조건식은 구현 전 **오픈**. 비트 의도만 고정.

---

## 4. 비트 시트

**규칙:** 모든 비트에 `BackgroundId` + 매체(대면 / 메신저 / 독백)를 적는다.

### Opening — 팀장 브리핑

| | |
|--|--|
| BackgroundId | `office_manager` |
| 매체 | 대면 |
| Portrait | `park` (+ `intern`) |
| Loc | `op.open.*` |

의도 (정보 덤프 금지 · 지시·압박 톤):

1. 팀장: 전환까지 **한 달(D-30)** · “결과만”
2. 인턴: 짧은 수긍/해명 (선택 전 연출 또는 1선택)
3. 팀장: **온보딩 서류 목록**, 민지한테 **확인 회신** 받아 와 · 오늘 중 감
4. → 자리로 가라

→ `OnboardingListAssigned`

### Return — 자리 복귀

| | |
|--|--|
| BackgroundId | `office_hr` |
| 매체 | 독백 |
| Portrait | `intern` |
| Loc | `op.return.*` |

의도: 처지 한 줄 정리(전환·팀장·할 일) · 메신저 켤 준비. 소설형 장문 금지.

### Tutorial — 팀장 메신저 압박 (T1)

| | |
|--|--|
| BackgroundId | `office_hr` (플레이어 자리 기준) |
| 매체 | 사내 메신저 DM |
| Portrait | `park` (상대 말풍선) · `intern` (내 말) |
| Loc | `op.tut.*` |
| 주 교섭 | **`park`** (확정 T1) |
| 사안 | 온보딩 목록 확인 진행 여부 / 회신 언제 (S3). 민지는 **직접 비트 상대 아님** |

비트 골격 (초안 · 조건식 오픈):

| 비트 | 의도 | 입력 감각 |
|------|------|-----------|
| Tut Connect | 팀장 DM: “목록 확인은?” | 진행중 / 지금 보냄 / 침묵 |
| Tut Push | 변명·지연에 압박 | 실무 보고 / 과도한 해명(함정 후보) / 침묵 |
| Tut Close | “오전에 보고”류 마감 | 수락 / 다시 미루기 / 침묵 → 판정 |

**역린(함정) 후보:** 팀장 앞에서 과정 장황 설명·책임 전가 (박팀장 성향: 과정=변명). 보상처럼 보이게 쓰지 말 것.

민지 회신 본문·장문 DM은 이 ep에서 **확정하지 않음**. 성공 시 `OnboardingListChased` 정도만 남겨 D-23으로 이어질 수 있게.

### Outcome

| Outcome | 의도 (요약) |
|---------|-------------|
| MinSuccess | 팀장 압박 통과 · 목록 chase 착수 인정 · 칭찬 없음 |
| Ambiguous | 애매한 보고 · 눈치만 남음 |
| Failure | 침묵·변명 과다 · Coop/인상 악화 |
| Escalate | 팀장 직접 개입·무능 프레임 (튜토리얼에서도 가능하되 짧게) |

재시도: 메신저 직전(Tut Connect 전) 후보.

---

## 5. Loc 키 prefix

`op.*` — 파일 `ep_opening.json` (미작성)

| 그룹 | 용도 |
|------|------|
| `op.open.*` | Opening 대면 |
| `op.return.*` | 자리 독백 |
| `op.tut.*` | 튜토리얼 DM (`op.tut.connect` / `push` / `close` 등) |
| `op.out.*` | Outcome |
| `op.restart_pending` | 재시도 (있으면) |

스피커·UI: `common.json` (`speaker.*`, `ui.chat.*`)  
**대사 전문은 이 md에 쓰지 않음.**

---

## 6. 스모크 루트

구현·Playtest 이전. 자리만:

| 루트 | 기대 (초안) |
|------|-------------|
| (수긍 진행) → 실무 보고 → 수락 | MinSuccess |
| (해명) → 침묵 과다 | Ambiguous / Failure |
| 책임 전가 → escalate 입력 | Escalate |

---

## 7. 메모

- 서사 다음: D-23 [`ep_family_certificate.md`](ep_family_certificate.md) — 증명서 **본** 수취. 오프닝의 목록 확인은 예고·씨앗만.
- 톤: 정보 나열 VN 금지. 팀장 애교 압박 × 하찮은 목록.
- 긴장: [`../reference-tension-from-lunchbox.md`](../reference-tension-from-lunchbox.md)
- 연출: [`../bible/07_PRESENTATION.md`](../bible/07_PRESENTATION.md)
- 구현 시 파이프라인: [`../bible/08_CONTENT_PIPELINE.md`](../bible/08_CONTENT_PIPELINE.md)
