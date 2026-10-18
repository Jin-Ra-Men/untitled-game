# 개발 프로토타입

가족관계증명서 교섭 루프를 먼저 검증하는 슬라이스.

## 지금 플레이 (Unity 없이)

이 PC에 Unity가 없어도, 교섭 Core는 .NET 콘솔로 바로 돌릴 수 있다.

```bash
dotnet run --project tools/Playtest
```

조작: `1~4` 선택 · `S` 침묵 · `I` 끼어들기 · `Q` 종료 · 대기 시 침묵 자동.

## Unity (2D / C#)

- 에디터: **Unity 2022.3 LTS** (winget: `Unity.Unity.2022`, Hub: `Unity.UnityHub`)
- Unity Hub → **Open** → 이 저장소 루트(`c:\untitled-game`) 선택
- 첫 오픈 시 패키지/메타 생성이 끝날 때까지 기다린 뒤 Play
- `Assets/Scripts/Negotiation/` — 엔진 비의존 Core + 에피소드
- `Assets/Scripts/UnityPresentation/NegotiationBootstrap.cs` — 플레이 모드 시 UI 자동 생성

스택 본체는 Unity다. 콘솔은 **로직 검증용 하네스**일 뿐, 출시 런타임이 아니다.

## 검증 포인트

`docs/reference-tension-from-lunchbox.md` / 테스트 에피소드 메모와 동일:

- 끼어들기가 변명 루프를 끊는지
- MZ 역린이 함정인지
- 성공해도 팀장 칭찬이 없는지
- 실패 후 교섭 직전 재시도가 되는지
