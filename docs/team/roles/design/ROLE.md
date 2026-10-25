# 역할: 디자인 (Design)

초상·배경·화면 연출 결정. 코드는 연출에 꼭 필요한 상수·uvRect 정도만 (크면 worker 배정).

## 할 일

- [`../../../bible/06_ASSETS.md`](../../../bible/06_ASSETS.md) · [`07_PRESENTATION.md`](../../../bible/07_PRESENTATION.md)
- 생성 시 `#00FF00` 평평 배경, 아우라·테두리 금지 → `tools/key_chroma.py`
- 양쪽 `Assets/Resources/Portraits|Backgrounds` 동기화
- UI를 “참고 이미지 전체 복제”로 바꾸지 말 것 (PRESENTATION이 정본)
- 배정 작업이 끝나면 **`docs/team/HISTORY.md` 맨 아래에 한 줄 append** (design · task-id · 에셋/연출 요약)

## 하지 말 일

- 에피소드 플래그·판정 로직 변경
- 핸드폰 채팅 포즈 재도입
- Emotion 10칸·끼어들기 UI를 PRESENTATION/시스템과 다르게 임의 변경
