# 06 · ASSETS

이미지·오디오 목록과 **파일 규칙**. 실제 파일은 `Assets/Resources/`.

두 Unity 카피 모두에 동일 반영:  
`c:\untitled-game\Assets\...` 와 `c:\untitled-game\untitled-game\Assets\...`

---

## 초상 (Portraits)

경로: `Assets/Resources/Portraits/{id}[_{pose}].png`  
캔버스: 키잉 결과 **864×1152**, 하단 앵커, 투명 배경.

### 세트 접두사

생성 원본(초록 배경 JPG): 에이전트 assets 폴더의 `{PREFIX}{name}.jpg`  
현재 `PREFIX = cb2_` → [`tools/key_chroma.py`](../../tools/key_chroma.py)

### 필수 파일 (현재)

| 파일 | 인물 | 포즈 |
|------|------|------|
| `intern.png` | 인턴 | 정면 |
| `intern_left.png` / `intern_right.png` | 인턴 | 좌/우 (한쪽 미러 가능) |
| `intern_chat.png` | 인턴 | 책상 타이핑 |
| `minjae.png` | 한민지 | 정면 |
| `minjae_left.png` / `minjae_right.png` | 한민지 | 좌/우 |
| `minjae_chat.png` | 한민지 | 책상·턱 괴기 (핸드폰 금지) |
| `park.png` | 박팀장 | 정면 |
| `park_left.png` / `park_right.png` | 박팀장 | 좌/우 |

스타일: ~2.5등신 SD, 굵은 외곽선, 뮤트 팔레트, 생성 시 **평평한 #00FF00**, 아우라·스티커 테두리 금지 (키잉 깨짐).

### 파이프라인

1. 캐릭터 정본([`02_CHARACTER_BIBLE.md`](02_CHARACTER_BIBLE.md)) + 스타일 레퍼런스로 생성  
2. `python tools/key_chroma.py`  
3. 양쪽 Portraits 폴더에 저장됐는지 확인  
4. Unity Refresh · 메신저 아바타 `uvRect`는 머리 비율 바뀌면 [`NegotiationBootstrap`](../../Assets/Scripts/UnityPresentation/NegotiationBootstrap.cs) 조정

---

## 배경 (Backgrounds)

경로: `Assets/Resources/Backgrounds/{id}.jpg` (16:9 권장)

| ID | 내용 |
|----|------|
| `office_hr` | 인사팀 — 서류 더미·캐비닛·낡은 모니터 |
| `office_dev` | 개발팀 — 듀얼 모니터·캔·과자·피규어 |
| `office_manager` | 팀장석 — 큰 책상·가죽 의자·트로피·달력 |
| `cafe` | 레거시. 새 장면 기본값으로 쓰지 말 것 |

인물 없이 배경만. 캐릭터는 초상 레이어.

---

## UI / 연출 (에셋이 아닌 코드)

분할 심·메신저·선택지 → [`07_PRESENTATION.md`](07_PRESENTATION.md)

---

## 오디오 (미구현 · 목록만)

| ID (예정) | 용도 |
|-----------|------|
| `sfx_chat_send` | 내 말풍선 전송 |
| `sfx_chat_recv` | 상대 말풍선 |
| `sfx_typing` | 입력 중 / 타닥 |
| `sfx_choice` | 선택 확정 |
| `bgm_office_tense` | 교섭 BGM (톤 오픈) |

파일 경로·포맷은 도입 시 이 표에 확정. 임의 추가 금지.

---

## Loc (텍스트 에셋)

| 경로 | 내용 |
|------|------|
| `Loc/{ko,en}/common.json` | 공통 UI·스피커명·상태 |
| `Loc/{ko,en}/ep_family_certificate.json` | 테스트 에피소드 |

키 규칙 → [`08_CONTENT_PIPELINE.md`](08_CONTENT_PIPELINE.md)
