# 08 · CONTENT_PIPELINE

에피소드·텍스트·초상을 **추가할 때의 계약**. AI/사람이 같은 순서를 따른다.

---

## 다국어 (필수)

- 플레이어에게 보이는 문자열을 C#에 하드코딩하지 말 것
- 경로: `Assets/Resources/Loc/{lang}/*.json` (`ko`, `en`)
- API: `Loc.T` / `Loc.Choice` / `Loc.F`
- 키 누락 검사: `python tools/check_loc_keys.py`

공통 UI·스피커: `common.json`  
에피소드: `ep_{id}.json` (예: `ep_family_certificate.json`)

키 prefix 예: 가족관계증명서 → `fc.*`

---

## 새 에피소드 작성 순서

```text
1. docs/bible 확인 (캐릭터·세계·시스템·연출)
2. docs/episodes/ep_{id}.md 작성 (비트·플래그·키 목록 — 대사 전문 X)
3. 04_STORY.md 표에 한 줄 추가
4. C# : INegotiationEpisode 구현 → Episodes/
5. Loc ko/en JSON (같은 키)
6. 필요 시 Portraits / Backgrounds
7. tools/Playtest 스모크 루트 추가
8. python tools/check_loc_keys.py
9. Unity Play + (가능하면) MCP 스크린샷
10. 양쪽 Assets/Scripts · Resources 동기화
```

문서 ID · Loc 파일명 · C# 클래스/상수 · Playtest 라우트가 **같은 `ep_*` ID**를 쓴다.

---

## 에피소드 md 최소 섹션

1. 목표 / 실패 조건 / 플레이어 비용  
2. 인물 · 장소 · `BackgroundId`  
3. 비트 표: 입력(choice/silence) → 플래그·게이지 → 다음  
4. Loc 키 prefix + 주요 키 목록  
5. 스모크 루트 ≥ 3 (기대 `EpisodeOutcome`)

양식: [`../templates/episode-template.md`](../templates/episode-template.md)  
목록: [`../episodes/README.md`](../episodes/README.md)

---

## 초상 파이프라인

1. `#00FF00` 평평한 배경으로 생성 (아우라·테두리 금지)  
2. `tools/key_chroma.py`의 `PREFIX` / `SOURCES` / `MIRRORS` 갱신  
3. `python tools/key_chroma.py` → 양쪽 `Portraits/`  
4. chat 포즈·uvRect·`ChatPortraitScale` 확인 ([`07_PRESENTATION.md`](07_PRESENTATION.md))

---

## 두 프로젝트 동기화

| 경로 | 역할 |
|------|------|
| `c:\untitled-game\` | git 워크스페이스 |
| `c:\untitled-game\untitled-game\` | Unity Hub가 여는 프로젝트 |

스크립트·Resources·docs 변경 시 **양쪽 복사**. Hub 쪽만 고치면 git에 안 남을 수 있음.

---

## 검증 체크리스트

- [ ] `dotnet run --project tools/Playtest -- --smoke` 통과  
- [ ] `python tools/check_loc_keys.py` 통과  
- [ ] Unity 콘솔 에러 없음  
- [ ] 미결정 항목을 임의 확정하지 않음 ([`01_GAME_BIBLE.md`](01_GAME_BIBLE.md))  
- [ ] 플레이어 문자열 하드코딩 없음  

---

## 관련 도구

| 도구 | 용도 |
|------|------|
| `tools/Playtest` | 콘솔 플레이·스모크 |
| `tools/check_loc_keys.py` | Loc 키 대칭 |
| `tools/key_chroma.py` | 초상 크로마키 |
| Unity MCP | refresh / play / screenshot ([`../unity-mcp-setup.md`](../unity-mcp-setup.md)) |
