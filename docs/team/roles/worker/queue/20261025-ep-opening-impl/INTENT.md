# INTENT

| | |
|--|--|
| task_id | `20261025-ep-opening-impl` |
| from | leader |
| role | worker |
| status | draft |

## 사람 원문

> N1 (Loc + C# 구현)

## 결정된 방향

- `ep_opening` 문서를 **플레이 가능**하게: Loc(ko/en) + `INegotiationEpisode` 구현 + Playtest/Unity에서 시작 가능.
- 스펙 정본: [`docs/episodes/ep_opening.md`](../../../../../episodes/ep_opening.md)  
  - Opening `office_manager` 대면(`park`) → Return `office_hr` 독백 → Tutorial DM(`park`, T1) · 사안 S3(온보딩 목록 확인 회신 지시; 민지 직접 교섭 없음).
- 입력: 문서대로 **Choice / Silence** (끼어들기는 이 ep에 넣지 않음. FC에 Interrupt가 있어도 오프닝 튜토리얼에는 강제 이식 금지).
- 조건식·게이지·시한: **`FamilyCertificateEpisode` 패턴 재사용** (Coop/Pride/MinutesLeft 등). 구체 시각은 문서 예(“오전에 보고”)에 가깝게 두고, 캠페인 메타 스탯은 신설하지 말 것.
- Unity 기본 진입: 프로토타입에서 **`ep_opening`을 시작 에피소드**로 연결. `ep_family_certificate`는 코드·Playtest에서 **유지**(스모크/전환 가능하면 유지, 삭제 금지).
- 파이프라인: [`docs/bible/08_CONTENT_PIPELINE.md`](../../../../../bible/08_CONTENT_PIPELINE.md). 문자열 하드코딩 금지 (`Loc.T` / `Loc.Choice` / `Loc.F`).

## 하지 말 것

- `ep_opening.md` 비트 의도·T1/S3·BackgroundId 규칙을 구현 중 임의 변경 (필요하면 독스 재배정)
- 회사명·미채번 캐릭터명 확정
- 캠페인 전환 점수 등 메타 스키마 하드코딩
- `ep_family_certificate` Loc/비트 대규모 리팩터 (공유 헬퍼 추출은 최소화·필요 시만)
- 새 초상·배경 에셋 생성 (기존 `park`/`intern`/`office_*` 사용)
- 사람 **설계 OK 전** 대량 본문 Loc·전체 비트 구현 (아래 설계안 먼저)

## 건드릴 범위

- 파일/폴더:
  - `Assets/Resources/Loc/{ko,en}/ep_opening.json` (신규) · 필요 시 `common.json` 키 **최소** 추가
  - `Assets/Scripts/Negotiation/Episodes/` — `OpeningEpisode`(또는 동등 이름, ID=`ep_opening`) : `INegotiationEpisode`
  - `Assets/Scripts/UnityPresentation/NegotiationBootstrap.cs` — 시작 ep 연결
  - `tools/Playtest/Program.cs` — 기본/선택으로 `ep_opening` + `--smoke` 루트
  - Unity Hub 쪽 `untitled-game/Assets/...` **동기화** (08 파이프라인)
  - 문서: `ep_opening.md` 상태 줄만 `초안`→`구현`/`플레이 가능` 등으로 갱신 가능. STORY 표 상태 한 칸 OK
- 바이블 참조: `02`·`03`·`05`·`07`·`08` · `ep_opening.md` · 기존 `FamilyCertificateEpisode` / `fc.*` Loc 구조

## 설계안 (구현 전 사람에게 OK)

짧게 채팅에:

1. Step 목록 (Opening / Return / Tut Connect·Push·Close / Outcome)과 재시도 지점
2. 플래그 사용 목록 (`OnboardingListAssigned` 등 — 문서 후보 중 실제 쓸 것)
3. 시한·게이지 초깃값 (FC와 뭐를 같게/다르게)
4. Bootstrap/Playtest에서 ep 전환 방식 (기본 opening, fc는 플래그·인자·임시 스위치 중 무엇)

**OK 받기 전** Loc 장문·전체 C# 금지. OK 후 구현.

## 완료 정의

1. ko/en `ep_opening.json` 키 정합 (`op.open.*` / `op.return.*` / `op.tut.*` / `op.out.*` 등 문서 prefix)
2. Episode 클래스가 비트·BackgroundId·매체(대면/독백/메신저)를 문서와 맞게 진행
3. Playtest: opening 플레이 + `--smoke`에 opening 최소 성공 루트 ≥1 (가능하면 문서 스모크 3중 핵심)
4. `python tools/check_loc_keys.py` 통과
5. Unity에서 시작 시 opening이 뜨고 콘솔 에러 없음 (가능하면 MCP 스크린샷 1장)
6. GATE 채움 · HISTORY 한 줄 append

## 검증

- [ ] Playtest smoke
- [ ] check_loc_keys
- [ ] Unity/화면
- [ ] 문서 모순 없음 (`ep_opening` ↔ 구현 ID ↔ Loc 파일명)
