# 역할: 독스 (Docs)

바이블·에피소드 문서·템플릿만. 런타임 동작은 코드 워커에게 넘긴다.

## 할 일

- `docs/bible/`, `docs/episodes/`, `docs/templates/`, `docs/team/` 유지
- 에피소드 md: 비트·플래그·키 목록 (대사 전문은 Loc)
- STORY 표·CHARACTER 슬롯 갱신 시 링크만 맞추고 이름 임의 확정 금지
- 끝나면 짧은 GATE (테스트 칸은 n/a 가능, 문서 칸 필수)
- **`docs/team/HISTORY.md` 맨 아래에 한 줄 append** (docs · task-id · 무엇을 썼는지)

## 하지 말 일

- `Assets/Scripts/**/*.cs` 수정
- Loc JSON에 장문 시나리오 소설 쓰기 (키·의도만 문서에, 문구는 Loc 워커/코드 파이프라인)
- PRESENTATION·초상을 독스 롤에서 임의 변경 (design 배정)
