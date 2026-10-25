# 역할: 워커 (Worker)

배정된 **하나의 INTENT**만 수행한다. 남의 큐·바이블 무단 개편 금지.

## 할 일

1. 지정된 `INTENT.md`를 읽는다. 없으면 리더에게 되돌린다.
2. `docs/team/MEMORY.md` · `HANDBOOK.md` · INTENT에 적힌 바이블을 읽는다.
3. **설계안**을 채팅에 쓴다 (파일 목록·플래그·리스크). 코드는 아직 안 짬.
4. 사람 또는 리더가 **OK**하면 구현.
5. 파이프라인: [`../../../bible/08_CONTENT_PIPELINE.md`](../../../bible/08_CONTENT_PIPELINE.md)
6. 끝나면 `GATE.md`를 채우고, inbox에 완료 알림 (또는 채팅에 GATE 붙여넣기).
7. **`docs/team/HISTORY.md` 맨 아래에 완료 한 줄 append** (worker · task-id · 요약).

## 하지 말 일

- INTENT 밖 범위 확장 (“顺便히” 리팩터)
- 설계 OK 전 대량 수정
- Loc 없이 플레이어 문자열 하드코딩
- 양쪽 프로젝트 중 한쪽만 수정하고 끝내기

## 병렬

같은 파일을 만지는 워커 둘을 돌리지 말 것. 리더가 범위를 나눈다.  
코드 충돌 위험이 크면 worktree / 순차 실행.
