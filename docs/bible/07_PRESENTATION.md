# 07 · PRESENTATION

화면 연출 계약. 구현: [`NegotiationBootstrap.cs`](../../Assets/Scripts/UnityPresentation/NegotiationBootstrap.cs)  
스텐실: `SplitRegionGraphic` / `SplitSeamGraphic`

---

## 전체 레이아웃 (현재 확정)

- **Canvas** Screen Space Overlay, 기준 1920×1080, width-match
- **대각 분할(Juusei형)** 유지 — 노란 지그재그 심(seam)
- 심 중심: 왼쪽 발화 ≈ 0.64 / 오른쪽 ≈ 0.36 / 선택·아웃컴 ≈ 0.5
- 발화자 초상 크게(`SpeakHeight`), 청자 작게(`QuietHeight`) + 딤
- **Emotion**: 우측 하단 라벨 + **10칸** 세그먼트 바 (총성식). 채움 = 상대 열기
- **끼어들기**: 프롬프트 중 창이 열려 있으면 버튼(◎) + 키 `I`. `InterruptIsPositive`면 POSITIVE 표시

이 에피소드가 사무실·메신저여도, **전 게임 UI를 참고 이미지 한 장으로 통일하지 않는다**.  
참고 이미지 요소(상단 우세 바·타이머·채널 사이드바·①②③ 가로 바)는 **미도입**. 도입 시 이 문서에 먼저 적는다.

---

## 배경

- 왼쪽 = 인턴 측 (`LeftBackgroundId`, 기본 `office_hr`)
- 오른쪽 = 파트너 (`RightBackgroundId`: 팀장 `office_manager`, 민지 `office_dev`)
- 파트너가 바뀌면 에피소드가 `RememberPartnerPlace`로 ID 갱신

---

## 대면 텍스트

- 박스 없는 이름 + 밑줄 + 대사 (외곽선)
- 배경·큰 머리가 겹치면 **반투명 플레이트** (`PlateSpeaking` / `PlateListening`)
- 말하는 쪽 플레이트·텍스트 진하게, 듣는 쪽 옅게

---

## 메신저 모드

조건: `MessengerOpen` + 민지와의 비-Speech 라인 → pose `chat`

| 항목 | 동작 |
|------|------|
| 창 | 중앙 상단, 고정 폭·높이 |
| 상대 말 | “입력 중 •••” → 말풍선 팝인 |
| 인턴 말 | 입력창 타이핑 → 전송 버블 |
| 읽음 | unread “1” → `ChatSeen` 또는 상대 답으로 해제 |
| 세션 | `ChatSession` 변경 시 로그 클리어 |
| 초상 | `_chatBlend`로 x≈0.15 / 0.85, `ChatPortraitScale`로 축소 (책상 클로즈업) |
| 아바타 | 민지 얼굴 crop `uvRect` (머리 비율 바뀌면 재조정) |

사이드 텍스트 그룹은 메신저 폭을 피하도록 클램프.

---

## 선택지

- 하단 중앙 세로 스택, 흰 박스 + 번호(1–4)
- 숫자 키 1–4로 선택
- 침묵은 별도 UI 버튼 없이 타이머 / 콘솔 `S`

가로 ①②③ 한 줄 배치는 미도입.

---

## 페이싱

| | |
|--|--|
| 대면 타자기 | ~14자/초 |
| 채팅 | ~20자/초 |
| 홀드 | `HoldBase` + 글자×`HoldPerChar` |
| 화자 전환 | ~0.5s 딜레이 |
| 클릭 | 타자 스킵 또는 홀드 종료 |

---

## 금지·주의

- 핸드폰 포즈 초상 재도입 금지 (회사 내부 채팅 = 키보드)
- 연출 결정을 에피소드 md에만 쓰고 여기 안 적지 말 것 (이중 정본)
- Hub 프로젝트와 git 워크스페이스 **양쪽** 스크립트·에셋 동기화
