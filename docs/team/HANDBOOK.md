# Team HANDBOOK

정식 규칙. `MEMORY.md`에서 두 번 이상 나온 지적을 올린다.  
프로젝트 정본과 충돌하면 **바이블·project-context가 우선**.

---

## 항상

1. 플레이어 문자열 → Loc JSON만 (`Loc.T` / `Loc.Choice`). C# 하드코딩 금지.
2. 설정·톤·인물 → `docs/bible/`. 에피소드 비트 → `docs/episodes/ep_*.md`.
3. 미결정 항목 임의 확정 금지.
4. `c:\untitled-game\` 와 `untitled-game\untitled-game\` **양쪽** 스크립트·Resources 동기화.
5. 끼어들기·Emotion(10칸)은 시스템 정본(`05_GAME_SYSTEM` / `07_PRESENTATION`)을 따른다.
6. 의미 있는 진행이 끝나면 [`HISTORY.md`](HISTORY.md) **맨 아래**에 한 줄 append (시간순). 형식은 그 파일 상단.

---

## 리더

- 과제를 받으면 **코딩하지 않는다**. 방향 2~3개 선택지 → 사람 선택 → 배정.
- 배정 시 `INTENT.md`에: 원문 / 하지 말 것 / 결정 / 관련 바이블 경로.
- Unity 연출·스크린샷이 필요한 과제는 워커 1명·로컬 세션으로 묶는다.

## 워커

- `INTENT.md` 없는 구현 시작 금지.
- **사람(또는 리더)의 설계 OK 전** 코드·에셋 대량 수정 금지.
- 끝나면 `GATE.md` 다섯 칸을 직접 채운다.

## 리뷰

- `GATE.md`만 보고 통과/반려. 반려은 `어디서 / 무엇이 / 어떤 기준` 한 줄.
- 통과 전 merge·“다 됐음” 선언 금지.

## 독스

- 바이블·에피소드 md·템플릿만. 런타임 C#·Unity 씬 직접 수정 금지 (필요하면 리더가 코드 워커 배정).

## 디자인

- 초상·배경·`07_PRESENTATION` 결정. 키잉은 `tools/key_chroma.py`.
- 전 게임 UI를 참고 이미지 한 장으로 바꾸지 말 것 (PRESENTATION 정본).

---

## 관문 (GATE) 다섯 칸

| 칸 | 통과 기준 (이 프로젝트) |
|----|-------------------------|
| 테스트 | `dotnet run --project tools/Playtest -- --smoke` (해당 시) |
| 린트/키 | `python tools/check_loc_keys.py` (Loc 변경 시) |
| 화면 | Unity Play 또는 스크린샷으로 연출 확인 (UI 변경 시) |
| 문서 | bible / episode / INTENT와 모순 없음 |
| 보고 | 변경 파일·남은 리스크 짧게 |
