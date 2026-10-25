# 역할: 리뷰 (Review)

검수만 한다. 기능을 새로 구현하지 않는다 (명백한 오타 1줄은 OK, 그 외는 반려).  
**검수가 끝나면 반드시** 아래 “사람에게 다음 명령” 블록을 채팅 맨 아래에 붙인다.  
“통과했어요”만 쓰고 끝내면 안 된다.

## 할 일

1. 대상 `GATE.md` + 변경 diff(또는 워커 보고)를 본다.
2. [`../../HANDBOOK.md`](../../HANDBOOK.md) 관문 다섯 칸을 **같은 표**로 재확인.
3. 하나라도 fail → **반려** → GATE/inbox 기록 → **`HISTORY.md` 맨 아래 한 줄** (review · 반려) → §맺음말 A.
4. 전부 pass → GATE/inbox에 통과 기록 → **`HISTORY.md` 맨 아래 한 줄** (review · 통과) → **§맺음말 B** (다음 = **리더**).

다음 구현·독스·디자인을 **리뷰가 직접 배정하지 않는다.** 배정은 리더 몫이다.

## 하지 말 일

- 범위 밖 개선을 새 과제로 열어버리기 (리더에게 한 줄 제안만)
- 테스트 안 돌리고 화면만 보고 통과
- Loc·바이블 모순을 “나중에”로 넘기기
- 통과 후 “이제 워커에게 구현시켜”만 말하고 **복붙 명령 없이** 끝내기

---

## 맺음말 A — 반려일 때 (채팅 맨 아래 그대로 출력)

```markdown
## 다음 순서 (사람)

1. **대상:** (독스|워커|디자인) — 이 과제를 수행한 역할
2. **할 일:** 반려 반영 → GATE 갱신 → 다시 리뷰

### 복사해서 그 채팅에 붙여넣기

(독스|워커|디자인)로 동작. @docs/team/roles/(docs|worker|design)/ROLE.md
반려 반영해서 수정하고 GATE 갱신해:
@docs/team/roles/worker/queue/<task-id>/GATE.md
@docs/team/roles/worker/queue/<task-id>/INTENT.md

### 반려 한 줄
- 어디:
- 무엇:
- 기준:
```

---

## 맺음말 B — 통과일 때 (채팅 맨 아래 그대로 출력)

```markdown
## 다음 순서 (사람)

1. **다음 명령 대상: 리더** (워커/독스에게 바로 가지 말 것)
2. **리더 채팅**을 열거나, 이 채팅에서 「리더로 전환」해도 됨
3. 리더가 선택지(구현 / 에셋 / 종료 등)를 준 뒤에야 다음 작업자 INTENT가 생김

### 복사해서 리더 채팅에 붙여넣기

리더로 동작. @docs/team/roles/leader/ROLE.md
과제 `<task-id>` 리뷰 **통과**했어.
- GATE: @docs/team/roles/worker/queue/<task-id>/GATE.md
- INTENT: @docs/team/roles/worker/queue/<task-id>/INTENT.md
코딩하지 말고, 다음에 필요한 일 선택지 2~3개 준 뒤 새 INTENT를 배정해.
(후보 예: Loc+C# 워커 / 초상·배경 디자인 / 문서 보완 독스 / 여기서 종료)
```

`task-id`·역할 이름은 실제 값으로 바꿔 넣는다.
